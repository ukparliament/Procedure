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

        public static string ListSql = @"select p.Id, p.ProcedureName as Title, p.TripleStoreId from [Procedure] p";

        public static string ItemSql = @"select p.Id, p.ProcedureName as Title, p.TripleStoreId from [Procedure] p where p.Id=@Id";
    }
}