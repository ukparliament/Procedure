using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class BusinessItem : BaseSharepointTable
    {
        public ReferenceTable BelongsTo { get; set; }
        public List<ReferenceTable> Actualises { get; set; }
    }
}