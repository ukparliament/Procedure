using Newtonsoft.Json;
using System;

namespace Procedure.Web.Models
{
    public class RouteItem
    {
        [JsonProperty(PropertyName = "ID")]
        public int Id { get; set; }

        public SharepointLookupItem FromStep { get; set; }

        [JsonProperty(PropertyName = "RouteType")]
        public SharepointLookupItem RouteTypeItem { get; set; }

        public SharepointLookupItem ToStep { get; set; }

        [JsonIgnore]
        public RouteType RouteKind
        {
            get
            {
                return (RouteType)Enum.Parse(typeof(RouteType), RouteTypeItem?.Value ?? RouteType.None.ToString());
            }
        }

    }

}