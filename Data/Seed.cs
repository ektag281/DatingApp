using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DatingApp.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Api.Data
{
    public class Seed
    {
        //returning void from this method
        public static async Task SeedUsers(UserManager<AppUser> userManager,
                                           RoleManager<AppRole> roleManager)
        {
            if(await userManager.Users.AnyAsync()) return;

            var roles = new List<AppRole>
            {
                new AppRole(){ Name = "Member" },
                new AppRole(){ Name = "Admin" },
                new AppRole(){ Name = "Moderator" }
            };

            foreach (var role in roles)
            {
              await roleManager.CreateAsync(role);  
            }

            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
            foreach (var user in users)
            {
                user.UserName = user.UserName.ToLower();
                // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Password@"));
                await userManager.CreateAsync(user,"Password@1");
                await userManager.AddToRoleAsync(user, "Member");
            }

            //Adding admin user on DN
            var admin = new AppUser
            {
                UserName = "admin"
            };

            await userManager.CreateAsync(admin,"Password@1");
            await userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"});
        }
    }
}