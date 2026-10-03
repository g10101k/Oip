---
name: prepare-release
description: Prepare the project for a release — switch to an up-to-date main, make sure unit tests are green, stop the dev container, start the test container and run the UI tests (Oip.UiTest) in standalone mode, then, with the user's permission, bring the dev container back. Use when the user asks to prepare a release, check main before a release, or run the full test suite ("подготовь релиз", "проверь main перед релизом", "prepare release", "release check").
---

# Prepare a release

This skill verifies that `main` is ready for a release. Run the steps **strictly in order**. If a step fails, do not
continue with the next verification step — go straight to **Step 6 (restore the dev environment)** and report.

Always reach Step 6, even if something failed. But switching back to the dev environment happens only after the user
explicitly allows it (see Step 6). Never finish silently and leave the user with the dev container stopped.

Run all commands from the repository root unless stated otherwise. Point `docker compose` at the compose files with
`-f` instead of `cd`-ing into their directories: the shell working directory persists between commands.

## Step 1 — Switch to an up-to-date main

1. `git status --porcelain` — if the working tree has changes, **stop** and ask the user. Do not stash, reset, or check
   out over the changes without their explicit consent.
2. Remember the current branch (`git branch --show-current`) — it goes into the report.
3. Switch and pull:

   ```bash
   git fetch origin
   git switch main
   git pull --ff-only origin main
   ```

   If `--ff-only` fails (local main has diverged from origin), stop and show the user
   `git log --oneline origin/main..main`. Do not merge, rebase, or reset on your own.
4. Report which commit `main` is on (`git log --oneline -1`).

## Step 2 — Unit tests

Run the test projects one by one. **Do not** run `Oip.UiTest` here: without the test container it is guaranteed to fail
at login.

```bash
dotnet test src/Oip.Test/Oip.Test.csproj
dotnet test src/Oip.Cli.Test/Oip.Cli.Test.csproj
dotnet test src/Oip.Rtds.Test/Oip.Rtds.Test.csproj
```

If any tests fail, show their names and error messages and stop (the dev container has not been touched yet, so Step 6
is a no-op in this case). Do not fix tests or code as part of this skill — only report.

## Step 3 — Stop the dev container

The test container uses the same ports (5432, 8080, 8443, 6379), so the dev environment has to be stopped.

1. Remember which dev services are currently running — Step 6 starts **only those**, not the whole `dev.yml` (the user
   usually runs only some of the services):

   ```bash
   docker compose -p oip-devcontainer ps --services --filter status=running
   ```

   Save the list and show it to the user. If the list is empty, the dev container was not running and Step 6 has
   nothing to start.
2. Stop it without removing volumes (the dev database and Keycloak data must survive):

   ```bash
   docker compose -f .oip-devcontainer/dev.yml -f .oip-devcontainer/dev.unix.yml down
   ```

   On Windows use `dev.windows.yml` instead of `dev.unix.yml`. **Never use `down -v` for the dev container.**
3. If the frontend is running locally (`ng serve` on port 50002), leave it alone — it does not conflict with the test
   container.

## Step 4 — Start the test container

The test container builds the `oip` image from the current checkout, i.e. from `main`. Standalone mode is set explicitly
by the `ServiceAddingMode: Local` variable of the `oip` service in `.oip-testcontainer/docker-compose.yml`. Before
starting, make sure it is there; if it is missing, stop and tell the user instead of running the tests in another mode.

Start the infrastructure (postgres, keycloak, redis, selenium, etc.) without recreating it — existing containers are
reused. Rebuild and recreate only `oip`, so that it is guaranteed to be built from the current `main`:

```bash
docker compose -f .oip-testcontainer/docker-compose.yml up -d
docker compose -f .oip-testcontainer/docker-compose.yml up -d --build --force-recreate --no-deps oip
```

The build takes several minutes — run it in the background and wait for it to finish. If the build fails because the
disk is full, show the error and suggest `docker builder prune -af` to the user (do not run it yourself).

Wait until everything is ready (poll with pauses, overall timeout ~5 minutes):

- Selenium Grid: `curl -sf http://localhost:4444/wd/hub/status` responds with `ready: true`;
- the application: `curl -sk -o /dev/null -w "%{http_code}" https://localhost:50000/` returns `200`;
- Keycloak: `curl -sk -o /dev/null -w "%{http_code}" https://localhost:8443/realms/oip` returns `200`.

If it is not up within the timeout, show `docker compose -f .oip-testcontainer/docker-compose.yml ps` and the tail of
`docker compose -f .oip-testcontainer/docker-compose.yml logs oip keycloak --tail 50`, then go to Step 6.

## Step 5 — UI tests

```bash
dotnet test src/Oip.UiTest/Oip.UiTest.csproj --settings .oip-testcontainer/settings/default.runsettings
```

`default.runsettings` switches the tests to Selenium Grid (`RemoteDriverUrl`) and to the `https://oip:50000` address
inside the docker network.

- If **all** tests fail in `OneTimeSetUp`, it is an environment problem (login, certificates, Keycloak), not a test
  problem. Say so in the report and include the first error.
- If individual tests fail, list them with their error messages.
- Test run videos are written to the anonymous `/videos` volume of the `chrome-video` container (viewable through
  file-browser at `http://localhost:8081` while the test container is up).

## Step 6 — Bring the dev container back (only with the user's permission)

If the dev container was not touched in Step 3 (e.g. unit tests failed), skip this step.

Otherwise, first show the intermediate results (unit tests, UI tests) and **ask the user whether it is OK to switch to
the dev environment**. In the question, mention:

- that the test container will be stopped together with its volumes, the test run videos will be deleted, and
  file-browser (`http://localhost:8081`) will become unavailable — if there are failed UI tests, the videos should be
  watched now;
- which dev services will be started (the list from Step 3).

Wait for an explicit answer. If the user does not allow it, do not stop or start anything — give the report and the
commands from items 1–2 below with the service list already filled in, so the user can restore the dev environment
themselves.

Once allowed:

1. Stop the test container together with its anonymous volumes (videos, test database data) so they do not pile up:

   ```bash
   docker compose -f .oip-testcontainer/docker-compose.yml down -v
   ```

2. Start **the same** dev services that were running in Step 3:

   ```bash
   docker compose -f .oip-devcontainer/dev.yml -f .oip-devcontainer/dev.unix.yml up -d <services from Step 3>
   ```

3. Check `docker compose -p oip-devcontainer ps` — every service from the list is `running`/`healthy`.

Stay on `main`. Do not switch back to the original branch unless the user asks — just mention it in the report.

## Report

Short, in the user's language:

- the `main` commit that was verified;
- unit tests: project → passed/failed/skipped;
- UI tests: passed/failed, the list of failures with reasons;
- environment state: dev environment restored, or (if the user did not allow it) the test container is still up plus
  the commands to restore the dev environment;
- the original branch you switched from;
- a one-line verdict: **ready for release** / **not ready** (and why).
