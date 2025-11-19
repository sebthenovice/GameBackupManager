# Developer Guide — Contribution & Branch Naming

This document explains how contributors should structure branches and PRs so the repository's CI/CD workflows (in `.github/workflows/`) run predictably and transparently.

Summary of important rules
- Feature / work branches that should trigger the development CI must start with the prefix `dev` (see examples below).
- Open PRs targeting `dev*` branches will run the Dev CI and various security checks. Open PRs targeting `main` will run release-related checks and full security scans.
- To create a release, open a PR that targets `main` and merge it. The release workflow will automatically bump the version (labels control major/minor bumps).

Why this matters
- The repository uses reusable workflows (core-build, core-test, core-codeql) and orchestrator workflows (`ci-dev.yaml`, `codeql.yaml`, `security-check.yaml`, `release.yaml`).
- These workflows trigger on specific branch name patterns. Using the naming rules below ensures your commits and PRs run the expected checks (and that branch-protection rules behave correctly).

Branch naming rules (exact)
- Dev branches: must start with `dev` (the workflow uses the pattern `dev**`).
  - Valid examples:
    - `dev`
    - `dev-ui`
    - `dev-feature/add-auto-detect`
    - `dev/1234-fix-backup-path`
  - These branches will trigger `ci-dev.yaml` on push and will also be included by `codeql.yaml` and `security-check.yaml` for PRs.

- Main / release: use `main` as the production branch. The release workflow (`release.yaml`) runs when a pull request targeting `main` is closed and merged.

Pull requests and labels
- For a release bump, add one of these labels to the PR before merging to `main`:
  - `major` → major version bump
  - `minor` → minor version bump
  - (no label) → patch bump is used by default

- PRs targeting `dev*` should be opened against the appropriate `dev` branch (for example `dev-ui`). This will run the Dev CI and the security pipeline.

Status checks (what you will see in GitHub)
- `CI Status Check` — Aggregates build (Windows/Linux/macOS) + tests/coverage from `ci-dev.yaml`. Required for `dev*` branch protections.
- `CodeQL Status` — Aggregated result from the CodeQL analysis job. Required for both `dev*` and `main` PRs depending on protection rules.
- `Security / Dependency Scan` — Dependency check run on PRs (reports vulnerable NuGet packages).

Local dev workflow (quick commands)
- Build locally (Release):

```powershell
dotnet build GameBackupManager.App/GameBackupManager.App.csproj -c Release -r win-x64
```

- Run tests locally:

```powershell
dotnet test GameBackupManager.Tests/GameBackupManager.Tests.csproj
```

- Run tests with coverage (XPlat):

```powershell
dotnet test GameBackupManager.Tests/GameBackupManager.Tests.csproj --collect:"XPlat Code Coverage" --results-directory ./coverage
```

Tips and best practices
- Keep branch names short but descriptive, and always start feature branches with `dev` when you want CI to run on push.
- Use PR labels `major`/`minor` when you intend to change versioning semantics — the release workflow reads those labels.
- Do not use `[skip ci]` on development PRs; the release workflow intentionally uses `[skip ci]` when committing automated version changes to avoid loops.
- If a workflow is failing in CI, check the logs of the `[Core]` steps in the workflow run — those indicate actions executed inside the reusable workflows.

How to contribute (short)
1. Create a branch that starts with `dev` (e.g. `dev/1234-fix-backup-path`).
2. Commit changes and push your branch (this will trigger the Dev CI).
3. Open a PR against the appropriate `dev*` branch or `main` depending on the change.
4. Add labels as needed (use `minor` or `major` for release bump intent if merging to `main`).
5. Address CI and review comments; when green, merge according to the repo rules.

If you need more details or want to run the CI workflows locally using act or the CodeQL CLI, ask in the issue or reach out on the PR — we can add longer how-tos as needed.

---

Last updated: 2025-11-19

## License & credits

This project is licensed under the MIT license. See `LICENSE.txt` for the full text.

Special thanks and credits:

- Project and open-source dependencies: Avalonia, .NET, and the many open-source contributors to the libraries we use.
- Documentation and automation were updated with assistance from GitHub Copilot.

If you'd like a fuller acknowledgements section in the repo root (CONTRIBUTING or an AUTHORS file), we can add that as well.
