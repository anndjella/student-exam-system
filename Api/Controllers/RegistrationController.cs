using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize(Roles ="StudentService",Policy ="PasswordChanged")]
    public class RegistrationController : ControllerBase
    {
        //[HttpGet]
        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
