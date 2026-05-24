# dotnet Template Configuration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Convert this repo into an installable dotnet new template (`cqrs-api`) that scaffolds a new CQRS project with namespace, file names, and database name auto-replaced from `--name`.

**Architecture:** A `.template.config/template.json` at the repo root makes the entire directory the template source. The `sourceName` token `BackendTemplate` is replaced throughout file names and content. A derived `nameLower` symbol handles the lowercase `backendtemplate` occurrences (docker-compose, connection strings). No NuGet packaging — local install only for now.

**Tech Stack:** dotnet new template engine, PowerShell

---

## File Map

| Action | Path | Responsibility |
|---|---|---|
| Create | `.template.config/template.json` | Template metadata, token substitution rules, exclusion list |
| Modify | `backend/CLAUDE.md` | Remove two stale comments that would confuse users of generated projects |

---

### Task 1: Create `.template.config/template.json`

**Files:**
- Create: `.template.config/template.json`

- [ ] **Step 1: Create the `.template.config` directory and `template.json`**

Create the directory `C:\GITS\CSharp\dotnet-cqrs-template\.template.config\` then write:

```json
{
  "$schema": "http://json.schemastore.org/template",
  "author": "TheBurntDev",
  "classifications": [ "API", "CQRS", "Backend", "Web" ],
  "identity": "theburntdev.cqrs-api",
  "name": "Traditional CQRS API",
  "description": "A traditional CQRS API with Domain, Application, Infrastructure, and API layers using ASP.NET Core minimal APIs, EF Core, MediatR, FluentValidation, Mapperly, and Serilog.",
  "shortName": "cqrs-api",
  "tags": {
    "language": "C#",
    "type": "project"
  },
  "sourceName": "BackendTemplate",
  "preferNameDirectory": true,
  "symbols": {
    "nameLower": {
      "type": "derived",
      "valueSource": "name",
      "valueTransform": "lowerCase",
      "replaces": "backendtemplate"
    }
  },
  "sources": [
    {
      "exclude": [
        "docs/**",
        ".git/**",
        "**/bin/**",
        "**/obj/**",
        "**/.vs/**"
      ]
    }
  ]
}
```

- [ ] **Step 2: Verify template installs locally**

Run:
```powershell
dotnet new install C:\GITS\CSharp\dotnet-cqrs-template
```

Expected output contains:
```
Success: theburntdev.cqrs-api::1.0.0 installed the following templates:
  cqrs-api
```

- [ ] **Step 3: Uninstall (clean up for now — full smoke test in Task 3)**

```powershell
dotnet new uninstall C:\GITS\CSharp\dotnet-cqrs-template
```

Expected: `Success: ...` uninstall confirmation.

- [ ] **Step 4: Commit**

```powershell
git add .template.config/template.json
git commit -m "feat: add dotnet new template configuration"
```

---

### Task 2: Fix stale content in `backend/CLAUDE.md`

**Files:**
- Modify: `backend/CLAUDE.md`

Two stale comments will confuse users of the generated project:
1. Line 2: `"See root \`CLAUDE.md\` for project vocabulary and cross-cutting rules."` — no root CLAUDE.md exists.
2. Line 16: `"## Solution structure (target — not yet created)"` — the structure is already created; this comment is a planning artifact.

- [ ] **Step 1: Remove the dead root CLAUDE.md reference**

In `backend/CLAUDE.md`, remove line 2:
```
See root `CLAUDE.md` for project vocabulary and cross-cutting rules.
```

The file should open with `## Tech choices (decided)` after the `# Backend — C# / .NET` heading.

- [ ] **Step 2: Fix the solution structure heading**

Change:
```markdown
## Solution structure (target — not yet created)
```
To:
```markdown
## Solution structure
```

- [ ] **Step 3: Commit**

```powershell
git add backend/CLAUDE.md
git commit -m "fix: remove stale planning comments from CLAUDE.md"
```

---

### Task 3: Smoke test — scaffold and verify

No file changes. This task verifies the template produces correct output.

- [ ] **Step 1: Install the template**

```powershell
dotnet new install C:\GITS\CSharp\dotnet-cqrs-template
```

Expected: installation success with `cqrs-api` listed.

- [ ] **Step 2: Scaffold a test project**

```powershell
dotnet new cqrs-api --name Acme -o C:\Temp\AcmeTest
```

Expected: scaffolding completes with no errors.

- [ ] **Step 3: Verify file names were replaced**

```powershell
Get-ChildItem -Recurse C:\Temp\AcmeTest | Where-Object { $_.Name -like "*Acme*" } | Select-Object FullName
```

Expected: entries like `Acme.slnx`, `Acme.Api.csproj`, `Acme.Domain.csproj`, `Acme.Application.csproj`, `Acme.Infrastructure.csproj`, and test projects.

- [ ] **Step 4: Verify no `BackendTemplate` remains in file names**

```powershell
Get-ChildItem -Recurse C:\Temp\AcmeTest | Where-Object { $_.Name -like "*BackendTemplate*" } | Select-Object FullName
```

Expected: empty (zero results).

- [ ] **Step 5: Verify namespace replacement in generated source**

```powershell
Select-String -Path "C:\Temp\AcmeTest\backend\src\Acme.Api\Program.cs" -Pattern "BackendTemplate"
```

Expected: no matches.

- [ ] **Step 6: Verify docker-compose DB name replaced**

```powershell
Get-Content C:\Temp\AcmeTest\docker-compose.yml
```

Expected: `POSTGRES_DB: acme` (not `backendtemplate`).

- [ ] **Step 7: Verify build succeeds**

```powershell
dotnet build C:\Temp\AcmeTest\backend\Acme.slnx
```

Expected: `Build succeeded.` with 0 errors.

- [ ] **Step 8: Clean up**

```powershell
dotnet new uninstall C:\GITS\CSharp\dotnet-cqrs-template
Remove-Item -Recurse -Force C:\Temp\AcmeTest
```

---

## Self-Review

**Spec coverage:**
- ✅ `sourceName: "BackendTemplate"` replaces namespace in file names and content
- ✅ `nameLower` derived symbol replaces `backendtemplate` in docker-compose and README connection string
- ✅ `docs/**` excluded
- ✅ `bin/`, `obj/`, `.vs/` excluded
- ✅ `README.md`, `.gitignore`, `backend/CLAUDE.md` included (no explicit exclude = included)
- ✅ `preferNameDirectory: true` for ergonomic `dotnet new cqrs-api --name X` UX
- ✅ Short name `cqrs-api`, description "Traditional CQRS API", author `TheBurntDev`
- ✅ No optional parameters
- ✅ Local install only (no NuGet packaging)
- ✅ Stale CLAUDE.md comments cleaned up

**Placeholder scan:** None found — all steps have exact commands or content.

**Type consistency:** No code types — N/A.
