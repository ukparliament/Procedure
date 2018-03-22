using Newtonsoft.Json;

namespace Procedure.Web.Models
{
    public class RouteItem
    {
        [JsonProperty(PropertyName = "ID")]
        public int Id { get; set; }

        public SharepointLookupItem FromStep { get; set; }

        [JsonProperty(PropertyName = "RouteType")]
        public SharepointLookupItem RouteKind { get; set; }

        public SharepointLookupItem ToStep { get; set; }

    }

}