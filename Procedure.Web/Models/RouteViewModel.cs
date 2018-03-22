using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class RouteViewModel
    {
        public List<RouteItem> Routes { get; set; }
        public List<RouteItem> Children { get; set; }
    }
}