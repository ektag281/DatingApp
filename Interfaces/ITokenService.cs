using System.Threading.Tasks;
using DatingApp.Api.Entities;

namespace DatingApp.Api.Interfaces
{
    public interface ITokenService
    {
        public Task<string> CreateToken(AppUser user);
    }
}