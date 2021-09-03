using DatingApp.Api.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(LogUserActivity))]
    public class BaseApiController : ControllerBase
    {
        
    }
}