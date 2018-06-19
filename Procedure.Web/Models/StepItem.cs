using Parliament.Model;

namespace Procedure.Web.Models
{
    public class StepItem : BaseSharepointItem
    {
        public string TripleStoreId { get; set; }

        public string Description { get; set; }

        public SharepointLookupItem[] House { get; set; }

        public ProcedureStep GiveMeMappedObject(string tripleStoreId)
        {
            string id = tripleStoreId ?? TripleStoreId;
            ProcedureStep result = new ProcedureStep();
            result.Id = new System.Uri($"https://id.parliament.uk/{id}");
            result.ProcedureStepName = Title;
            result.ProcedureStepDescription = Description;

            return result;
        }

    }
}