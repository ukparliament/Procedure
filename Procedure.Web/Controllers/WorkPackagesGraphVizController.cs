using Procedure.Web.Models;
using System;
using System.Collections.Generic;
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
            // string passAlong = "digraph {a -> b}";
            StringBuilder builder = new StringBuilder("graph [fontname = \"calibri\"]; node[fontname = \"calibri\"]; edge[fontname = \"calibri\"];");

            IEnumerable<Triple> allowsTriples = g.GetTriplesWithPredicate(new System.Uri("http://example.com/allows"));
            IEnumerable<Triple> causesTriples = g.GetTriplesWithPredicate(new System.Uri("http://example.com/causes"));

            SparqlQueryParser parser = new SparqlQueryParser();

            foreach (Triple t in causesTriples)
            {
                SparqlResultSet from = getNameFromGraph(parser, t.Subject, "procedureStepName", g);
                string fromStep = from[0]["stepName"].ToString();
                SparqlResultSet to = getNameFromGraph(parser, t.Object, "procedureStepName", g);
                string toStep = to[0]["stepName"].ToString();
                builder.Append($"\"{fromStep}\" -> \"{toStep}\" [label = \"Causes\"];");
            }

            foreach (Triple t in allowsTriples)
            {
                SparqlResultSet from = getNameFromGraph(parser, t.Subject, "procedureStepName", g);
                string fromStep = from[0]["stepName"].ToString();
                SparqlResultSet to = getNameFromGraph(parser, t.Object, "procedureStepName", g);
                string toStep = to[0]["stepName"].ToString();
                builder.Append($"edge [color=red]; \"{fromStep}\" -> \"{toStep}\" [label = \"Allows\"]; edge [color=black];");
            }

            IEnumerable<Triple> allSteps = g.GetTriplesWithObject(new System.Uri("https://id.parliament.uk/schema/ProcedureStep"));
            foreach (Triple t in allSteps)
            {
                SparqlResultSet actualized = checkIfActualized(parser, t.Subject, g);
                if (actualized.Result)
                {
                    SparqlResultSet step = getNameFromGraph(parser, t.Subject, "procedureStepName", g);
                    string stepName = step[0]["stepName"].ToString();
                    builder.Append($"\"{stepName}\" [style=filled,color=gray];");
                }
            }

            builder.Append("subgraph cluster_key {" +
                    "label=\"Key\"; labeljust=\"l\" " +
                    "k1[label=\"Actualised step\", style=filled, color=gray]" +
                    "k2[label=\"Actualised step that can be actualised again\", style=filled, color=lemonchiffon2]" +
                    "k3[label=\"Possible next step yet to be actualised\" style=filled,fillcolor=white, color=orange, peripheries=2]; node [shape=plaintext];" +
                    "ktable [label=<<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" cellborder=\"0\"> " +
                    "<tr><td align=\"right\" port=\"i1\" > Causes </td></tr>" +
                    "<tr><td align=\"right\" port=\"i2\"> Allows </td></tr>" +
                    "<tr><td align=\"right\" port=\"i3\" > Precludes </td></tr>" +
                    "<tr><td align=\"right\" port=\"i4\" > Requires </td></tr> </table>>];" +
                    "ktabledest [label =<<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" cellborder=\"0\">" +
                    "<tr><td port=\"i1\" > &nbsp;</td></tr> <tr><td port=\"i2\"> &nbsp;</td></tr> <tr><td port=\"i3\"> &nbsp;</td></tr> <tr><td port=\"i4\"> &nbsp;</td></tr> </table>>];" +
                    "ktable:i1:e->ktabledest:i1:w ktable:i2:e->ktabledest:i2:w [color=red] ktable:i3:e->ktabledest:i3:w [color = blue] ktable:i4:e->ktabledest:i4:w [color = yellow] {rank = sink; k1 k2 k3}  { rank = same; ktable ktabledest } };");

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

            string q = TempData["StringFromRedirect"].ToString();

            if (q.StartsWith("digraph"))
            {
                viewmodel.DotString = q;
            }
            else viewmodel.DotString = "";

            return View(viewmodel);
        }



    }
}
