using DatingApp.Api.Entities;

namespace DatingApp.Api.Interfaces
{
    public interface ITokenService
    {
        public string CreateToken(AppUser user);
    }
}