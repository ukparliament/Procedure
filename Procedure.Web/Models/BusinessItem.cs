using Newtonsoft.Json;
using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class BusinessItem : BaseSharepointItem
    {
        public List<SharepointLookupItem> BelongsTo { get; set; }

        [JsonProperty(PropertyName = "ActualisesProcedureStep_x003a_Tr")]
        public List<SharepointLookupItem> Actualises { get; set; }
    }
}