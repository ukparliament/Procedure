using Parliament.Model;

namespace Procedure.Web.Models
{
    public class StepItem : BaseSharepointItem
    {
        public string TripleStoreId { get; set; }

        public string Description { get; set; }

        public IProcedureStep GiveMeMappedObject(string tripleStoreId)
        {
            string id = tripleStoreId ?? TripleStoreId;
            IProcedureStep result = new Parliament.Model.ProcedureStep();
            result.Id = new System.Uri($"https://id.parliament.uk/{id}");
            result.ProcedureStepName = Title;

            return result;
        }

    }
}