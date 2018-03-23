using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class WorkPackageRouteTree: RouteStep
    {
        public bool IsPrecluded { get; set; }
        public List<BaseSharepointItem> BusinessItems { get; set; }
        public List<WorkPackageRouteTree> ChildrenRoutes { get; set; }
    }
}