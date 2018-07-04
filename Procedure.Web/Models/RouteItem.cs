using Newtonsoft.Json;
using Parliament.Model;
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

        public string TripleStoreId { get; set; }

        [JsonProperty(PropertyName = "FromStep_x003a_TripleStoreId")]
        public SharepointLookupItem FromStepTripleStoreIdJsonObj { get; set; }

        [JsonProperty(PropertyName = "ToStep_x003a_TripleStoreId")]
        public SharepointLookupItem ToStepTripleStoreIdJsonObj { get; set; }

        public SharepointLookupItem Procedure { get; set; }


        [JsonProperty(PropertyName = "Procedure_x003a_TripleStoreId")]
        public SharepointLookupItem ProcedureTripleStoreIdJsonObj { get; set; }

        public ProcedureRoute GiveMeMappedObject()
        {
            ProcedureRoute result = new ProcedureRoute();
            result.Id = new System.Uri($"https://id.parliament.uk/{TripleStoreId}");
            result.ProcedureRouteIsToProcedureStep = new ProcedureStep[] { ToStep.ToSharepointItem<StepItem>().GiveMeMappedObject(ToStepTripleStoreIdJsonObj.Value)};
            result.ProcedureRouteIsFromProcedureStep = new ProcedureStep[] { FromStep.ToSharepointItem<StepItem>().GiveMeMappedObject(FromStepTripleStoreIdJsonObj.Value)};

            return result;
        }

    }

}