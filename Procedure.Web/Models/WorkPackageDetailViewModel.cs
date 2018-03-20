using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class WorkPackageDetailViewModel
    {
        public string WorkPackageName { get; set; }
        public List<BusinessItem> BusinessItems { get; set; }
    }
}