using Procedure.Web.Extensions;
using Procedure.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
            string dotString = ConvertRDFToDotString(inputString);
            TempData["StringFromRedirect"] = dotString;

            return RedirectToAction("GraphViz");
        }

        public string ConvertRDFToDotString(string inputString)
        {
            IGraph g = new Graph();
            try
            {
                StringParser.Parse(g, inputString);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            StringBuilder builder = new StringBuilder("graph [fontname = \"calibri\"]; node[fontname = \"calibri\"]; edge[fontname = \"calibri\"];");

            g.NamespaceMap.AddNamespace("parl", new Uri("https://id.parliament.uk/schema/"));
            g.NamespaceMap.AddNamespace("ex", new Uri("https://example.com/"));

            INode procedureStepName = g.CreateUriNode("parl:procedureStepName");

            IEnumerable<Triple> canLeadToTriples = g.GetTriplesWithPredicate(g.CreateUriNode("ex:canLeadTo"));
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

            IEnumerable<Triple> canEnablesTriples = g.GetTriplesWithPredicate(g.CreateUriNode("ex:enables"));
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

            IEnumerable<Triple> actualized = g.GetTriplesWithObject(g.CreateUriNode("ex:Actualized"));
            foreach (Triple t in actualized)
            {
                IEnumerable<Triple> actualizedTripleNames = g.GetTriplesWithSubjectPredicate(t.Subject, procedureStepName);
                if (!actualizedTripleNames.IsNullOrEmpty())
                {
                    actualizedTripleNames.ToList().ForEach(triple => triple.WriteToDotString(builder, GraphVizNodeType.Actualized));
                }
            }

            IEnumerable<Triple> possible = g.GetTriplesWithObject(g.CreateUriNode("ex:Possible"));
            foreach (Triple t in possible)
            {
                IEnumerable<Triple> possibleTripleNames = g.GetTriplesWithSubjectPredicate(t.Subject, procedureStepName);
                if (!possibleTripleNames.IsNullOrEmpty())
                {
                    possibleTripleNames.ToList().ForEach(triple => triple.WriteToDotString(builder, GraphVizNodeType.Possible));
                }
            }

            IEnumerable<Triple> distance = g.GetTriplesWithPredicate(g.CreateUriNode("ex:distance"));
            List<Triple> distWithStepNames = new List<Triple>();
            foreach (Triple t in distance)
            {
                IEnumerable<Triple> names = g.GetTriplesWithSubjectPredicate(t.Subject, procedureStepName);
                if (!names.IsNullOrEmpty())
                {
                    distWithStepNames.Add(new Triple(names.FirstOrDefault().Object, t.Predicate, t.Object));
                }
            }

            //Dictionary<string, List<Triple>> rankDict = distWithStepNames.GroupBy(t => t.Object.ToString()).ToDictionary(group => group.Key, group => group.ToList());
            //foreach (KeyValuePair<string, List<Triple>> entry in rankDict)
            //{
            //    builder.Append($"{{ rank=same; {String.Join(" ", entry.Value.Select(t => t.Subject.ToString().SurroundWithDoubleQuotes()))} }}");
            //}

            builder.Insert(0, "digraph{");
            builder.Append("}");

            return builder.ToString();

        }

        [Route("{tripleStoreId}")]
        public ActionResult GraphViz(string tripleStoreId)
        {
            string workPackageResponse = null;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    workPackageResponse = client.GetStringAsync($"https://api.parliament.uk/staging/fixed-query/work_package_by_id?work_package_id=https://id.parliament.uk/{tripleStoreId}").Result;
                }
            }
            catch
            {
                workPackageResponse = "";
            }

            GraphVizViewModel viewmodel = new GraphVizViewModel();
            if (!workPackageResponse.IsNullOrEmpty())
            {
                viewmodel.DotString = ConvertRDFToDotString(workPackageResponse);
            }
            else
            {
                viewmodel.DotString = "";
            }
            return View(viewmodel);
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
