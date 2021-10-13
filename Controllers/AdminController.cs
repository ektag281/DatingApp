using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Api.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUserWithRoles()
        {
            var users = await _userManager.Users
            .Include(c => c.UserRoles)
            .ThenInclude(r => r.Role)
            .OrderBy(c => c.UserName)
            .Select( u => new 
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(c =>  c.Role.Name).ToList()
            })
            .ToListAsync();

            return Ok(users);
        }

        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, 
                                        [FromQuery] string roles)
        {
            var selectedRoles = roles.Split(",").ToArray();

            var user = await _userManager.FindByNameAsync(username);

            if(user == null) return NotFound("Could not found user");

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if(!result.Succeeded)
                return BadRequest("Failed to remove from roles!");
            
            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotoForModeration()
        {
            var users = await _userManager.Users
            .Include(c => c.UserRoles)
            .ThenInclude(r => r.Role)
            .OrderBy(c => c.UserName)
            .Select( u => new 
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(c =>  c.Role.Name).ToList()
            })
            .ToListAsync();

            return Ok(users);
        }

    }
}