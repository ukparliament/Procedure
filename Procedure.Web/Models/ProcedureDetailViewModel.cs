using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class ProcedureDetailViewModel
    {
        public ProcedureItem Procedure { get; set; }
        public List<StepItem> EntryPoints { get; set; }
        public List<RouteItem> Routes { get; set; }
    }
}