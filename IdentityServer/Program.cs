using System.Reflection;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityServer.Data;
using IdentityServer.Factories;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using ApiResource = Duende.IdentityServer.Models.ApiResource;
using ApiScope = Duende.IdentityServer.Models.ApiScope;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>((provider, optionsBuilder) =>
{
    optionsBuilder.UseNpgsql(provider.GetRequiredService<IConfiguration>().GetConnectionString("Identity"),
        ngOptions => { ngOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name); });
});


builder.Host.UseSerilog();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer()
    .AddAspNetIdentity<ApplicationUser>()
    .AddConfigurationStore(confOptions =>
    {
        confOptions.ResolveDbContextOptions = (provider, optionsBuilder) =>
        {
            optionsBuilder
                .UseNpgsql(
                    provider.GetRequiredService<IConfiguration>().GetConnectionString("IdentityServerConfiguration"),
                    ngOptions =>
                    {
                        ngOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
                    });
        };
    })
    .AddOperationalStore(operationalOptions =>
    {
        operationalOptions.ResolveDbContextOptions = (provider, optionsBuilder) =>
        {
            optionsBuilder.UseNpgsql(
                provider.GetRequiredService<IConfiguration>().GetConnectionString("IdentityServerOperational"),
                ngOptions => { ngOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name); });
        };
    });


builder.Services.AddRazorPages();

var app = builder.Build();
app.UseSerilogRequestLogging(); 
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();
app.MapRazorPages();

if (app.Environment.IsDevelopment())
{
    var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.MigrateAsync();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    
    if (await userManager.FindByEmailAsync("bershadskyartem@gmail.com") is null)
        await userManager.CreateAsync(new ApplicationUser()
        {
            UserName = "Artyom",
            Email = "bershadskyartem@gmail.com",
            GivenName = "Artyom",
            FamilyName = "Bershadsky"
        }, "34019672baD!");

    var confDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

    if (!await confDbContext.ApiResources.AnyAsync())
    {
        await confDbContext.ApiResources.AddAsync(new ApiResource()
        {
            Name = "9fc33c2e-dbc1-4d0a-b212-68b9e07b3ba0",
            DisplayName = "API",
            Scopes = new List<string> { "https://www.example.com/api" }
        }.ToEntity());

        await confDbContext.SaveChangesAsync();
    }
    
    if (!await confDbContext.ApiScopes.AnyAsync())
    {
        await confDbContext.ApiScopes.AddAsync(new ApiScope()
        {
            Name = "https://www.example.com/api",
            DisplayName = "API"
        }.ToEntity());

        await confDbContext.SaveChangesAsync();
    }

    if (!await confDbContext.Clients.AnyAsync())
    {
        await confDbContext.Clients.AddRangeAsync(new Client()
        {
            ClientId = "b4e758d2-f13d-4a1e-bf38-cc88f4e290e1",
            ClientSecrets = new List<Secret>()
            {
                new Secret("secret".Sha512())
            },
            ClientName = "Console Application",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            AllowedScopes = new List<string>()
            {
                "https://www.example.com/api"
            },
            AllowedCorsOrigins = new List<string>()
            {
                "https://api:7001",
                "http://api:7001",
                "https://localhost:7001",
                "http://localhost:7001"
            }
        }.ToEntity(), new Client()
        {
            ClientId = "4ecc4153-daf9-4eca-8b60-818a63637a81",
            ClientSecrets = new List<Secret>()
            {
                new Secret("secret".Sha512())
            },
            ClientName = "Web Application",
            AllowedGrantTypes = GrantTypes.Code,
            AllowedScopes = new List<string>()
            {
                "openid",
                "profile",
                "email",
                "https://www.example.com/api"
            },
            RedirectUris = new List<string>()
            {
                "https://webapplication:7002/signin-oidc"
            },
            PostLogoutRedirectUris = new List<string>()
            {
                "https://webapplication:7002/signout-callback-oidc"
            }
        }.ToEntity(), new Client()
        {
            ClientId = "7e98ad57-540a-4191-b477-03d88b8187e1",
            RequireClientSecret = false,
            ClientName = "Single page application",
            AllowedGrantTypes = GrantTypes.Code,
            AllowedScopes = new List<string>()
            {
                "openid",
                "profile",
                "email",
                "https://www.example.com/api"
            },
            AllowedCorsOrigins = new List<string>()
            {
                "http://singlepageapplication:7003"
            },
            RedirectUris = new List<string>()
            {
                "http://singlepageapplication:7003/authentication/login-callback",
            },
            PostLogoutRedirectUris = new List<string>()
            {
                "http://singlepageapplication:7003/authentication/logout-callback",
            }
        }.ToEntity());

        await confDbContext.SaveChangesAsync();
    }

    if (!await confDbContext.IdentityResources.AnyAsync())
    {
        await confDbContext.IdentityResources.AddRangeAsync(
            new IdentityResources.OpenId().ToEntity(),
            new IdentityResources.Email().ToEntity(),
            new IdentityResources.Profile().ToEntity()
        );
        
        await confDbContext.SaveChangesAsync();
    }
}

app.Run();