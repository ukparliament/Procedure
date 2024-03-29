﻿using Procedure.Web.Extensions;
using Procedure.Web.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;
using VDS.RDF;

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
            var g = new Graph();
            g.NamespaceMap.AddNamespace("", new Uri("https://id.parliament.uk/schema/"));
            g.NamespaceMap.AddNamespace("ex", new Uri("https://example.com/"));

            g.LoadFromString(inputString);

            var schema_step = g.CreateUriNode(":ProcedureStep");
            var schema_stepName = g.CreateUriNode(":procedureStepName");
            var schema_workPackageableThingName = g.CreateUriNode(":workPackageableThingName");
            var schema_procedureName = g.CreateUriNode(":procedureName");
            var rdf_type = g.CreateUriNode("rdf:type");
            var ex_possible = g.CreateUriNode("ex:Possible");
            var ex_actualized = g.CreateUriNode("ex:Actualized");
            var ex_canLeadTo = g.CreateUriNode("ex:canLeadTo");
            var ex_ledTo = g.CreateUriNode("ex:ledTo");
            var ex_enables = g.CreateUriNode("ex:enables");

            var builder = new StringBuilder("digraph { graph [fontname = \"calibri\"]; node [fontname = \"calibri\"]; edge [fontname = \"calibri\"];");

            var steps = g.GetTriplesWithPredicateObject(rdf_type, schema_step).Select(t => t.Subject as IUriNode);
            foreach (var step in steps)
            {
                var name = g.GetTriplesWithSubjectPredicate(step, schema_stepName).Select(t => t.Object as ILiteralNode).Single().Value;

                name = string.Join(string.Empty, name.Split(' ').Select((c, i) => c + ((i + 1) % 3 == 0 ? "\\n" : " ")));

                builder.Append($" \"{Reduce(step)}\" [label = \"{name}\"");

                if (g.Triples.Contains(new Triple(step, rdf_type, ex_actualized)))
                {
                    builder.Append(", style = filled, color = gray");
                }

                if (g.Triples.Contains(new Triple(step, rdf_type, ex_possible)))
                {
                    builder.Append(", style = filled, fillcolor = white, color = orange, peripheries = 2");
                }

                builder.Append("];");
            }

            var routes = g.GetTriplesWithPredicate(ex_canLeadTo);
            foreach (var route in routes)
            {
                builder.Append($" \"{Reduce(route.Subject as IUriNode)}\" -> \"{Reduce(route.Object as IUriNode)}\" [label = \"");

                var moreRoutes = g.GetTriplesWithSubjectObject(route.Subject, route.Object);
                if (moreRoutes.WithPredicate(ex_enables).Any())
                {
                    builder.Append("enables\", color = blue");
                }

                if (moreRoutes.WithPredicate(ex_ledTo).Any())
                {
                    builder.Append("led to\"");
                }

                builder.Append("];");
            }

            var workPackageableThing = g.GetTriplesWithPredicate(schema_workPackageableThingName).FirstOrDefault();
            var procedure = g.GetTriplesWithPredicate(schema_procedureName).FirstOrDefault();

            if (workPackageableThing != null && procedure != null)
            {
                builder.Append($"labelloc=\"t\"; fontsize = \"25\"; label = \"{workPackageableThing.Object.ToString()} \\n Subject to: {procedure.Object.ToString()}\"");
            }

            builder.Append("}");

            return builder.ToString();
        }

        private Uri Reduce(IUriNode step)
        {
            return new Uri("https://id.parliament.uk/").MakeRelativeUri(step.Uri);
        }

        [Route("{tripleStoreId}")]
        public ActionResult GraphVizLive(string tripleStoreId)
        {
            return this.GraphViz($"https://api.parliament.uk/query/work_package_by_id?work_package_id={tripleStoreId}");
        }

        [Route("{tripleStoreId}/staging")]
        public ActionResult GraphVizStaging(string tripleStoreId)
        {
            return this.GraphViz($"https://api.parliament.uk/staging/fixed-query/work_package_by_id?work_package_id={tripleStoreId}");
        }

        private ActionResult GraphViz(string url)
        {
            string workPackageResponse = null;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    workPackageResponse = client.GetStringAsync(url).Result;
                }
            }
            catch
            {
                workPackageResponse = string.Empty;
            }

            GraphVizViewModel viewmodel = new GraphVizViewModel();
            if (!workPackageResponse.IsNullOrEmpty())
            {
                viewmodel.DotString = ConvertRDFToDotString(workPackageResponse);
            }
            else
            {
                viewmodel.DotString = string.Empty;
            }
            return View("GraphViz", viewmodel);
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
                else viewmodel.DotString = string.Empty;

                return View(viewmodel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }



    }
}
