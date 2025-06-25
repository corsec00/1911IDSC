using CompetitionApp.Data;
using CompetitionApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configurar Entity Framework com PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<CompetitionDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
    
    // Habilitar logs sensíveis apenas em desenvolvimento
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Registrar serviços
builder.Services.AddScoped<ICompetitionService, CompetitionService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IFinalResultService, FinalResultService>();

// Adicionar Razor Pages
builder.Services.AddRazorPages();

// Configurar middleware para normalizar valores decimais
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

var app = builder.Build();

// Configurar pipeline de requisições
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Middleware para normalizar valores decimais (vírgula para ponto)
app.Use(async (context, next) =>
{
    if (context.Request.Method == "POST" && context.Request.HasFormContentType)
    {
        var form = await context.Request.ReadFormAsync();
        var normalizedForm = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();
        
        foreach (var item in form)
        {
            var value = item.Value.ToString();
            // Normalizar valores decimais (trocar vírgula por ponto)
            if (decimal.TryParse(value.Replace(',', '.'), System.Globalization.NumberStyles.Float, 
                System.Globalization.CultureInfo.InvariantCulture, out _))
            {
                normalizedForm[item.Key] = value.Replace(',', '.');
            }
            else
            {
                normalizedForm[item.Key] = item.Value;
            }
        }
        
        // Substituir o form original pelo normalizado
        context.Request.Form = new FormCollection(normalizedForm);
    }
    
    await next();
});

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

// Executar migrações automaticamente em desenvolvimento
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<CompetitionDbContext>();
        try
        {
            await context.Database.MigrateAsync();
            app.Logger.LogInformation("Migrações do banco de dados aplicadas com sucesso");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Erro ao aplicar migrações do banco de dados");
        }
    }
}

app.Logger.LogInformation("Aplicação iniciada com PostgreSQL");

app.Run();

