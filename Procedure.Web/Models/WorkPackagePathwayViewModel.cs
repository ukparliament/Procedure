using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class WorkPackagePathwayViewModel
    {
        public WorkPackageItem WorkPackage { get; set; }
        public List<WorkPackageRoute> Routes { get; set; }
    }
}