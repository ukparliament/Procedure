namespace Procedure.Web.Models
{
    public class ListRequestObject
    {
        public string ListId { get; set; }
        public int Limit { get; set; }
        public string Filter { get; set; }
    }
}