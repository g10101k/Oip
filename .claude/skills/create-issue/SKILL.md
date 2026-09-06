---
name: create-issue
description: Create a GitHub issue in this repository from a free-form description by the user. Validates that the issue is worth filing, agrees on the title and body with the user in their own language, then files the issue on GitHub in English. Use when the user describes a bug, a feature request, or a task and wants it tracked as an issue ("создай issue", "заведи задачу", "file an issue", "make a github issue").
---

# Create a GitHub issue

The user describes what they want in free form, usually in Russian. Turn that into a
well-formed GitHub issue in **three ordered steps**. Never skip step 1 or step 2, and
never create the issue before the user has explicitly approved the draft.

## Step 1 — Validate the issue

Before drafting anything, make sure you and the user actually mean the same thing.

1. Restate the request in one or two sentences in the user's language and say what you
   understood the problem/goal to be.
2. Check the repository for context — do not guess:
   - `gh issue list --search "<keywords>" --state all --limit 10` — is it a duplicate?
   - Grep the relevant code (`AGENTS.md`, the affected project) — does the behaviour
     already exist, or is it already fixed on `main`?
3. Judge whether it is worth filing. Say so plainly if it is not:
   - duplicate of an existing issue (give the number and link),
   - already implemented or already fixed,
   - too vague to act on — then ask the **specific** missing questions
     (which module, which version, expected vs actual behaviour, reproduction steps),
   - a trivial change better done right now than tracked.
4. If something is genuinely ambiguous, ask before drafting. One focused round of
   questions, not an interrogation.

Only continue when the issue is understood and worth filing.

## Step 2 — Draft and agree (in the user's language)

Draft the title and body **in the language the user is writing in** and show it in chat
for approval. This draft is for agreement, not the final artifact.

Title:
- short, imperative or declarative noun phrase, no trailing period,
- names the affected area, matching existing repo style, e.g.
  `Setup default module for root path (/)`, `Oip.Cli find front project by SpaCommand`,
  `MinIO empty stream error`.

Body — include only the sections that carry information:

- **Bug**: Description / Steps to reproduce / Expected behaviour / Actual behaviour /
  Environment (version, browser, OS) when relevant.
- **Feature or task**: Problem or motivation / Proposed solution / Acceptance criteria /
  Out of scope, when it prevents misreading.

Reference concrete files and lines you verified (`Oip.Base/Exceptions/ApiExceptionResponse.cs`),
never invented ones. Keep it tight — no filler, no restated title.

Propose labels from the repository set: `bug`, `enhancement`, `documentation`,
`architecture`, `security`, `question`, `good first issue`, `help wanted`.
Use `gh label list` if unsure what exists.

Ask the user to confirm or correct. Iterate until they approve.

## Step 3 — Create the issue in English

Translate the approved title and body into **English** — same content, same structure,
technical terms kept as-is. Do not add anything the user did not approve.

Write the body to a file first so markdown and newlines survive:

```bash
gh issue create --title "<English title>" --body-file /tmp/issue-body.md --label "<label>"
```

Use the scratchpad directory for the body file. Add `--assignee`, `--milestone` or extra
`--label` flags only when the user asked for them.

Then report back to the user in their language: the issue number, the URL, and the labels
applied. If `gh issue create` fails (auth, missing label), show the error and fix it —
do not silently drop flags.
