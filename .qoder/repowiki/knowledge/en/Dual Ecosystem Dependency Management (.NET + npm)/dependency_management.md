## Overview

This Blazor Interactive Server application uses a **dual dependency management system**:
- **.NET NuGet packages** for server-side and framework dependencies (managed via `.csproj` files)
- **npm packages** for frontend build tooling (Tailwind CSS, PostCSS, Autoprefixer)

No centralized version pinning strategy (e.g., `Directory.Packages.props`) or private registry configuration is present.

---

## .NET Dependencies (NuGet)

### Package Declaration
Dependencies are declared inline in each project file using `<PackageReference>` elements with explicit version numbers:

**Main project (`split_bill.csproj`):**
- `Microsoft.EntityFrameworkCore.Design` v10.0.8 — design-time EF Core tooling (build-only, not published)
- `Microsoft.EntityFrameworkCore.Sqlite` v10.0.8 — SQLite database provider
- `Microsoft.EntityFrameworkCore.Tools` v10.0.8 — migration CLI tools (build-only)

**Test project (`split_bill.Tests/split_bill.Tests.csproj`):**
- `coverlet.collector` v6.0.4 — code coverage
- `Microsoft.NET.Test.Sdk` v17.14.1 — test infrastructure
- `xunit` v2.9.3 — test framework
- `xunit.runner.visualstudio` v3.1.4 — VS test runner integration

### Versioning Strategy
- **Explicit pinned versions** — all packages use exact semantic versions (no floating ranges like `*` or `>=`)
- **No central package management** — each `.csproj` declares its own versions independently; no `Directory.Packages.props` for centralized version control
- **No lockfile** — NuGet does not produce a deterministic lockfile by default in this setup; reproducibility relies on the explicit version pins in `.csproj`

### Asset Control
EF Core design/tools packages use `<PrivateAssets>all</PrivateAssets>` and restricted `<IncludeAssets>` to ensure they are only available at build time and never flow to consumers or the publish output.

### Project References
The test project references the main project via `<ProjectReference Include="..\split_bill.csproj" />`, establishing an intra-solution dependency without external packaging.

---

## JavaScript/TypeScript Dependencies (npm)

### Package Declaration
Frontend build dependencies are declared in `package.json` under `devDependencies`:
- `autoprefixer` ^10.5.0
- `postcss` ^8.5.15
- `tailwindcss` ^3.4.19

All three are **dev-only** dependencies used for CSS compilation — none ship to the browser at runtime.

### Lockfile
`package-lock.json` (lockfileVersion 3) provides deterministic resolution of the entire transitive dependency tree. All packages resolve from the public npm registry (`https://registry.npmjs.org`).

### Build Integration
The `.csproj` hooks into the npm build pipeline via an MSBuild target:
```xml
<Target Name="TailwindCompileBeforeBuild" BeforeTargets="Build" Condition="Exists('node_modules')">
  <Exec Command="npm run build:css" />
</Target>
```
This ensures Tailwind CSS is compiled (`Styles/app.css` → `wwwroot/app.css`) before every .NET build, creating a tight coupling between the two dependency ecosystems.

### Vendored Assets
`wwwroot/lib/bootstrap/dist/` exists but is empty, suggesting Bootstrap was either removed or intended for future vendoring. No bootstrap npm package is declared, so Bootstrap is not currently managed through either system.

---

## Architecture & Conventions

1. **Framework-aligned targeting**: Both projects target `net10.0`, ensuring all NuGet packages are compatible with .NET 10.
2. **Separation of concerns**: Runtime dependencies (EF Core SQLite) are distinct from build-time tooling (EF Core Design/Tools, Tailwind stack).
3. **No private registries**: All packages come from public sources (nuget.org, npmjs.org). No `NuGet.config`, `npmrc`, or `GOPRIVATE`-equivalent configuration exists.
4. **No automated updates**: No Dependabot, Renovate, or similar tooling configuration is present.
5. **Solution structure**: `SplitBill.sln` aggregates both projects, enabling unified restore/build/test workflows via `dotnet restore` / `dotnet build`.

---

## Rules for Developers

1. **Pin versions explicitly** — always specify exact versions in `<PackageReference>` and `package.json`. Avoid caret/tilde ranges in production-critical dependencies.
2. **Commit lockfiles** — `package-lock.json` must be committed to ensure reproducible npm installs across environments.
3. **Use PrivateAssets for build-only packages** — EF Core design/tools packages must remain marked as `<PrivateAssets>all</PrivateAssets>` to prevent leaking into publish artifacts.
4. **Run `npm install` before first build** — the MSBuild target silently skips if `node_modules` is missing; developers must manually restore npm dependencies.
5. **Centralize versions if the repo grows** — consider adopting `Directory.Packages.props` (Central Package Management) if more projects are added to avoid version drift.
6. **No vendoring policy** — third-party JS/CSS libraries should be added via npm rather than copied into `wwwroot/lib/`, unless specifically required for offline scenarios.