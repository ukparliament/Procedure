using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Procedure.Web.Models
{
    public class WorkPackageProcedureItem
    {
        public int Id { get; set; }
        public int WorkPackageId { get; set; }
        public int SubjectToProcedureId { get; set; }
    }
}