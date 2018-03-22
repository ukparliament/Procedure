namespace Procedure.Web.Models
{
    public class SharepointLookupItem
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public T ToSharepointItem<T>() where T : BaseSharepointItem, new()
        {
            return new T()
            {
                Id = Id,
                Title = Value
            };
        }
    }
}