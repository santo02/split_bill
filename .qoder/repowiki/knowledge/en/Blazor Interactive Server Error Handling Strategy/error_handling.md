# Error Handling Approach

This Blazor Interactive Server application uses ASP.NET Core's built-in error handling infrastructure combined with minimal custom error management. The approach is framework-driven rather than implementing a bespoke error system.

## System Architecture

### 1. Global Exception Handling Middleware

**File: `Program.cs` (lines 57-63)**

The application configures ASP.NET Core's standard exception handling pipeline:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
```

Key decisions:
- **Production-only activation**: `UseExceptionHandler` is only enabled when NOT in development mode, following ASP.NET Core best practices to avoid leaking sensitive exception details to end users.
- **Scoped error context**: `createScopeForErrors: true` ensures each error gets its own DI scope for proper resource cleanup.
- **404 handling**: `UseStatusCodePagesWithReExecute` re-executes the request pipeline at `/not-found` for missing routes, preserving the original request context.

### 2. Dedicated Error Pages

**Files:**
- `Components/Pages/Error.razor` — Generic error page at `/Error`
- `Components/Pages/NotFound.razor` — 404 page at `/not-found`

**Error.razor** displays:
- A generic "An error occurred while processing your request" message
- Request ID from `Activity.Current?.Id` or `HttpContext.TraceIdentifier` for traceability
- Development-mode guidance explaining that detailed errors are only shown in Development environment via `ASPNETCORE_ENVIRONMENT` variable

**NotFound.razor** provides a simple "content does not exist" message with the `MainLayout` wrapper.

### 3. Circuit Reconnection Handling

**File: `Components/Layout/ReconnectModal.razor` (+ `.razor.js`, `.razor.css`)**

Blazor Interactive Server includes built-in circuit disconnection recovery. The `ReconnectModal` component handles:
- Automatic retry attempts with countdown display
- Manual retry button triggering JavaScript-based reconnection
- Visibility-change detection to retry when the browser tab becomes visible again
- Graceful failure messaging when reconnection is impossible

The JavaScript (`ReconnectModal.razor.js`) wraps reconnection logic in try/catch blocks and falls back to document visibility listeners on failure.

### 4. Localized Try/Catch Patterns

**File: `Components/Pages/BillDetails.razor`**

Two instances of bare `try/catch` with empty catch blocks (swallowing exceptions):

1. **Lines 1074-1086** — localStorage access for creator token verification:
   ```csharp
   try
   {
       var savedToken = await JS.InvokeAsync<string>("localStorage.getItem", $"bill_creator_token_{Uuid}");
       isCreator = !string.IsNullOrEmpty(savedToken) && savedToken == bill.CreatorToken;
       StateHasChanged();
   }
   catch
   {
       // Fallback
   }
   ```

2. **Lines 1438-1450** — Clipboard API invocation:
   ```csharp
   try
   {
       await JS.InvokeVoidAsync("navigator.clipboard.writeText", Navigation.Uri);
       copied = true;
       // ... reset after delay
   }
   catch
   {
       // Fail
   }
   ```

Both cases involve JavaScript interop calls where failure is non-critical (graceful degradation). However, the empty catch blocks provide no logging or user feedback.

### 5. Database Initialization Error Handling

**File: `Program.cs` (lines 36-46)**

During development database setup, `IOException` is caught when attempting to delete a locked SQLite file:

```csharp
try
{
    if (File.Exists(dbPath))
    {
        File.Delete(dbPath);
    }
}
catch (IOException)
{
    // If locked, skip
}
```

This is a targeted catch for a known transient condition (file lock during hot-reload scenarios).

### 6. Service Layer — No Explicit Error Handling

**File: `Services/SettlementService.cs`**

The core business logic service contains no try/catch blocks, null guards, or validation. It assumes well-formed input from callers. Edge cases like division by zero are guarded implicitly (e.g., `totalRatio > 0` checks before division), but there are no thrown exceptions or error results for invalid states.

## Conventions and Developer Rules

1. **Rely on framework middleware** for unhandled exceptions — do not implement custom global error handlers.
2. **Never expose raw exception details** in production; use the `/Error` page which hides stack traces.
3. **Use `UseStatusCodePagesWithReExecute`** for HTTP-level errors (404, etc.) to maintain request context.
4. **Empty catch blocks are acceptable only for non-critical JS interop** where failure has no downstream impact. Prefer logging or user notification in other contexts.
5. **Service layer methods do not throw** — they assume valid input. Callers (Blazor components) are responsible for input validation before invoking services.
6. **No custom exception types** exist in this codebase. All errors flow through the standard .NET exception hierarchy.
7. **No panic/recover pattern** — this is a C# application; unhandled exceptions terminate the current request/circuit, not the process.

## Key Files

- `Program.cs` — Middleware configuration for exception and status code handling
- `Components/Pages/Error.razor` — Production error page
- `Components/Pages/NotFound.razor` — 404 page
- `Components/Layout/ReconnectModal.razor` + `.razor.js` — Circuit reconnection UI and logic
- `Components/Pages/BillDetails.razor` — Localized try/catch for JS interop
- `Services/SettlementService.cs` — Service layer with implicit (no explicit) error handling