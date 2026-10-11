using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Middleware.Security
{
    //https://www.craftedforeveryone.com/adding-your-own-custom-authorize-attribute-to-asp-net-core-2-2-and-above/
    //https://www.codeproject.com/Articles/5247609/ASP-NET-CORE-Token-Authentication-and-Authorizat-2
    public class ApiAuthorizeAttribute : TypeFilterAttribute
    {
        public ApiAuthorizeAttribute(params string[] claim) : base(typeof(ApiAuthorizeFilter))
        {
            Arguments = new object[] { claim };
        }
    }
    public class AdminAuthorizeAttribute : TypeFilterAttribute
    {
        public AdminAuthorizeAttribute() : base(typeof(AdminAuthorizeFilter))
        {
            Arguments = Array.Empty<object>();
        }
    }
    public class AdminAuthorizeFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            var isAuthenticated = user.Identity?.IsAuthenticated;

            if (isAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check UserTypeId claim (7 = Admin / SuperAdmin, 1 = Consultant / Recruiter Admin if granted, etc.)
            var userTypeClaim = user.FindFirst("UserTypeId")?.Value;
            if (int.TryParse(userTypeClaim, out int userTypeId))
            {
                // UserTypeId 7 is the dedicated System / Super Admin
                if (userTypeId == 7)
                {
                    return;
                }
            }

            // If not an admin role, deny with 403 Forbidden
            context.Result = new ForbidResult();
        }
    }
    public class ApiAllowAnonymousAttribute : AllowAnonymousAttribute
    {
       
    }
    public class ApiAuthorizeFilter : IAuthorizationFilter
    {
        readonly string[] _claim;

        public ApiAuthorizeFilter(params string[] claim)
        {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var IsAuthenticated = context.HttpContext.User.Identity?.IsAuthenticated;
            var claimsIndentity = context.HttpContext.User.Identity as ClaimsIdentity;

            if (IsAuthenticated != null && IsAuthenticated.Value)
            {
                bool flagClaim = false;
                foreach (var item in _claim)
                {
                    if (context.HttpContext.User.HasClaim("Permissions", item))
                        flagClaim = true;
                }
                if (!flagClaim)
                    context.Result = new UnauthorizedResult();
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
            return;
        }
    }
}
