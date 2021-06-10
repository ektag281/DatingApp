using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.Api.Data;
using DatingApp.Api.DTOs;
using DatingApp.Api.Entities;
using DatingApp.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Api.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRespository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRespository, IMapper mapper)
        {
            _mapper = mapper;
            _userRespository = userRespository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var user = await _userRespository.GetMembersAsync();

            return Ok(user);
        }

        //[Authorize]
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await _userRespository.GetMemberAsync(username);

            return user;
        }
    }
}