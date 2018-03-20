using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class WorkPackagePathwayViewModel
    {
        public WorkPackageItem WorkPackage { get; set; }
        public List<BusinessItem> BusinessItems { get; set; }
        public List<ProcedureRouteItem> Routes { get; set; }
    }
}