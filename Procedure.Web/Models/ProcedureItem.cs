using Parliament.Model;

namespace Procedure.Web.Models
{
    public class ProcedureItem : BaseSharepointItem
    {
        public string TripleStoreId { get; set; }

        public IProcedure GiveMeMappedObject(string tripleStoreId)
        {
            string id = tripleStoreId ?? TripleStoreId;
            IProcedure result = new Parliament.Model.Procedure();
            result.ProcedureName = Title;
            result.Id = new System.Uri($"https://id.parliament.uk/{id}");

            return result;
        }
    }
}