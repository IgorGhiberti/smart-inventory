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

        var rawConn = Environment.GetEnvironmentVariable("DefaultConnection")
              ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        // O Trim() remove espaços ou quebras de linha acidentais no início/fim
        var connectionString = rawConn?.Trim();

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("LOG DE ERRO: A variável de conexão veio VAZIA ou NULA!");
        }

        // Log seguro para você ver no Railway se ele está lendo algo
        Console.WriteLine($"LOG: Connection String carregada. connection string: {connectionString}");

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

