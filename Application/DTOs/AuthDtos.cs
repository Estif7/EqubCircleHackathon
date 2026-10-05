namespace EkubCircle.Application.DTOs;

public record RegisterRequest(
    string FullName,
    string PhoneNumber,
    string Email,
    string Password,
    string ConfirmPassword,
    string FaydaFanNumber);

public record VerifyOtpRequest(int UserId, string Otp);
public record LoginRequest(string EmailOrPhone, string Password);
public record AuthResponse(int UserId, string FullName, string Email, string Role, string Token);
public record RegisterResponse(int UserId, string Message, string? DemoOtp);
