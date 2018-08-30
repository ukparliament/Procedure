namespace Procedure.Web.Models
{
    public class BusinessItemStep
    {
        public int BusinessItemId { get; set; }
        public int StepId { get; set; }
        public string StepName { get; set; }

        public static string ListByWorkPackageSql = @"select bi.Id as BusinessItemId,
                ps.Id as StepId, ps.ProcedureStepName as StepName
            from ProcedureBusinessItem bi
            join ProcedureWorkPackagedThing wp on wp.Id=bi.ProcedureWorkPackageId
            join ProcedureBusinessItemProcedureStep bips on bips.ProcedureBusinessItemId=bi.Id
            join ProcedureStep ps on ps.Id=bips.ProcedureStepId
            where wp.Id=@WorkPackageId";

        public static string ListByStepSql = @"select bi.Id as BusinessItemId,
                ps.Id as StepId, ps.ProcedureStepName as StepName
            from ProcedureBusinessItem bi
            join ProcedureBusinessItemProcedureStep bips on bips.ProcedureBusinessItemId=bi.Id
            join ProcedureStep ps on ps.Id=bips.ProcedureStepId
            where bips.ProcedureStepId=@StepId";
    }
}