using EkubCircle.Application.DTOs;

namespace EkubCircle.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
