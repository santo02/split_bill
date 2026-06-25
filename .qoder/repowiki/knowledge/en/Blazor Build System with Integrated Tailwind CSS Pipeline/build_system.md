## Build System Overview

The Split Bill Web Application uses a standard **.NET SDK** build system for a Blazor Interactive Server application, enhanced with a custom **Node.js/Tailwind CSS** integration for frontend styling.

### Core Build Tools
- **.NET 10.0**: The primary framework for compilation, testing, and runtime.
- **MSBuild**: Orchestrates the build process via `split_bill.csproj`.
- **npm/Tailwind CSS**: Manages frontend asset compilation (CSS) via `package.json`.
- **xUnit**: Handles unit testing in the `split_bill.Tests` project.

### Build Architecture & Conventions

#### 1. Integrated CSS Build Pipeline
The project employs a hybrid build strategy where .NET MSBuild triggers Node.js tasks:
- **Trigger**: A custom MSBuild target `TailwindCompileBeforeBuild` runs before the main `Build` target.
- **Condition**: It only executes if the `node_modules` directory exists, ensuring graceful handling of environments without Node.js installed.
- **Command**: Executes `npm run build:css`, which compiles `Styles/app.css` into `wwwroot/app.css` using Tailwind CSS.

#### 2. Project Structure
- **Solution File**: `SplitBill.sln` includes both the main application (`split_bill.csproj`) and the test project (`split_bill.Tests/split_bill.Tests.csproj`).
- **Test Isolation**: The main `.csproj` explicitly removes test files from compilation/content to prevent conflicts, while the test project references the main project.

#### 3. Development Workflow
- **Launch Profiles**: Defined in `Properties/launchSettings.json`, supporting both `http` (port 5006) and `https` (port 7280) profiles.
- **Hot Reload**: Enabled via `dotnetRunMessages: true` in launch settings.
- **CSS Watching**: Developers can use `npm run watch:css` for continuous CSS compilation during development, separate from the main .NET build loop.

### Key Files
- `split_bill.csproj`: Defines the .NET build configuration, dependencies, and the custom Tailwind build target.
- `package.json`: Defines Node.js scripts for CSS compilation (`build:css`, `watch:css`).
- `SplitBill.sln`: Solution file grouping the app and tests.
- `Properties/launchSettings.json`: Configures local development server profiles.
- `split_bill.Tests/split_bill.Tests.csproj`: Configuration for the xUnit test suite.

### Developer Rules
1. **Node.js Requirement**: Ensure Node.js and npm are installed to compile CSS. If `node_modules` is missing, the build will skip CSS compilation but may result in unstyled output.
2. **CSS Changes**: After modifying `Styles/app.css`, run `npm run build:css` or rebuild the project to update `wwwroot/app.css`.
3. **Testing**: Run tests using `dotnet test` in the root or `split_bill.Tests` directory.
4. **No CI/CD**: No GitHub Actions, Dockerfiles, or Makefiles were found; deployment and CI processes are not currently defined in the repository.