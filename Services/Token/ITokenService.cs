using Security_Bug_Reports.Models;
using System.Security.Claims;

namespace Services.Token
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
