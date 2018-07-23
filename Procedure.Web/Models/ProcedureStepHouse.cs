namespace Procedure.Web.Models
{
    public class ProcedureStepHouse
    {
        public int Id { get; set; }
        public int ProcedureStepId { get; set; }
        public string HouseName { get; set; }

        public static string ListSql = @"select sh.ProcedureStepId, h.Id, h.HouseName from ProcedureStepHouse sh
            join House h on h.Id=sh.HouseId";

        public static string ListByStepSql = @"select sh.ProcedureStepId, h.Id, h.HouseName from ProcedureStepHouse sh
            join House h on h.Id=sh.HouseId
            where sh.ProcedureStepId=@StepId";
    }
}