# Contributing to Game Backup Manager

Thank you for your interest in contributing. This short guide points contributors to the canonical developer guide and explains branch naming rules so CI behaves predictably.

Core pointers

- Read the full developer workflow and CI expectations in `.github/DEVELOPER_GUIDE.md` (branch naming, PR labels for release bumps, and local commands).
- Branches that should trigger the Dev CI must start with `dev` (the workflows use the `dev**` pattern). Examples: `dev`, `dev-ui`, `dev/1234-fix`.
- PRs targeting `main` trigger the release workflow — add the `major` or `minor` label to indicate a version bump; otherwise a patch bump is applied.
- The repository is licensed under the MIT license. See `LICENSE.txt`.

Credits

- This project uses Avalonia and .NET. Thanks to the upstream projects and contributors.
- Documentation and workflow refactors were performed with assistance from GitHub Copilot.

If you need help with the contribution flow, open an issue or a draft PR and tag maintainers.

Last updated: 2025-11-19
