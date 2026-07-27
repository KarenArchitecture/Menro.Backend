using Menro.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult FromResult(Result result, string successMessage)
        {
            if (result.IsSuccess)
                return Ok(new { message = successMessage });

            return result.ErrorCode switch
            {
                ErrorCode.Duplicate => Conflict(new { message = result.Error }),
                ErrorCode.NotFound => NotFound(new { message = result.Error }),
                ErrorCode.Invalid => BadRequest(new { message = result.Error }),
                _ => BadRequest(new { message = result.Error }),
            };
        }
    }
}