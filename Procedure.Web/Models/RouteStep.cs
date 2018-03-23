namespace Procedure.Web.Models
{
    public class RouteStep
    {
        public RouteType SelfReferencedRouteKind { get; set; }
        public RouteType RouteKind { get; set; }
        public SharepointLookupItem Step { get; set; }        
    }
}