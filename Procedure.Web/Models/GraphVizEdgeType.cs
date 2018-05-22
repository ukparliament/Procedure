using System;

namespace Procedure.Web.Models
{
    [Flags]
    public enum GraphVizEdgeType
    {
        CanLeadTo = 0,
        Enables = 1,
    }
}