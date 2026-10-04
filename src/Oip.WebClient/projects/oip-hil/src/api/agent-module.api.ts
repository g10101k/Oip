/* eslint-disable */
/* tslint:disable */
// @ts-nocheck

import { Injectable } from "@angular/core";
import { ContentType, HttpClient, RequestParams } from "oip-common";
import {
  AgentDto,
  AgentModuleSettings,
  AgentToolDto,
  ApiExceptionResponse,
  DeleteAgentParams,
  DeleteSkillParams,
  SaveAgentRequest,
  SaveSkillRequest,
  SkillDto,
  UpdateAgentParams,
  UpdateSkillParams,
} from "./data-contracts";

@Injectable()
export class AgentModuleApi<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  getAgents = (params: RequestParams = {}) =>
    this.request<AgentDto[], ApiExceptionResponse>({
      path: `/api/agent-module/get-agents`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  createAgent = (data: SaveAgentRequest, params: RequestParams = {}) =>
    this.request<AgentDto, ApiExceptionResponse>({
      path: `/api/agent-module/create-agent`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  updateAgent = (
    { id, ...query }: UpdateAgentParams,
    data: SaveAgentRequest,
    params: RequestParams = {},
  ) =>
    this.request<AgentDto, ApiExceptionResponse>({
      path: `/api/agent-module/update-agent/${id}`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  deleteAgent = (
    { id, ...query }: DeleteAgentParams,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/agent-module/delete-agent/${id}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
  getSkills = (params: RequestParams = {}) =>
    this.request<SkillDto[], ApiExceptionResponse>({
      path: `/api/agent-module/get-skills`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  createSkill = (data: SaveSkillRequest, params: RequestParams = {}) =>
    this.request<SkillDto, ApiExceptionResponse>({
      path: `/api/agent-module/create-skill`,
      method: "POST",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  updateSkill = (
    { id, ...query }: UpdateSkillParams,
    data: SaveSkillRequest,
    params: RequestParams = {},
  ) =>
    this.request<SkillDto, ApiExceptionResponse>({
      path: `/api/agent-module/update-skill/${id}`,
      method: "PUT",
      body: data,
      secure: true,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  deleteSkill = (
    { id, ...query }: DeleteSkillParams,
    params: RequestParams = {},
  ) =>
    this.request<void, ApiExceptionResponse>({
      path: `/api/agent-module/delete-skill/${id}`,
      method: "DELETE",
      secure: true,
      ...params,
    });
  getTools = (params: RequestParams = {}) =>
    this.request<AgentToolDto[], ApiExceptionResponse>({
      path: `/api/agent-module/get-tools`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
  getModuleInstanceSettings = (params: RequestParams = {}) =>
    this.request<AgentModuleSettings, ApiExceptionResponse>({
      path: `/api/agent-module/get-module-instance-settings`,
      method: "GET",
      secure: true,
      format: "json",
      ...params,
    });
}
