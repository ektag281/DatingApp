using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.Api.Entities;
using DatingApp.Api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.Api.Services
{
    public class TokenService : ITokenService
    {
        //SymmetricEncryption,only one key is used for encrypt and decrypt
        //where as asymmetricEncryption need two different key private 
        //and public for encryption
        private readonly SymmetricSecurityKey _key;
        private readonly IConfiguration config;
        private readonly UserManager<AppUser> _userManager;

        public TokenService(IConfiguration config, UserManager<AppUser> userManager)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
            this.config = config;
            _userManager = userManager;
        }

        public async Task<string> CreateToken(AppUser user)
        {
            //Putting claim inside this token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId,user.Id.ToString()),
                //Nameid used to store username
                new Claim(JwtRegisteredClaimNames.UniqueName,user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);

            //Adding roles to claim
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role,role)));

            //Create Credntial
            var cred = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

            //Describes out token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = cred
            };

            //Now create a token handler
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }

        public string CreateToken()
        {
            throw new NotImplementedException();
        }
    }
}