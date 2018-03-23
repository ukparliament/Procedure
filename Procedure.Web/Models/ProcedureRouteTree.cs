using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class ProcedureRouteTree: RouteStep
    {
        public List<ProcedureRouteTree> ChildrenRoutes { get; set; }
    }
}