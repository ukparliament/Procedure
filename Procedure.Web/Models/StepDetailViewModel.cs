using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class StepDetailViewModel
    {
        public StepItem Step { get; set; }
        public List<RouteItem> Routes { get; set; }
        public List<BusinessItem> BusinessItems { get; set; }
    }
}