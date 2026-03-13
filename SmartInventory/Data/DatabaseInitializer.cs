using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SmartInventory.Entities;
using SmartInventory.Models;

namespace SmartInventory.Data;

public class DatabaseInitializer
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public DatabaseInitializer(AppDbContext dbContext, PasswordHasher<User> passwordHasher, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _dbContext.Database.MigrateAsync();

            var adminEmail = _configuration["SeedAdmin:Email"] ?? "admin@smartinventory.local";
            var adminPassword = _configuration["SeedAdmin:Password"] ?? "Admin@123";
            var adminName = _configuration["SeedAdmin:Name"] ?? "Administrador";

            var existingAdmin = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == adminEmail);
            if (existingAdmin is not null)
            {
                return;
            }

            var admin = new User
            {
                Name = adminName,
                Email = adminEmail,
                Role = UserRole.Admin,
                IsActive = true
            };

            admin.PasswordHash = _passwordHasher.HashPassword(admin, adminPassword);
            _dbContext.Users.Add(admin);

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Isso vai mostrar no console do Railway exatamente qual Host ele não achou
            Console.WriteLine("CRITICAL ERRO NA MIGRAÇÃO: " + ex.Message);
            if (ex.InnerException != null)
            {
                Console.WriteLine("DETALHE INTERNO: " + ex.InnerException.Message);
            }
            // O comando 'throw;' sozinho preserva o rastro (stack trace) original do erro
            throw;
        }
    }
}
