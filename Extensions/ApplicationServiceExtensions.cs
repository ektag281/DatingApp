using DatingApp.Api.Data;
using DatingApp.Api.Helpers;
using DatingApp.Api.Interfaces;
using DatingApp.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.Api.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<ITokenService,TokenService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<LogUserActivity>();
            services.AddScoped<IUserRepository,UserRepository>();
            services.AddScoped<ILikesRepository, LikesRepository>();
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

            services.AddCors();
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(config.GetConnectionString("defaultConnection"));
            });

            return services;
        }
        
    }
}