using EkubCircle.Domain.Entities;

namespace EkubCircle.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
}
