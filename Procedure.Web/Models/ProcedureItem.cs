namespace Procedure.Web.Models
{
    public class ProcedureItem : BaseSharepointItem
    {
        public string TripleStoreId { get; set; }

        public Parliament.Model.Procedure GiveMeMappedObject(string tripleStoreId)
        {
            string id = tripleStoreId ?? TripleStoreId;
            Parliament.Model.Procedure result = new Parliament.Model.Procedure();
            result.ProcedureName = Title;
            result.Id = new System.Uri($"https://id.parliament.uk/{id}");

            return result;
        }
    }
}