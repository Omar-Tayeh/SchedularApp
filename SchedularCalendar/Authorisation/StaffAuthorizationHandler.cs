using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using SchedularCalendar.Models;
using Microsoft.AspNetCore.Identity;

namespace SchedularCalendar.Authorisation
{
    public class StaffAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Event>
    {
        private readonly UserManager<IdentityUser> _userManager;

        public StaffAuthorizationHandler(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Event @event)
        {
            if (context.User == null)
            {
                return Task.CompletedTask;
            }

            if(requirement.Name != Constants.CreateOperationName &&
                requirement.Name != Constants.UpdateOperationName &&
                requirement.Name != Constants.DeleteOperationName &&
                requirement.Name != Constants.ReadOperationName)
            {
                return Task.CompletedTask;
            }

            if (@event.CreatorId == _userManager.GetUserId(context.User))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
