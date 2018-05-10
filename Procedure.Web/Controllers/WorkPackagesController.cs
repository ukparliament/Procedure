using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Procedure.Web.Extensions;
using Procedure.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;

namespace Procedure.Web.Controllers
{
    [RoutePrefix("WorkPackages")]
    public class WorkPackagesController : BaseController
    {
        [Route]
        public ActionResult Index()
        {
            return ShowList<WorkPackageItem>(ProcedureWorkPackageListId);
        }

        [Route("{id:int}")]
        public ActionResult Details(int id)
        {
            WorkPackageDetailViewModel viewModel = new WorkPackageDetailViewModel();

            string workPackageResponse = null;
            using (HttpResponseMessage responseMessage = GetItem(ProcedureWorkPackageListId, id))
            {
                workPackageResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            viewModel.WorkPackage = ((JObject)JsonConvert.DeserializeObject(workPackageResponse)).ToObject<WorkPackageItem>();

            viewModel.Tree = giveMeTheTree(id, viewModel.WorkPackage.SubjectTo.Id);

            return View(viewModel);
        }

        [Route("{id:int}/graph")]
        public ActionResult GraphViz(int id)
        {
            GraphVizViewModel viewmodel = new GraphVizViewModel();
            viewmodel.DotString = GiveMeDotString(id);

            return View(viewmodel);
        }

        [Route("{id:int}/graph.dot")]
        public ActionResult GraphDot(int id)
        {
            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);

            byte[] output = wrapper.GenerateGraph(GiveMeDotString(id), Enums.GraphReturnType.Plain);
            return File(output, "text/plain");
        }

        private string GiveMeDotString(int workPackageId)
        {
            WorkPackageDetailViewModel viewModel = new WorkPackageDetailViewModel();

            string workPackageResponse = null;
            using (HttpResponseMessage responseMessage = GetItem(ProcedureWorkPackageListId, workPackageId))
            {
                workPackageResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            viewModel.WorkPackage = ((JObject)JsonConvert.DeserializeObject(workPackageResponse)).ToObject<WorkPackageItem>();
            int procedureId = viewModel.WorkPackage.SubjectTo.Id;

            // Get actualized steps (this needs workPackage ID param)
            string businessItemResponse = null;
            using (HttpResponseMessage responseMessage = GetList(ProcedureBusinessItemListId, filter: $"BelongsTo/ID eq {workPackageId}"))
            {
                businessItemResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonBusinessItem = (JObject)JsonConvert.DeserializeObject(businessItemResponse);

            List<BusinessItem> businessItemList = jsonBusinessItem.SelectToken("value").ToObject<List<BusinessItem>>();
            int[] actualizedStepIds = businessItemList
                .SelectMany(bi => bi.Actualises.Select(s => s.Id))
                .ToArray();

            // Get all their possible next steps (this needs procedure ID param)
            string routeResponse = null;
            using (HttpResponseMessage responseMessage = GetList(ProcedureRouteListId, filter: $"Procedure/ID eq {procedureId}"))
            {
                routeResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonRoute = (JObject)JsonConvert.DeserializeObject(routeResponse);
            List<RouteItem> routes = ((JArray)jsonRoute.SelectToken("value")).ToObject<List<RouteItem>>();
            List<RouteItem> actualizedRouteItems = routes.Where(route => actualizedStepIds.Contains(route.FromStep.Id)).ToList();
            List<RouteItem> lastActualizedRouteItems = actualizedRouteItems.Except(actualizedRouteItems.Where(route => actualizedStepIds.Contains(route.ToStep.Id) & actualizedStepIds.Contains(route.FromStep.Id))).ToList();
            List<RouteItem> bothEndsActualizedRoutes = actualizedRouteItems.Where(route => actualizedStepIds.Contains(route.ToStep.Id) & actualizedStepIds.Contains(route.FromStep.Id) & route.RouteKind != RouteType.Precludes).ToList();
            int[] blackOutStepIds = bothEndsActualizedRoutes.Select(s => s.FromStep.Id).ToArray();

            // Or use LINQ .Aggregate()
            StringBuilder builder = new StringBuilder("graph [fontname = \"calibri\"]; node[fontname = \"calibri\"]; edge[fontname = \"calibri\"];");

            foreach (RouteItem route in actualizedRouteItems)
            {
                if (route.RouteKind == RouteType.Causes) {
                    builder.Append($"\"{route.FromStep.Value.RemoveQuotesAndTrim()}\" -> \"{route.ToStep.Value.RemoveQuotesAndTrim()}\" [label = \"Causes\"]; ");
                }
                if (route.RouteKind == RouteType.Allows) {
                    builder.Append($"edge [color=red]; \"{route.FromStep.Value.RemoveQuotesAndTrim()}\" -> \"{route.ToStep.Value.RemoveQuotesAndTrim()}\" [label = \"Allows\"]; edge [color=black];");
                }
                if (route.RouteKind == RouteType.Precludes) {
                    builder.Append($"edge [color=blue]; \"{route.FromStep.Value.RemoveQuotesAndTrim()}\" -> \"{route.ToStep.Value.RemoveQuotesAndTrim()}\" [label = \"Precludes\"]; edge [color=black];");
                }
                if (route.RouteKind == RouteType.Requires)
                {
                    builder.Append($"edge [color=yellow]; \"{route.FromStep.Value.RemoveQuotesAndTrim()}\" -> \"{route.ToStep.Value.RemoveQuotesAndTrim()}\" [label = \"Requires\"]; edge [color=black];");
                }
                if (actualizedStepIds.Contains(route.FromStep.Id)) {
                    builder.Append($"\"{route.FromStep.Value.RemoveQuotesAndTrim()}\" [style=filled,color=\"gray\"];");
                }
            }

            foreach (RouteItem route in lastActualizedRouteItems)
            {
                if (!blackOutStepIds.Contains(route.FromStep.Id) & !new[]{RouteType.Precludes, RouteType.Requires}.Contains(route.RouteKind) ) {
                    builder.Append($"\"{route.ToStep.Value.RemoveQuotesAndTrim()}\" [style=filled,peripheries=2,color=\"orange\"];");
                }
            }

            // Add a legend
            builder.Append("subgraph cluster_key {" +
                "label=\"Key\"; labeljust=\"l\" " +
                "k1[label=\"Actualised step\", style=filled, color=\"gray\"]" +
                "k2[label=\"Possible next step\", style=filled, color=\"orange\", peripheries=2]; node [shape=plaintext];" +
            "k3 [label=<<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" cellborder=\"0\"> " +
            "<tr><td align=\"right\" port=\"i1\" > Causes </td></tr>" +
            "<tr><td align=\"right\" port=\"i2\"> Allows </td></tr>" +
            "<tr><td align=\"right\" port=\"i3\" > Precludes </td></tr>" +
            "<tr><td align=\"right\" port=\"i4\" > Requires </td></tr> </table>>];" +
            "k3e [label =<<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" cellborder=\"0\">" +
            "<tr><td port=\"i1\" > &nbsp;</td></tr> <tr><td port=\"i2\"> &nbsp;</td></tr> <tr><td port=\"i3\"> &nbsp;</td></tr> <tr><td port=\"i4\"> &nbsp;</td></tr> </table>>];" +
             "k3:i1:e->k3e:i1:w k3:i2:e->k3e:i2:w [color=red] k3:i3:e->k3e:i3:w [color = blue] k3:i4:e->k3e:i4:w [color = yellow] { rank = same; k3 k3e } {rank = source; k1 k2}};");

            builder.Insert(0, "digraph{");
            builder.Append("}");

            return builder.ToString();
        }

        private List<WorkPackageRouteTree> giveMeTheTree(int workPackageId, int procedureId)
        {
            List<WorkPackageRouteTree> result = new List<WorkPackageRouteTree>();

            string businessItemResponse = null;
            using (HttpResponseMessage responseMessage = GetList(ProcedureBusinessItemListId, filter: $"BelongsTo/ID eq {workPackageId}"))
            {
                businessItemResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonBusinessItem = (JObject)JsonConvert.DeserializeObject(businessItemResponse);
            List<BusinessItem> allBusinessItems = ((JArray)jsonBusinessItem.SelectToken("value")).ToObject<List<BusinessItem>>();
            int[] stepsDone = allBusinessItems
                .SelectMany(bi => bi.Actualises.Select(s => s.Id))
                .ToArray();

            List<ProcedureRouteTree> procedureTree = GenerateProcedureTree(procedureId);
            List<int> precludedSteps = giveMePrecludedSteps(null, stepsDone, procedureTree);

            foreach (ProcedureRouteTree procedureRouteTreeItem in procedureTree)
            {
                List<BaseSharepointItem> businessItems = allBusinessItems
                    .Where(bi => bi.Actualises.Any(s => s.Id == procedureRouteTreeItem.Step.Id))
                    .Select(bi => new BaseSharepointItem() { Id = bi.Id, Title = bi.Title })
                    .ToList();
                if (businessItems.Any())
                {
                    foreach (BusinessItem businessItem in allBusinessItems.Where(bi => businessItems.Exists(b => b.Id == bi.Id)))
                        businessItem.Actualises.RemoveAll(s => s.Id == procedureRouteTreeItem.Step.Id);
                    allBusinessItems.RemoveAll(bi => bi.Actualises.Any() == false);
                    result.Add(new WorkPackageRouteTree()
                    {
                        BusinessItems = businessItems,
                        IsPrecluded = precludedSteps.Contains(procedureRouteTreeItem.Step.Id),
                        SelfReferencedRouteKind = procedureRouteTreeItem.SelfReferencedRouteKind,
                        RouteKind = procedureRouteTreeItem.RouteKind,
                        Step = procedureRouteTreeItem.Step,
                        ChildrenRoutes = giveMeFilteredChildren(allBusinessItems, procedureRouteTreeItem.ChildrenRoutes, precludedSteps)
                    });
                }
            }

            return result;
        }

        private List<WorkPackageRouteTree> giveMeFilteredChildren(List<BusinessItem> allBusinessItems, List<ProcedureRouteTree> procedureTree, List<int> precludedSteps)
        {
            List<WorkPackageRouteTree> result = new List<WorkPackageRouteTree>();

            foreach (ProcedureRouteTree procedureRouteTreeItem in procedureTree)
            {
                List<BaseSharepointItem> businessItems = allBusinessItems
                    .Where(bi => bi.Actualises.Any(s => s.Id == procedureRouteTreeItem.Step.Id))
                    .Select(bi => new BaseSharepointItem() { Id = bi.Id, Title = bi.Title })
                    .ToList();
                bool isPrecluded = precludedSteps.Contains(procedureRouteTreeItem.Step.Id);
                foreach (BusinessItem businessItem in allBusinessItems.Where(bi => businessItems.Exists(b => b.Id == bi.Id)))
                    businessItem.Actualises.RemoveAll(s => s.Id == procedureRouteTreeItem.Step.Id);
                allBusinessItems.RemoveAll(bi => bi.Actualises.Any() == false);
                result.Add(new WorkPackageRouteTree()
                {
                    BusinessItems = businessItems,
                    IsPrecluded = isPrecluded,
                    SelfReferencedRouteKind = procedureRouteTreeItem.SelfReferencedRouteKind,
                    RouteKind = procedureRouteTreeItem.RouteKind,
                    Step = procedureRouteTreeItem.Step,
                    ChildrenRoutes = isPrecluded ? new List<WorkPackageRouteTree>() : giveMeFilteredChildren(allBusinessItems, procedureRouteTreeItem.ChildrenRoutes, precludedSteps)
                });
            }
            return result;
        }

        private List<int> giveMePrecludedSteps(int? parentStepId, int[] stepsDone, List<ProcedureRouteTree> procedureTree)
        {
            List<int> result = new List<int>();
            foreach (ProcedureRouteTree procedureRouteTreeItem in procedureTree)
            {
                if ((procedureRouteTreeItem.RouteKind == RouteType.Precludes) &&
                    (parentStepId.HasValue) && (stepsDone.Contains(parentStepId.Value)))
                    result.Add(procedureRouteTreeItem.Step.Id);
                if (stepsDone.Contains(procedureRouteTreeItem.Step.Id))
                    result.AddRange(giveMePrecludedSteps(procedureRouteTreeItem.Step.Id, stepsDone, procedureRouteTreeItem.ChildrenRoutes));
            }

            return result;
        }

    }
}