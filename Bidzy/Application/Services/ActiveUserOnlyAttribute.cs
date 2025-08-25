using System.Security.Claims;
using Bidzy.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Bidzy.Application.Services
{
    public class ActiveUserOnlyAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly ApplicationDbContext dbContext;

        public ActiveUserOnlyAttribute(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = dbContext.Users.Find(userId);

            if (user == null)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
