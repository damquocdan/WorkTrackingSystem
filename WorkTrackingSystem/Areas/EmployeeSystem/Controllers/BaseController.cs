using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WorkTrackingSystem.Areas.EmployeeSystem.Controllers
{
        [Area("EmployeeSystem")]
        public class BaseController : Controller, IActionFilter
        {
            public override void OnActionExecuted(ActionExecutedContext context)
            {
                if (context.HttpContext.Session.GetString("AdminLogin") == null)
                {
                    context.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { Controller = "Login", Action = "Index", Areas = "EmployeeSystem" }));
                }
                base.OnActionExecuted(context);
            }
        }
    }
