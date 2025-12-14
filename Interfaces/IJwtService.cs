using IEEEBackend.Models;

namespace IEEEBackend.Interfaces;

public interface IJwtService
{
    string GenerateToken(Admin admin);
}

