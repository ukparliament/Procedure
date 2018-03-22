using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class PathwayRouteViewModel
    {
        public List<RouteItem> Routes { get; set; }
        public int FromStepId { get; set; }
    }
}