using Newtonsoft.Json;

namespace Procedure.Web.Models
{
    public class ProcedureRouteItem
    {
        [JsonProperty(PropertyName = "ID")]
        public int Id { get; set; }

        [JsonIgnore]
        public int? FromStepId
        {
            get
            {
                return FromStep?.Id;
            }
        }

        [JsonIgnore]
        public string FromStepText
        {
            get
            {
                return FromStep?.Value;
            }
        }

        [JsonIgnore]
        public int? ToStepId
        {
            get
            {
                return ToStep?.Id;
            }
        }

        [JsonIgnore]
        public string ToStepText
        {
            get
            {
                return ToStep?.Value;
            }
        }

        [JsonIgnore]
        public int? RouteTypeId
        {
            get
            {
                return RouteType?.Id;
            }
        }

        [JsonIgnore]
        public string RouteTypeText
        {
            get
            {
                return RouteType?.Value;
            }
        }

        public ReferenceTable FromStep { get; set; }

        public ReferenceTable RouteType { get; set; }

        public ReferenceTable ToStep { get; set; }

    }

}