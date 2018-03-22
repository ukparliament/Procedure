using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class WorkPackageRoute
    {
        public RouteType RouteKind { get; set; }
        public StepItem Step { get; set; }
        public List<BaseSharepointItem> BusinessItems { get; set; }
        public List<WorkPackageRoute> FollowingRoutes { get; set; }
    }
}