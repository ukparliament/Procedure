using Newtonsoft.Json;

namespace Procedure.Web.Models
{
    public class ProcedureRouteItem
    {
        [JsonProperty(PropertyName = "ID")]
        public int Id { get; set; }

        public ReferenceTable FromStep { get; set; }

        public ReferenceTable RouteType { get; set; }

        public ReferenceTable ToStep { get; set; }

    }

}