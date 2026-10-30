# Contributing to CONCOST.UI

Thank you for helping improve CONCOST.UI. Bug reports, focused fixes, documentation improvements, and new controls are welcome.

## Before you start

- Search existing issues and pull requests to avoid duplicate work.
- For a substantial change, open an issue first so the intended behavior can be discussed.
- Keep user-facing text and code comments in English.

## Development environment

- Windows 10 or later
- .NET 8 SDK
- Visual Studio 2022 with the .NET desktop development workload, or another editor with .NET support

Clone the repository, then restore and build the solution:

```powershell
dotnet restore CONCOST.UI.sln
dotnet build CONCOST.UI.sln --configuration Debug
```

Run the relevant tests before submitting a pull request:

```powershell
dotnet test tests/CONCOST.UI.Tests/CONCOST.UI.Tests.csproj --configuration Debug
```

## Control and theme guidelines

- Build each control from first principles, tuned for `CONCOST.UI` (theme integration, design tokens, modern animation).
- Implement every meaningful variant and style, so users can see how to apply the control.
- Use the existing design tokens and dynamic theme resources where applicable.
- Keep controls keyboard accessible and verify their behavior in light and dark themes.
- Add or update a Showcase example for new controls or meaningful variants.
- Add focused tests for behavior that can be tested without UI interaction.
- Keep the library as one `CONCOST.UI` assembly unless an architectural change has been discussed.

### Showcase layout

- Section cards use a surface background, `CornerRadius="10"`, a 1 px theme border and consistent padding.
- Controls in the same group share a uniform width or grid column — no ragged alignment.
- Show the active/enabled state; redundant "disabled" tiles are not needed.
- Every demo exposes **View XAML** and **View C#** at the top-right of its card, with a copy button.
- The snippet must match the running demo exactly — no truncated placeholders.

## Pull request checklist

- [ ] The change has a clear description and scope.
- [ ] The solution builds successfully.
- [ ] Relevant tests have been run and their results are reported.
- [ ] User-facing labels, tooltips, sample text, and code comments are in English.
- [ ] New controls include suitable theme resources and a Showcase example.
- [ ] Documentation is updated when the public API or setup steps change.

## Commit messages

Use concise Conventional Commit prefixes when practical:

- `feat:` for a new capability
- `fix:` for a bug fix
- `docs:` for documentation
- `refactor:` for internal restructuring
- `test:` for test changes
- `build:` for build and repository configuration
