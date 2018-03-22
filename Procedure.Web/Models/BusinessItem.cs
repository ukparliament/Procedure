using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class BusinessItem : BaseSharepointItem
    {
        public List<SharepointLookupItem> BelongsTo { get; set; }
        public List<SharepointLookupItem> Actualises { get; set; }
    }
}