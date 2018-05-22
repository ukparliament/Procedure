using Procedure.Web.Extensions;
using Procedure.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Procedure.Web.Controllers
{
    [RoutePrefix("WorkPackagesGraphViz")]
    public class WorkPackagesGraphVizController : Controller
    {
        [Route]
        public ActionResult Index()
        {
            return View();
        }

        [ValidateInput(false)]
        [HttpPost, Route]
        public ActionResult Index(string inputString)
        {
            IGraph g = new Graph();
            try
            {
                StringParser.Parse(g, inputString);
            }
            catch (Exception e)
            {
                TempData["StringFromRedirect"] = e.Message;
                return RedirectToAction("GraphViz");
            }

            StringBuilder builder = new StringBuilder("graph [fontname = \"calibri\"]; node[fontname = \"calibri\"]; edge[fontname = \"calibri\"];");

            g.NamespaceMap.AddNamespace("parl", new Uri("https://id.parliament.uk/schema/"));
            g.NamespaceMap.AddNamespace("ex", new Uri("https://example.com/"));

            INode procedureStepName = g.CreateUriNode("parl:procedureStepName");

            IEnumerable<Triple> canLeadToTriples = g.GetTriplesWithPredicate(g.CreateUriNode("ex:CANLEADTO"));
            foreach (Triple t in canLeadToTriples)
            {
                IEnumerable<Triple> fromStepNameTriple = g.GetTriplesWithSubjectPredicate(t.Subject, procedureStepName);
                IEnumerable<Triple> toStepNameTriple = g.GetTriplesWithSubjectPredicate(t.Object, procedureStepName);
                if (!fromStepNameTriple.IsNullOrEmpty() && !toStepNameTriple.IsNullOrEmpty())
                {
                    Triple passToWriter = new Triple(fromStepNameTriple.FirstOrDefault().Object, t.Predicate, toStepNameTriple.FirstOrDefault().Object);
                    passToWriter.WriteToDotString(builder, GraphVizEdgeType.CanLeadTo);
                }
            }

            IEnumerable<Triple> canEnablesTriples = g.GetTriplesWithPredicate(g.CreateUriNode("ex:ENABLES"));
            foreach (Triple t in canEnablesTriples)
            {
                IEnumerable<Triple> fromStepNameTriple = g.GetTriplesWithSubjectPredicate(t.Subject, procedureStepName);
                IEnumerable<Triple> toStepNameTriple = g.GetTriplesWithSubjectPredicate(t.Object, procedureStepName);
                if (!fromStepNameTriple.IsNullOrEmpty() && !toStepNameTriple.IsNullOrEmpty())
                {
                    Triple passToWriter = new Triple(fromStepNameTriple.FirstOrDefault().Object, t.Predicate, toStepNameTriple.FirstOrDefault().Object);
                    passToWriter.WriteToDotString(builder, GraphVizEdgeType.Enables);
                }
            }

            IEnumerable<Triple> actualized = g.GetTriplesWithObject(g.CreateUriNode("ex:ACTUALIZED"));
            foreach (Triple t in actualized)
            {
                IEnumerable<Triple> actualizedTripleNames = g.GetTriplesWithSubjectPredicate(t.Subject, procedureStepName);
                if (!actualizedTripleNames.IsNullOrEmpty())
                {
                    actualizedTripleNames.ToList().ForEach(triple => triple.WriteToDotString(builder, GraphVizNodeType.Actualized));
                }
            }

            IEnumerable<Triple> possible = g.GetTriplesWithObject(g.CreateUriNode("ex:POSSIBLE"));
            foreach (Triple t in possible)
            {
                IEnumerable<Triple> possibleTripleNames = g.GetTriplesWithSubjectPredicate(t.Subject, procedureStepName);
                if (!possibleTripleNames.IsNullOrEmpty())
                {
                    possibleTripleNames.ToList().ForEach(triple => triple.WriteToDotString(builder, GraphVizNodeType.Possible));
                }
            }

            IEnumerable<Triple> distance = g.GetTriplesWithPredicate(g.CreateUriNode("ex:DISTANCE"));
            List<Triple> distWithStepNames = new List<Triple>();
            foreach (Triple t in distance)
            {
                IEnumerable<Triple> names = g.GetTriplesWithSubjectPredicate(t.Subject, procedureStepName);
                if(!names.IsNullOrEmpty())
                {
                    distWithStepNames.Add(new Triple(names.FirstOrDefault().Object, t.Predicate, t.Object));
                }
            }

            //Dictionary<string, List<Triple>> rankDict = distWithStepNames.GroupBy(t => t.Object.ToString()).ToDictionary(group => group.Key, group => group.ToList());
            //foreach (KeyValuePair<string, List<Triple>> entry in rankDict)
            //{
            //    builder.Append($"{{ rank=same; {String.Join(" ", entry.Value.Select(t => t.Subject.ToString().SurroundWithDoubleQuotes()))} }}");
            //}

            builder.Append("subgraph cluster_key {" +
                    "label=\"Key\"; labeljust=\"l\" " +
                    "k1[label=\"Actualised step\", style=filled, color=gray]" +
                    "k2[label=\"Actualised step that can be actualised again\", style=filled, color=lemonchiffon2]" +
                    "k3[label=\"Possible next step yet to be actualised\" style=filled,fillcolor=white, color=orange, peripheries=2]; node [shape=plaintext];" +
                    "ktable [label=<<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" cellborder=\"0\"> " +
                    "<tr><td align=\"right\" port=\"i1\" > Can lead to </td></tr>" +
                    "<tr><td align=\"right\" port=\"i2\"> Enables </td></tr> </table>>];" +
                    "ktabledest [label =<<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" cellborder=\"0\">" +
                    "<tr><td port=\"i1\" > &nbsp;</td></tr> <tr><td port=\"i2\"> &nbsp;</td></tr> </table>>];" +
                    "ktable:i1:e->ktabledest:i1:w ktable:i2:e->ktabledest:i2:w [color=blue] {rank = sink; k1 k2 k3}  { rank = same; ktable ktabledest } };");

            builder.Insert(0, "digraph{");
            builder.Append("}");

            TempData["StringFromRedirect"] = builder.ToString();

            return RedirectToAction("GraphViz");
        }

        private SparqlResultSet getNameFromGraph(SparqlQueryParser parser, INode stepNode, string predicateForName, IGraph g)
        {
            SparqlQuery getStepName = parser.ParseFromString($"PREFIX : <https://id.parliament.uk/schema/> SELECT ?stepName WHERE {{<{stepNode.ToString()}> :{predicateForName} ?stepName. }}");
            return (SparqlResultSet)g.ExecuteQuery(getStepName);
        }

        private SparqlResultSet checkIfActualized(SparqlQueryParser parser, INode stepNode, IGraph g)
        {
            SparqlQuery check = parser.ParseFromString($"PREFIX : <https://id.parliament.uk/schema/> ASK {{?bi :businessItemHasProcedureStep <{stepNode.ToString()}>.}}");
            return (SparqlResultSet)g.ExecuteQuery(check);
        }

        [Route("generate")]
        public ActionResult GraphViz()
        {
            GraphVizViewModel viewmodel = new GraphVizViewModel();
            if (TempData["StringFromRedirect"] != null)
            {
                string q = TempData["StringFromRedirect"].ToString();
                if (q.StartsWith("digraph"))
                {
                    viewmodel.DotString = q;
                }
                else viewmodel.DotString = "";

                return View(viewmodel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }



    }
}
