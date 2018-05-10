using System.Collections.Generic;
using System.Linq;

namespace Procedure.Web.Extensions
{
    public static class IEnumerableExtension
    {
        public static bool IsNullOrEmpty<T> (this IEnumerable<T> ienumerable)
        {
            return (ienumerable == null) || (!ienumerable.Any());
        }
    }
}