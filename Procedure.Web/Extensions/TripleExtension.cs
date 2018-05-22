using Procedure.Web.Models;
using System.Text;
using VDS.RDF;

namespace Procedure.Web.Extensions
{
    public static class TripleExtension
    {
        public static void WriteToDotString(this Triple triple, StringBuilder stringBuilder, GraphVizNodeType nodeType)
        {
            if(nodeType == GraphVizNodeType.Possible)
            {
                stringBuilder.Append($"\"{triple.Object.ToString()}\" [style=filled,fillcolor=white,color=orange,peripheries=2];");
            }
            if(nodeType == GraphVizNodeType.Actualized)
            {
                stringBuilder.Append($"\"{triple.Object.ToString()}\" [style=filled,color=gray];");
            }

        }

        public static void WriteToDotString(this Triple triple, StringBuilder stringBuilder, GraphVizEdgeType edgeType)
        {
            if (edgeType == GraphVizEdgeType.CanLeadTo)
            {
                stringBuilder.Append($"\"{triple.Subject.ToString()}\" -> \"{triple.Object.ToString()}\" [label = \"Led to\"];"); 
            }
            if (edgeType == GraphVizEdgeType.Enables)
            {
                stringBuilder.Replace($"\"{triple.Subject.ToString()}\" -> \"{triple.Object.ToString()}\" [label = \"Led to\"];", $"edge [color=blue];\"{triple.Subject.ToString()}\" -> \"{triple.Object.ToString()}\" [label = \"Enables\"]; edge [color=black];");
            }

        }
    }
}