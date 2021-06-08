using System;
using DatingApp.Api.Data;
using DatingApp.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Api.Controllers
{
    public class BuggyController : BaseApiController
    {
        private readonly DataContext context;
        public BuggyController(DataContext context)
        {
            this.context = context;
        }

        [HttpGet("auth")]
        //[Authorize]
        public ActionResult<string> GetSecret(){
            return "Secret code";
        }

        [HttpGet("not-found")]
        //[Authorize]
        public ActionResult<AppUser> GetNotFound(){
            var thing = context.Users.Find(-1);
            if(thing == null) return NotFound();

            return Ok(thing);
        }

        [HttpGet("server-error")]
        //[Authorize]
        public ActionResult<string> GetServerError(){
                var thing = context.Users.Find(-1);

                var thingToReturn = thing.ToString();

                return thingToReturn;     
        }

        [HttpGet("bad-request")]
        //[Authorize]
        public ActionResult<string> GetBadRequest(){
            return BadRequest("This is not the good request");
        }

    }
}