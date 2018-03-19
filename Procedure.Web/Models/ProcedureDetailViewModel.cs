using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class ProcedureDetailViewModel
    {
        public string ProcedureName { get; set; }
        public List<ProcedureRouteItem> Routes { get; set; }
    }
}