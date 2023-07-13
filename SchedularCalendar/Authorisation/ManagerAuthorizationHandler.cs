using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using SchedularCalendar.Models;
using SchedularCalendar.Authorisation;

namespace LibraryManagmentSystem.Authorization
{
    public class ManagerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Event>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement, Event @event)
        {
            if (context.User == null || @event == null)
            {
                return Task.CompletedTask;
            }

            if ( requirement.Name != Constants.ApproveOperationName &&
                requirement.Name != Constants.RejectOperationName)
            {
                return Task.CompletedTask;
            }

            if (context.User.IsInRole(Constants.ManagerRole))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
