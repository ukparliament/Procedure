using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Procedure.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            TelemetryConfiguration.Active.InstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process)??string.Empty;
            RouteTable.Routes.MapMvcAttributeRoutes();           
        }
    }
}
