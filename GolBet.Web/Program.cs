// GolBet.Web/Program.cs
using System.Globalization;
using GolBet.Entities; // Nuevo para Identity
using GolBet.Repositories.Data;
using GolBet.Repositories.Implementations;
using GolBet.Repositories.Interfaces;
using GolBet.Services.Implementations;
using GolBet.Services.Interfaces;
using GolBet.Services.Mapping;
using Microsoft.AspNetCore.Identity; // Nuevo para Identity
using Microsoft.EntityFrameworkCore;
using Serilog;

// Configuración de cultura es-CO
var culture = new CultureInfo("es-CO");
culture.NumberFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
culture.NumberFormat.NumberGroupSeparator = "";

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registro del DbContext (heredando de IdentityDbContext en el siguiente paso)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- Registro de Identity (Módulo 7) ----
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Política de contraseñas amigable para el entorno académico
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";            // Redirige al login si no está autenticado
    options.AccessDeniedPath = "/Account/AccessDenied";  // Redirige si no tiene el rol
});

// Open generic registration: one line, a repository for every entity
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Specific repositories
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IBetRepository, BetRepository>(); // 👈 NUEVO: Registro del repositorio de apuestas

// AutoMapper: scans the assembly containing MappingProfile for all profiles
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Business services
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IBetService, BetService>();       // 👈 NUEVO: Registro del servicio de apuestas

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/General");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSerilogRequestLogging();

app.UseRouting();

// ---- Pipeline de Autenticación y Autorización (¡El orden importa!) ----
app.UseAuthentication();   // 1º: Identifica quién eres leyendo la cookie
app.UseAuthorization();    // 2º: Decide si tienes permisos según [Authorize]

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ejecutar el Seeder al iniciar la aplicación (Actualizado para Identity)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        await DbSeeder.SeedAsync(context, roleManager, userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al migrar o sembrar la base de datos.");
    }
}