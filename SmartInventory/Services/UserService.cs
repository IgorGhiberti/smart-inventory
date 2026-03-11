using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;
using SmartInventory.DTOs.UserDTOs;
using SmartInventory.Entities;
using SmartInventory.Models;
using SmartInventory.Interfaces;

namespace SmartInventory.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHasher<User> _passwordHasher;

    public UserService(AppDbContext dbContext, PasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<ServiceResult<UserResponseDto>> CreateAsync(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return ServiceResult<UserResponseDto>.Fail("Nome, email e senha sao obrigatorios.", StatusCodes.Status400BadRequest);
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var exists = await _dbContext.Users.AnyAsync(x => x.Email == normalizedEmail);
        if (exists)
        {
            return ServiceResult<UserResponseDto>.Fail("Email ja cadastrado.", StatusCodes.Status409Conflict);
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = normalizedEmail,
            Role = dto.Role,
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return ServiceResult<UserResponseDto>.Ok(ToResponse(user), StatusCodes.Status201Created);
    }

    public async Task<List<UserResponseDto>> ListAsync()
    {
        return await _dbContext.Users
            .OrderBy(x => x.Name)
            .Select(x => new UserResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Role = x.Role,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ServiceResult<UserResponseDto>> UpdateRoleAsync(int id, UpdateUserRoleDto dto)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
        {
            return ServiceResult<UserResponseDto>.Fail("Usuario nao encontrado.", StatusCodes.Status404NotFound);
        }

        user.Role = dto.Role;
        await _dbContext.SaveChangesAsync();

        return ServiceResult<UserResponseDto>.Ok(ToResponse(user));
    }

    public async Task<ServiceResult<UserResponseDto>> SetActiveAsync(int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
        {
            return ServiceResult<UserResponseDto>.Fail("Usuario nao encontrado.", StatusCodes.Status404NotFound);
        }

        user.IsActive = true;
        await _dbContext.SaveChangesAsync();

        return ServiceResult<UserResponseDto>.Ok(ToResponse(user));
    }
    public async Task<ServiceResult<string>> DeleteAsync(int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
        {
            return ServiceResult<string>.Fail("Usuario nao encontrado.", StatusCodes.Status404NotFound);
        }

        user.IsActive = false;
        await _dbContext.SaveChangesAsync();
        return ServiceResult<string>.Ok("Usuário desativado.", StatusCodes.Status204NoContent);
    }

    private static UserResponseDto ToResponse(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }
}

