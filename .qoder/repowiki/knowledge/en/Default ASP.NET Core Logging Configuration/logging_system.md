## Overview

This Blazor Interactive Server application uses the **default ASP.NET Core logging infrastructure** provided by `Microsoft.Extensions.Logging`. No third-party logging framework (e.g., Serilog, NLog) is configured.

## What System/Approach Is Used

- **Framework**: Built-in `Microsoft.Extensions.Logging` via `WebApplication.CreateBuilder(args)` in `Program.cs`
- **No explicit logger injection**: No service or component in the codebase injects or uses `ILogger<T>` — all business logic (`SettlementService`) and UI components operate without any logging calls
- **No structured logging**: No custom log formatters, enrichment, or structured fields are configured
- **No custom sinks**: Output goes to the default console provider enabled by the ASP.NET Core host

## Key Files

- **`appsettings.json`** — Defines global log level strategy:
  - `Default`: `Information`
  - `Microsoft.AspNetCore`: `Warning` (suppresses verbose framework logs)
- **`appsettings.Development.json`** — Mirrors production settings (no environment-specific overrides)
- **`Program.cs`** — Standard host bootstrap; no `AddLogging()`, `UseSerilog()`, or custom logger factory configuration

## Architecture and Conventions

The logging system is entirely passive:
1. The ASP.NET Core host automatically registers the default logging providers (console) when `WebApplication.CreateBuilder()` is called.
2. Log levels are controlled solely through `appsettings.json`.
3. No application code emits log messages — there are zero references to `ILogger`, `LogInformation`, `LogError`, `Console.WriteLine`, or any diagnostic output across the entire codebase.
4. Error handling relies on `UseExceptionHandler` and `UseStatusCodePagesWithReExecute` middleware rather than logged diagnostics.

## Rules Developers Should Follow

Since no logging convention exists yet, developers introducing logging should:
1. **Inject `ILogger<T>`** into services/components that need diagnostics (constructor injection via DI).
2. **Use semantic log levels**: `LogError` for failures, `LogWarning` for recoverable issues, `LogInformation` for significant state changes, `LogDebug` for trace-level details.
3. **Respect existing level filters**: Keep `Microsoft.AspNetCore` at `Warning` unless debugging framework internals; use `Information` as the default for application-level events.
4. **Avoid `Console.WriteLine`**: Use the injected logger instead to ensure consistent formatting, level filtering, and future sink extensibility.
5. **Consider structured logging**: If adding a third-party provider like Serilog, define consistent event IDs and named properties for queryable logs.
