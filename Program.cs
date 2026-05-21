using DonationsTorresKevin.Components;
using DonationsTorresKevin.Components.Account;
using DonationsTorresKevin.Data;
using DonationsTorresKevin.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Stripe;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de servicios en el contenedor de dependencias
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// 2. Configuración de Identity para la autenticación y autorización
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

// 3. Configuración de Entity Framework Core con SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// A. Configurar la clave secreta global de Stripe leyendo desde User Secrets
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// B. Registrar nuestro servicio para poder inyectarlo en los componentes Blazor
builder.Services.AddScoped<StripeCheckoutService>();

// C. Agregar servicios para controladores API
builder.Services.AddControllers();

var app = builder.Build();

// 4. Configuración del pipeline de middleware HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    // The default HSTS value is 30 days. You may want to change this for production scenarios
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

// Mapear controladores API para que puedan ser accedidos desde el frontend Blazor
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 5. Configuración de rutas para Identity
app.MapAdditionalIdentityEndpoints();

app.Run();