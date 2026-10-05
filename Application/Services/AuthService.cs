using EkubCircle.Application.Interfaces;
using EkubCircle.Application.DTOs;
using EkubCircle.Domain.Enums;
using EkubCircle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EkubCircle.Application.Services;

public class AuthService(IApplicationDbContext db, IConfiguration configuration, IPasswordHasher passwordHasher, IFaydaFanHasher faydaFanHasher, ITokenService tokenService) : IAuthService
{
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            throw new InvalidOperationException("Passwords do not match.");
        if (request.Password.Length < 6)
            throw new InvalidOperationException("Password must contain at least 6 characters.");
        if (string.IsNullOrWhiteSpace(request.FaydaFanNumber))
            throw new InvalidOperationException("Fayda FAN is required.");
        if (await db.Users.AnyAsync(x => x.Email == request.Email.Trim().ToLower()))
            throw new InvalidOperationException("Email is already registered.");
        if (await db.Users.AnyAsync(x => x.PhoneNumber == request.PhoneNumber.Trim()))
            throw new InvalidOperationException("Phone number is already registered.");

        var fanHash = faydaFanHasher.Hash(request.FaydaFanNumber);
        if (await db.Users.AnyAsync(x => x.FaydaFanHash == fanHash))
            throw new InvalidOperationException("Fayda FAN is already registered.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PhoneNumber = request.PhoneNumber.Trim(),
            PasswordHash = passwordHasher.Hash(request.Password),
            FaydaFanHash = fanHash,
            FaydaVerified = false
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var otp = Random.Shared.Next(100000, 999999).ToString();
        db.OtpVerifications.Add(new OtpVerification
        {
            UserId = user.Id,
            PhoneNumber = user.PhoneNumber,
            OtpHash = passwordHasher.Hash(otp),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });
        await db.SaveChangesAsync();

        var exposeDemoOtp = !bool.TryParse(configuration["Hackathon:ExposeDemoOtp"], out var configuredExpose) || configuredExpose;
        return new RegisterResponse(user.Id, "Account created. Verify the simulated Fayda OTP.",
            exposeDemoOtp ? otp : null);
    }

    public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var user = await db.Users.FindAsync(request.UserId)
                   ?? throw new KeyNotFoundException("User not found.");

        var otp = await db.OtpVerifications
            .Where(x => x.UserId == user.Id && x.VerifiedAt == null)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No active OTP was found.");

        if (otp.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("OTP has expired.");
        if (otp.Attempts >= 5)
            throw new InvalidOperationException("Too many OTP attempts.");

        otp.Attempts++;
        if (!passwordHasher.Verify(request.Otp, otp.OtpHash))
        {
            await db.SaveChangesAsync();
            throw new InvalidOperationException("Invalid OTP.");
        }

        otp.VerifiedAt = DateTime.UtcNow;
        user.FaydaVerified = true;
        await db.SaveChangesAsync();

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var identifier = request.EmailOrPhone.Trim().ToLower();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == identifier || x.PhoneNumber == request.EmailOrPhone.Trim())
                   ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");
        if (!user.FaydaVerified)
            throw new UnauthorizedAccessException("Account is not verified. Complete OTP verification first.");

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var role = UserRole.Member.ToString();
        return new AuthResponse(user.Id, user.FullName, user.Email, role, tokenService.CreateToken(user));
    }
}
