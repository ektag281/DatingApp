using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DatingApp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Api.Data
{
    public class Seed
    {
        //returning void from this method
        public static async Task SeedUsers(DataContext dbContext)
        {
            if(await dbContext.Users.AnyAsync()){
                return;
            }

            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
            foreach (var user in users)
            {
                using var hmac = new HMACSHA512();
                user.UserName = user.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Password@"));
                user.PasswordSalt = hmac.Key;

                dbContext.Users.Add(user);
            }
            await dbContext.SaveChangesAsync();

        }
    }
}