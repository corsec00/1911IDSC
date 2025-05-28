using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CompetitionApp.Managers;
using CompetitionApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        // Configure Razor Pages options if needed
    })
    .AddMvcOptions(options =>
    {
        // Configure model binding to accept both comma and dot as decimal separators
        options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
            _ => "Por favor, insira um número válido. Use ponto ou vírgula como separador decimal.");
    });

// Configure globalization options to support multiple cultures
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "pt-BR", "en-US" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

// Configure Razor View Engine to improve partial view discovery
builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
{
    // Add additional view location formats to ensure partials are found
    options.ViewLocationFormats.Add("/Pages/Shared/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Pages/{1}/{0}" + RazorViewEngine.ViewExtension);
    options.ViewLocationFormats.Add("/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
});

// Configure Azure Key Vault
var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
if (!string.IsNullOrEmpty(keyVaultUri))
{
    var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
    builder.Services.AddSingleton(secretClient);
}

// Register Azure Table Storage services
builder.Services.AddScoped<ITableStorageService, TableStorageService>();
builder.Services.AddScoped<ICompetitionService, CompetitionService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IFinalResultService, FinalResultService>();
builder.Services.AddScoped<ICompetitionManager, CompetitionManager>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

// Add request localization middleware
app.UseRequestLocalization();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
