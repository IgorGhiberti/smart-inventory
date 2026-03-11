using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SmartInventory.Data;
using SmartInventory.DTOs.AuthDTOs;
using SmartInventory.Entities;
using SmartInventory.Interfaces;
using SmartInventory.Models;

namespace SmartInventory.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(AppDbContext dbContext, PasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<ServiceResult<AuthUserDto>> LoginAsync(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);
        if (user is null || !user.IsActive)
        {
            return ServiceResult<AuthUserDto>.Fail("Credenciais invalidas.", StatusCodes.Status401Unauthorized);
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return ServiceResult<AuthUserDto>.Fail("Credenciais invalidas.", StatusCodes.Status401Unauthorized);
        }

        return ServiceResult<AuthUserDto>.Ok(new AuthUserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        });
    }
}

