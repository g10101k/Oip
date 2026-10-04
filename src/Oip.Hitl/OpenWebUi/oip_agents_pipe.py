"""
title: OIP Agents
author: OIP
version: 0.1.0
description: Agents of the OIP agent gateway (Oip.Hitl): tool calls are shown as statuses, questions of the agent and
  approvals of tool calls as dialogs.
"""

# Open WebUI pipe of the agent gateway of Oip.Hitl, see src/Oip.Hitl/README.md.
#
# The models are the agents of GET /v1/models, read with the service account of the Open WebUI client in Keycloak,
# because Open WebUI lists the models without a user. A chat message is sent to POST /v1/chat/completions with the
# Keycloak token of the user and "oip_events": true, so the gateway streams the statuses and the user steps of the run
# as "oip" events next to the answer. A user step is shown as a dialog while the answer keeps streaming: the run waits
# until the user answers, here or on the task page of OIP, or the step times out.

import asyncio
import json
import logging
import os
import time
from typing import Any, AsyncGenerator, Awaitable, Callable, Optional

import aiohttp
from pydantic import BaseModel, Field

log = logging.getLogger(__name__)

EventEmitter = Callable[[dict], Awaitable[None]]
EventCall = Callable[[dict], Awaitable[Any]]


class Pipe:
    class Valves(BaseModel):
        GATEWAY_URL: str = Field(
            default="",
            description="Base URL of the gateway API, e.g. https://oip-hil.example.com/v1; OIP_GATEWAY_URL when empty.",
        )
        OPENID_CONFIGURATION_URL: str = Field(
            default="",
            description="OpenID configuration of the Keycloak realm; OPENID_PROVIDER_URL when empty.",
        )
        CLIENT_ID: str = Field(
            default="",
            description="Keycloak client whose service account lists the agents; OAUTH_CLIENT_ID when empty.",
        )
        CLIENT_SECRET: str = Field(
            default="",
            description="Secret of the client; OAUTH_CLIENT_SECRET when empty.",
            json_schema_extra={"input": {"type": "password"}},
        )

    def __init__(self):
        self.valves = self.Valves()
        # Prefix of the model names in Open WebUI.
        self.name = "OIP: "
        self._service_token: Optional[str] = None
        self._service_token_expires = 0.0

    async def pipes(self) -> list[dict]:
        try:
            token = await self._get_service_token()
            async with aiohttp.ClientSession(timeout=aiohttp.ClientTimeout(total=30)) as session:
                async with session.get(f"{self._gateway_url()}/models", headers=_auth(token)) as response:
                    response.raise_for_status()
                    models = (await response.json())["data"]
            return [{"id": model["id"], "name": model.get("name") or model["id"]} for model in models]
        except Exception as e:
            log.error(f"OIP agents are not available: {e}")
            return []

    async def pipe(
        self,
        body: dict,
        __oauth_token__: Optional[dict] = None,
        __event_emitter__: Optional[EventEmitter] = None,
        __event_call__: Optional[EventCall] = None,
    ) -> str | AsyncGenerator[dict, None]:
        token = (__oauth_token__ or {}).get("access_token")
        if not token:
            return "Error: sign in to Open WebUI with OIP to use the agents."

        payload = {**body, "model": body["model"].split(".", 1)[-1]}
        if not body.get("stream"):
            return await self._complete(payload, token)
        return self._stream({**payload, "oip_events": True}, token, __event_emitter__, __event_call__)

    async def _complete(self, payload: dict, token: str) -> str:
        async with aiohttp.ClientSession(timeout=aiohttp.ClientTimeout(total=None, sock_connect=30)) as session:
            async with session.post(
                f"{self._gateway_url()}/chat/completions", json=payload, headers=_auth(token)
            ) as response:
                data = await response.json(content_type=None)
                if response.status >= 400:
                    return f"Error: {_error_message(data)}"
                return data["choices"][0]["message"]["content"]

    async def _stream(
        self,
        payload: dict,
        token: str,
        emit: Optional[EventEmitter],
        call: Optional[EventCall],
    ) -> AsyncGenerator[dict, None]:
        dialogs: list[asyncio.Task] = []
        last_status: Optional[str] = None
        # The run may wait long for the user, so only connecting is timed out.
        timeout = aiohttp.ClientTimeout(total=None, sock_connect=30)
        try:
            async with aiohttp.ClientSession(timeout=timeout) as session:
                async with session.post(
                    f"{self._gateway_url()}/chat/completions", json=payload, headers=_auth(token)
                ) as response:
                    if response.status >= 400:
                        data = await response.json(content_type=None)
                        yield _text_chunk(f"Error: {_error_message(data)}")
                        return

                    async for raw in response.content:
                        line = raw.decode("utf-8").strip()
                        if not line.startswith("data:"):
                            continue
                        data = line[len("data:"):].strip()
                        if data == "[DONE]":
                            break
                        chunk = json.loads(data)
                        if "error" in chunk:
                            yield _text_chunk(f"\n\nError: {_error_message(chunk)}")
                            continue

                        event = chunk.get("oip")
                        if event is None:
                            yield chunk
                        elif event["type"] == "status":
                            last_status = event.get("text") or ""
                            await _emit_status(emit, last_status, done=False)
                        elif event["type"] == "user_step":
                            # Not kept as the last status: the run goes on after the answer.
                            await _emit_status(emit, f"⏸ {event.get('title')}", done=False)
                            if call is not None:
                                dialogs.append(asyncio.create_task(self._ask(event, token, emit, call)))
        finally:
            # A dialog left open is answered on the task page or has timed out; the run is over anyway.
            for dialog in dialogs:
                dialog.cancel()
            if last_status is not None:
                await _emit_status(emit, last_status, done=True)

    async def _ask(self, event: dict, token: str, emit: Optional[EventEmitter], call: EventCall) -> None:
        title = event.get("title") or "Agent"
        message = event.get("description") or ""
        outcomes = event.get("outcomes") or []
        try:
            if event.get("kind") == "approval" and len(outcomes) == 2:
                # The first outcome allows the call, the second denies it.
                reply = await call({"type": "confirmation", "data": {"title": title, "message": message}})
                if isinstance(reply, dict):
                    return
                result = outcomes[0] if reply is True else outcomes[1]
            else:
                data = {"title": title, "message": message, "placeholder": "Your answer"}
                if outcomes:
                    data |= {"type": "select", "options": outcomes}
                reply = await call({"type": "input", "data": data})
                if isinstance(reply, dict):
                    # The browser tab is closed; the step is still answered on the task page.
                    return
                result = reply if isinstance(reply, str) else ""

            url = f"{self._gateway_url()}/agent-runs/{event['run_id']}/steps/{event['step_id']}/complete"
            async with aiohttp.ClientSession(timeout=aiohttp.ClientTimeout(total=30)) as session:
                async with session.post(url, json={"result": result}, headers=_auth(token)) as response:
                    if response.status == 404:
                        await _notify(emit, "warning", "The question is already answered or has expired.")
                    elif response.status >= 400:
                        data = await response.json(content_type=None)
                        await _notify(emit, "error", _error_message(data))
        except asyncio.CancelledError:
            raise
        except Exception as e:
            log.exception("Failed to answer an OIP agent step")
            await _notify(emit, "error", f"The answer is not sent: {e}")

    async def _get_service_token(self) -> str:
        if self._service_token and time.monotonic() < self._service_token_expires:
            return self._service_token

        configuration_url = self.valves.OPENID_CONFIGURATION_URL or os.getenv("OPENID_PROVIDER_URL", "")
        client_id = self.valves.CLIENT_ID or os.getenv("OAUTH_CLIENT_ID", "")
        client_secret = self.valves.CLIENT_SECRET or os.getenv("OAUTH_CLIENT_SECRET", "")
        async with aiohttp.ClientSession(timeout=aiohttp.ClientTimeout(total=30)) as session:
            async with session.get(configuration_url) as response:
                response.raise_for_status()
                token_endpoint = (await response.json())["token_endpoint"]
            async with session.post(
                token_endpoint,
                data={"grant_type": "client_credentials", "client_id": client_id, "client_secret": client_secret},
            ) as response:
                response.raise_for_status()
                token = await response.json()

        self._service_token = token["access_token"]
        self._service_token_expires = time.monotonic() + max(token.get("expires_in", 60) - 30, 0)
        return self._service_token

    def _gateway_url(self) -> str:
        return (self.valves.GATEWAY_URL or os.getenv("OIP_GATEWAY_URL", "")).rstrip("/")


def _auth(token: str) -> dict:
    return {"Authorization": f"Bearer {token}"}


def _text_chunk(text: str) -> dict:
    return {"choices": [{"index": 0, "delta": {"content": text}}]}


def _error_message(data: Any) -> str:
    if isinstance(data, dict) and isinstance(data.get("error"), dict):
        return data["error"].get("message") or str(data["error"])
    return str(data)


async def _emit_status(emit: Optional[EventEmitter], description: str, done: bool) -> None:
    if emit is not None:
        await emit({"type": "status", "data": {"description": description, "done": done}})


async def _notify(emit: Optional[EventEmitter], kind: str, content: str) -> None:
    if emit is not None:
        await emit({"type": "notification", "data": {"type": kind, "content": content}})
