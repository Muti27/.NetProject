using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error")]
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var exception = feature?.Error;

            _logger.LogError(exception, "全域例外發生於 {Path}", feature?.Path ?? "未知路徑");

            ViewData["ErrorMessage"] = exception?.Message ?? "系統發生錯誤";
            ViewData["Path"] = feature?.Path ?? "未知路徑";

            return View("Error");
        }

        [Route("Error/Status/{statusCode}")]
        public IActionResult StatusCodeHandler(int statusCode)
        {
            ViewData["StatusCode"] = statusCode;
            return View("Status");
        }
    }
}
