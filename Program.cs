using split_bill.Components;
using Microsoft.EntityFrameworkCore;
using SplitBill.Data;
using SplitBill.Services;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=splitbill.db"));

builder.Services.AddScoped<SettlementService>();

// Configure Kestrel server URL
var preferredUrl = "http://localhost:5006";
builder.WebHost.UseUrls(preferredUrl);

// Build the app
var app = builder.Build();

// using System.IO; // moved to top
// In development, attempt to delete the SQLite database if it exists, then ensure the schema is created.
if (app.Environment.IsDevelopment())
{
    var dbPaths = new[]
    {
        Path.Combine(AppContext.BaseDirectory, "splitbill.db"),
        Path.Combine(Directory.GetCurrentDirectory(), "splitbill.db")
    };
    foreach (var dbPath in dbPaths)
    {
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
    }
    using (var scope = app.Services.CreateScope())
    {
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ctx.Database.EnsureCreated();   // create schema
    }
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
