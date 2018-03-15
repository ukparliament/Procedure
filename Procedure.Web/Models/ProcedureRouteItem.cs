namespace Procedure.Web.Models
{
    public class ProcedureRouteItem
    {
        public int Id { get; set; }
        public int FromStepId { get; set; }
        public string FromStepText { get; set; }
        public int ToStepId { get; set; }
        public string ToStepText { get; set; }
        public int RouteTypeId { get; set; }
        public string RouteTypeText { get; set; }
    }
}