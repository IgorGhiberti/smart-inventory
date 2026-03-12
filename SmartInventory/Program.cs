using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;
using SmartInventory.Endpoints;
using SmartInventory.Entities;
using SmartInventory.Middleware;
using SmartInventory.Interfaces;
using SmartInventory.Models;
using SmartInventory.Services;

namespace SmartInventory;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Busca a variável de ambiente
        var rawConn = Environment.GetEnvironmentVariable("DefaultConnection")?.Trim();

        if (string.IsNullOrEmpty(rawConn))
        {
            throw new Exception("ERRO: Variável 'DefaultConnection' está vazia ou nula no Railway.");
        }

        // Se você colou a URL começando com postgresql://, este bloco corrige para o formato ADO.NET
        string connectionString = rawConn;
        if (rawConn.StartsWith("postgresql://"))
        {
            var uri = new Uri(rawConn);
            var userInfo = uri.UserInfo.Split(':');
            connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        }

        Console.WriteLine($"LOG: Connection String processada. Tamanho final: {connectionString.Length} caracteres.");

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "smartinventory.auth";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.LoginPath = "/auth/login";
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRole.Admin.ToString()));
        });

        builder.Services.AddScoped<PasswordHasher<User>>();
        builder.Services.AddScoped<DatabaseInitializer>();

        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IInventoryMovementService, InventoryMovementService>();
        builder.Services.AddScoped<IApprovalService, ApprovalService>();
        builder.Services.AddScoped<IMovementTypeService, MovementTypeService>();

        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapAuthEndpoints();
        app.MapUserEndpoints();
        app.MapProductEndpoints();
        app.MapInventoryMovementEndpoints();
        app.MapApprovalEndpoints();
        app.MapMovementTypeEndpoints();

        await using (var scope = app.Services.CreateAsyncScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            await initializer.InitializeAsync();
        }

        await app.RunAsync();
    }
}

