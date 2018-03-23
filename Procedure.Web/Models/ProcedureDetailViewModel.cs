using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class ProcedureDetailViewModel
    {
        public ProcedureItem Procedure { get; set; }
        public List<ProcedureRouteTree> Tree { get; set; }
    }
}