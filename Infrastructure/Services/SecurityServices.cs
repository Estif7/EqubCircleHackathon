using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using EkubCircle.Application.Interfaces;

namespace EkubCircle.Infrastructure.Services;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string value) => BCrypt.Net.BCrypt.HashPassword(value);
    public bool Verify(string value, string hash) => BCrypt.Net.BCrypt.Verify(value, hash);
}

public sealed class Sha256FaydaFanHasher : IFaydaFanHasher
{
    public string Hash(string fanNumber)
    {
        var normalized = fanNumber.Trim();
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }
}

