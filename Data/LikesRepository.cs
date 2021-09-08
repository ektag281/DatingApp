using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.Api.DTOs;
using DatingApp.Api.Entities;
using DatingApp.Api.Extensions;
using DatingApp.Api.Helpers;
using DatingApp.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Api.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext context;

        public LikesRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await context.Likes.FindAsync(sourceUserId,likedUserId);
        }

        public async  Task<PagedList<LikeDto>> GetUserLikes(LikesParam likesParam)
        {
            var users = context.Users.OrderBy(c => c.UserName).AsQueryable();
            var likes = context.Likes.AsQueryable();

            if(likesParam.Predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == likesParam.UserId);
                users = likes.Select(like => like.LikedUser);
            }

            if(likesParam.Predicate == "likedBy")
            {
                likes = likes.Where(like => like.LikedUserId == likesParam.UserId);
                users = likes.Select(like => like.SourceUser);
            }

            var likedUsers =  users.Select(user => new LikeDto{
                UserName = user.UserName,
                Age = user.DateOfBirth.calculateAge(),
                City = user.City,
                KnownAs = user.KnowAs,
                PhotoUrl = user.Photos.FirstOrDefault(c => c.IsMain).Url,
                Id = user.Id
            });

            return await PagedList<LikeDto>.CreateAsync(likedUsers,likesParam.PageNumber,likesParam.PageSize) ;
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await context.Users
                    .Include(c => c.LikedUsers)
                    .FirstOrDefaultAsync(c => c.Id == userId);
        }
    }
}