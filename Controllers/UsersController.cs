using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.Api.Data;
using DatingApp.Api.DTOs;
using DatingApp.Api.Entities;
using DatingApp.Api.Extensions;
using DatingApp.Api.Helpers;
using DatingApp.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Api.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRespository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRespository, IMapper mapper, IPhotoService photoService)
        {
            _mapper = mapper;
            _photoService = photoService;
            _userRespository = userRespository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var user = await _userRespository.GetUserByUsernameAsync(User.GetUserName());
            userParams.CurrentUserName = user.UserName;

            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = user.Gender == "male" ? "female" : "male";
            }

            var users = await _userRespository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users.CurrentPage,users.PageSize,
            users.TotalCounts,users.TotalPages);
            return Ok(users);
        }

        //[Authorize]
        [HttpGet("{username}",Name ="GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await _userRespository.GetMemberAsync(username);

            return user;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            //Getting username from Token
            var username = User.GetUserName();
            var user = await _userRespository.GetUserByUsernameAsync(username);

            _mapper.Map(memberUpdateDto, user);
            _userRespository.Update(user);

            if(await _userRespository.SaveAllAsync()){
                return NoContent();
            }

            return BadRequest("Failed to update");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userRespository.GetUserByUsernameAsync(User.GetUserName());
            var result = await _photoService.AddPhotoAsync(file);
            if(result.Error != null){
                return BadRequest(result.Error.Message);
            }

            var photo =  new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if(user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if(await _userRespository.SaveAllAsync())
            {

                //return CreatedAtRoute("GetUser",_mapper.Map<PhotoDto>(photo));
                return CreatedAtRoute("GetUser", new { username = user.UserName} ,_mapper.Map<PhotoDto>(photo));
            }
            //return _mapper.Map<PhotoDto>(photo);

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRespository.GetUserByUsernameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault( c => c.Id == photoId);

            if(photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x=> x.IsMain);

            if(currentMain != null)
            {
                currentMain.IsMain = false;
            }
            photo.IsMain = true;

            if(await _userRespository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRespository.GetUserByUsernameAsync(User.GetUserName());
            
            var photo = user.Photos.FirstOrDefault( c => c.Id == photoId);

            if(photo == null) return NotFound();

            if(photo.IsMain) return BadRequest("You can not delete your main photo");

            //remove from cloudinary
            if(photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null)
                {
                    return BadRequest(result.Error.Message);
                }
            }

            user.Photos.Remove(photo);
            if(await _userRespository.SaveAllAsync()) return Ok();
            
            return BadRequest("Failed to delete the photo") ;
        }

    }
}