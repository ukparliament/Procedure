namespace Procedure.Web.Models
{
    public class BusinessItem : BaseSharepointTable
    {
        public ReferenceTable BelongsTo { get; set; }
        public ReferenceTable Actualises { get; set; }
    }
}