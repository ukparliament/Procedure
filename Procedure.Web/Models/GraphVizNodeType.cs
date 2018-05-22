using System;

namespace Procedure.Web.Models
{
    [Flags]
    public enum GraphVizNodeType
    {
        Actualized = 0,
        Possible = 1,
        ActualizedAndPossible = 2,
    }
}