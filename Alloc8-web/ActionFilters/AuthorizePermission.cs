using Alloc8_web.Services.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Alloc8_web.Filters
{
    public class AuthorizePermission : ActionFilterAttribute
    {
        private readonly string _permissionName;
        private readonly string _permissionType;

        public AuthorizePermission(string permissionName, string permissionType)
        {
            _permissionName = permissionName;
            _permissionType = permissionType;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var auth = context.HttpContext.RequestServices.GetService<IAuth>();

            if (auth != null && auth.hasPermission(_permissionName, _permissionType))
            {
                base.OnActionExecuting(context);
            }
            else
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
