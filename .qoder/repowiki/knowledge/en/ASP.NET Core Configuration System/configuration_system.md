## Configuration Approach

This Blazor Interactive Server application uses the standard **ASP.NET Core configuration system** with JSON-based configuration files and environment-aware layering.

### Configuration Files

- **`appsettings.json`** — Base configuration file containing default settings for logging levels and allowed hosts.
- **`appsettings.Development.json`** — Environment-specific overrides applied when `ASPNETCORE_ENVIRONMENT` is set to `Development`. Currently mirrors base logging settings.
- **`Properties/launchSettings.json`** — Defines development launch profiles (`http`, `https`) with embedded environment variables (`ASPNETCORE_ENVIRONMENT=Development`) and application URLs.

### Configuration Loading

The application relies on the default ASP.NET Core configuration pipeline established by `WebApplication.CreateBuilder(args)` in `Program.cs`. This automatically:
1. Loads `appsettings.json`
2. Overlays `appsettings.{Environment}.json` based on the current environment
3. Applies command-line arguments and environment variables as higher-priority sources

No custom configuration sections, `IOptions<T>` patterns, or `Configuration.GetSection()` calls are present. All configuration is handled implicitly through the framework defaults.

### Key Configuration Decisions

- **Database connection string** is hardcoded in `Program.cs` (`Data Source=splitbill.db`) rather than pulled from configuration files. This is a deviation from best practices and should be moved to `appsettings.json` for proper layering.
- **Server URL** is hardcoded in `Program.cs` via `builder.WebHost.UseUrls("http://localhost:5006")`, bypassing the standard `launchSettings.json` or `appsettings` mechanism. The `launchSettings.json` already defines matching URLs, making this redundant.
- **Environment-based behavior** (database deletion and schema creation) is gated on `app.Environment.IsDevelopment()`, leveraging the standard environment detection from `ASPNETCORE_ENVIRONMENT`.
- **No secrets management** — No `user-secrets`, Azure Key Vault, or environment-variable-based secret injection is configured. The SQLite database path and connection details are plain-text.
- **No feature flags** — No feature flag system or toggle configuration exists.

### Conventions Developers Should Follow

1. **Move hardcoded values to configuration**: The SQLite connection string and server URL should be extracted into `appsettings.json` and accessed via `builder.Configuration.GetConnectionString()` or `Configuration.GetValue<T>()`.
2. **Use `IOptions<T>` for structured config**: If additional settings are added (e.g., tax rates, settlement rules), define a POCO options class and register it via `builder.Services.Configure<T>()`.
3. **Leverage environment layering**: Add environment-specific overrides (e.g., different database paths for staging/production) in `appsettings.Production.json` rather than conditional logic in code.
4. **Secrets for production**: For any sensitive data (API keys, external service credentials), use .NET User Secrets in development and a proper secrets manager (Azure Key Vault, AWS Secrets Manager, or environment variables) in production.
5. **Keep `launchSettings.json` for dev-only**: Do not duplicate URL or environment configuration in `Program.cs`; let `launchSettings.json` and the hosting environment handle this.