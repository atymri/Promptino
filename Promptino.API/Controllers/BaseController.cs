using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Promptino.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected Guid? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            return Guid.TryParse(value, out var id) && id != Guid.Empty ? id : null;
        }

        protected ActionResult InvalidUserProblem()
            => Problem(
                "هویت کاربر از توکن قابل خواندن نیست.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "احراز هویت نامعتبر");
    }
}
