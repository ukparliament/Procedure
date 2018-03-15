using System.Web.Mvc;
using System.Web.Routing;

namespace Procedure.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RouteTable.Routes.MapRoute(
                   name: "Default",
                   url: "{controller}/{action}/{id}",
                   defaults: new { controller = "Procedure", action = "Index", id = UrlParameter.Optional }
               );
        }
    }
}
