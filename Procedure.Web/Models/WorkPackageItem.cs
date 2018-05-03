using Newtonsoft.Json;
using Parliament.Model;

namespace Procedure.Web.Models
{

    public class WorkPackageItem : BaseSharepointItem
    {
        public SharepointLookupItem SubjectTo { get; set; }

        [JsonProperty(PropertyName = "SubjectTo_x003a_TripleStoreId")]
        public SharepointLookupItem SubjectToTripleStoreIdJsonObj { get; set; }

        [JsonProperty(PropertyName = "OData__x0076_dy9")]
        public string TripleStoreId { get; set; }

        public SharepointLookupItem WorkPackagableThingType { get; set; }

        // Note Title is property of WorkPackageableThing instead of WorkPackage  
        public IWorkPackage GiveMeMappedObject(string tripleStoreId)
        {
            string id = tripleStoreId ?? TripleStoreId;
            IWorkPackage result = new WorkPackage();
            result.Id = new System.Uri($"https://id.parliament.uk/{id}");
            result.WorkPackageHasProcedure = SubjectTo.ToSharepointItem<ProcedureItem>().GiveMeMappedObject(SubjectToTripleStoreIdJsonObj.Value);

            return result;
        }
    }
}