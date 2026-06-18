using CurrencyExchange.Core;
using CurrencyExchange.Core.DTOs.User;
using CurrencyExchange.Core.Entities;
using CurrencyExchange.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.API.Controllers;

/// <summary>CRUD operations for domain Users. Admin-only for most operations.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;

    public UsersController(IUserRepository repo) => _repo = repo;

    // ── GET /api/users ────────────────────────────────────────────────────────

    /// <summary>Returns all users.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _repo.GetAllAsync();
        return Ok(users.Select(ToDto));
    }

    // ── GET /api/users/{id} ──────────────────────────────────────────────────

    /// <summary>Returns a single user by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        return user is null ? NotFound(new { message = $"User {id} not found." }) : Ok(ToDto(user));
    }

    // ── GET /api/users/by-email?email= ───────────────────────────────────────

    /// <summary>Looks up a user by email address.</summary>
    [HttpGet("by-email")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email query parameter is required." });

        var user = await _repo.GetByEmailAsync(email);
        return user is null ? NotFound(new { message = $"No user found with email '{email}'." }) : Ok(ToDto(user));
    }

    // ── POST /api/users ──────────────────────────────────────────────────────

    /// <summary>Creates a new user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (await _repo.ExistsAsync(u => u.Email == dto.Email))
            return Conflict(new { message = $"A user with email '{dto.Email}' already exists." });

        var user = new User
        {
            FullName     = dto.FullName.Trim(),
            Email        = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCryptHash(dto.Password),
            IdentityId   = dto.IdentityId
        };

        await _repo.AddAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToDto(user));
    }

    // ── PUT /api/users/{id} ──────────────────────────────────────────────────

    /// <summary>Updates an existing user.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _repo.GetByIdAsync(id);
        if (user is null)
            return NotFound(new { message = $"User {id} not found." });

        // Guard duplicate email (exclude current user)
        if (await _repo.ExistsAsync(u => u.Email == dto.Email.Trim().ToLowerInvariant() && u.Id != id))
            return Conflict(new { message = $"Email '{dto.Email}' is already used by another user." });

        user.FullName   = dto.FullName.Trim();
        user.Email      = dto.Email.Trim().ToLowerInvariant();
        user.IdentityId = dto.IdentityId;

        await _repo.UpdateAsync(user);
        return NoContent();
    }

    // ── DELETE /api/users/{id} ───────────────────────────────────────────────

    /// <summary>Deletes a user and their wallets (cascade).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user is null)
            return NotFound(new { message = $"User {id} not found." });

        await _repo.DeleteAsync(user);
        return NoContent();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static UserDto ToDto(User u) => new(u.Id, u.FullName, u.Email, u.IdentityId);

    // Simple placeholder — in production use ASP.NET Identity's password hasher
    private static string BCryptHash(string password)
        => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password + "_hashed"));
}
