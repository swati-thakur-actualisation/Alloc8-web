using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alloc8_web.Controllers
{
    [AllowAnonymous]
    public class OauthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [Route("/Firefly/Clickup/redirectUrl")]
        public async Task<IActionResult> redirectUrl()
        {
            try
            {
                var queryString = HttpContext.Request.QueryString.Value;
                if (!string.IsNullOrEmpty(queryString))
                {
                    var parameters = queryString.Split('&');
                    var codeParam = parameters.FirstOrDefault(p => p.StartsWith("code="));
                    if (codeParam != null)
                    {
                        var code = codeParam.Split('=')[1];
                        return Content(code); 
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while processing the request.");
            }

            return BadRequest("No code found in the request.");
        }

    }
}
