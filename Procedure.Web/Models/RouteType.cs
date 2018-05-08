using System;

namespace Procedure.Web.Models
{
    [Flags]
    public enum RouteType
    {
        None = 0,
        Causes = 1,
        Allows = 2,
        Precludes = 3,
        Requires = 4
    }
}