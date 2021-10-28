using System;
using System.Threading.Tasks;
using DatingApp.Api.Extensions;
using DatingApp.Api.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.Api.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            if(!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

            //Getting username
            //var username = resultContext.HttpContext.User.GetUserName();
            //var repo.user = resultContext.HttpContext.RequestServices.GetService<IUserRepo.usersitory>();
            //var user = await repo.user.GetUserByUsernameAsync(username);
            //user.LastActive = DateTime.Now;
            //await repo.user.SaveAllAsync();

            //Getting userId
            var userId = resultContext.HttpContext.User.GetUserId();
            var uow = resultContext.HttpContext.RequestServices.GetService<IUnitOfWork>();
            var user = await uow.UserRepository.GetUserByIdAsync(userId);
            user.LastActive = DateTime.UtcNow;
            await uow.Complete();
        }
    }
}