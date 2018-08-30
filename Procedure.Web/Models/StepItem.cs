using Parliament.Model;
using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class StepItem : BaseSharepointItem
    {
        public string TripleStoreId { get; set; }

        public string Description { get; set; }

        public IEnumerable<ProcedureStepHouse> Houses { get; set; }

        public ProcedureStep GiveMeMappedObject(string tripleStoreId)
        {
            string id = tripleStoreId ?? TripleStoreId;
            ProcedureStep result = new ProcedureStep();
            result.Id = new System.Uri($"https://id.parliament.uk/{id}");
            result.ProcedureStepName = Title;
            result.ProcedureStepDescription = Description;

            return result;
        }

        public static string ListSql = @"select ps.Id, ps.TripleStoreId, ps.ProcedureStepName as Title,
	             ps.ProcedureStepName as [Description]
            from ProcedureStep ps";

        public static string ItemSql = @"select ps.Id, ps.TripleStoreId, ps.ProcedureStepName as Title,
	             ps.ProcedureStepName as [Description]
            from ProcedureStep ps
            where ps.Id=@Id";

    }
}