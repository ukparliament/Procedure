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
                return RedirectToAction("GraphViz", new { dotString = e.Message}); ;
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

            builder.Insert(0, "digraph{");
            builder.Append("}");

            return RedirectToAction("GraphViz", new { dotString = builder.ToString() });
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

        [Route("generated")]
        public ActionResult GraphViz(string dotString)
        {
            GraphVizViewModel viewmodel = new GraphVizViewModel();
            if (dotString.StartsWith("digraph"))
            {
                viewmodel.DotString = dotString;
            }
            else viewmodel.DotString = "";

            return View(viewmodel);
        }



    }
}
