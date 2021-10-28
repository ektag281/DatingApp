using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.Api.DTOs;
using DatingApp.Api.Entities;
using DatingApp.Api.Extensions;
using DatingApp.Api.Helpers;
using DatingApp.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Api.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        public LikesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

            //Get user with likes
            var sourceUser = await _unitOfWork.LikesRepository.GetUserWithLikes(sourceUserId);

            if(likedUser == null)
            {
                return NotFound();
            }
            if(sourceUser.UserName == username)
            {
                return BadRequest("You can not like yourself!");
            }

            var userLike = await _unitOfWork.LikesRepository.GetUserLike(sourceUserId,likedUser.Id);

            if(userLike != null)
            {
                return BadRequest("You already liked this user");
            }

            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);
            if(await _unitOfWork.Complete())
            {
                return Ok();
            }
            return BadRequest("Failed to like User");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParam likesParam)
        {
            likesParam.UserId = User.GetUserId();
            var users =  await _unitOfWork.LikesRepository.GetUserLikes(likesParam);

            Response.AddPaginationHeader(users.CurrentPage,
            users.PageSize,users.TotalCounts,users.TotalPages);
            return Ok(users);
        }
    }
}