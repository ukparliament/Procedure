using Newtonsoft.Json;
using System;

namespace Procedure.Web.Models
{
    public class RouteTypeItem : BaseSharepointItem
    {
        [JsonIgnore]
        public RouteType RouteKind
        {
            get
            {
                return (RouteType)Enum.Parse(typeof(RouteType), Title);
            }
        }
    }
}