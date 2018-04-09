using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Procedure.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public ActionResult Graph(int id)
        {
            WorkPackageDetailViewModel viewModel = new WorkPackageDetailViewModel();

            string workPackageResponse = null;
            using (HttpResponseMessage responseMessage = GetItem(ProcedureWorkPackageListId, id))
            {
                workPackageResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            viewModel.WorkPackage = ((JObject)JsonConvert.DeserializeObject(workPackageResponse)).ToObject<WorkPackageItem>();

            int workPackageId = id;
            int procedureId = viewModel.WorkPackage.SubjectTo.Id;

            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);

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
            List<RouteItem> bothEndsActualizedRoutes = actualizedRouteItems.Where(route => actualizedStepIds.Contains(route.ToStep.Id) & actualizedStepIds.Contains(route.FromStep.Id) & (int)route.RouteKind != 3).ToList();
            int[] blackOutStepIds = bothEndsActualizedRoutes.Select(s => s.FromStep.Id).ToArray();

            // Or use LINQ .Aggregate()
            string toGraph = "graph [fontname = \"calibri\"]; node[fontname = \"calibri\"]; edge[fontname = \"calibri\"];";
            foreach (RouteItem route in actualizedRouteItems)
            {
                string newRoute = "", styling = "";
                if (route.RouteKind.ToString().Equals("Causes")) { newRoute = String.Concat("\"", route.FromStep.Value, "\" -> \"", route.ToStep.Value, "\" [label = \"Causes\"]; "); }
                if (route.RouteKind.ToString().Equals("Allows")) { newRoute = String.Concat("edge [style=dashed]; \"", route.FromStep.Value, "\" -> \"", route.ToStep.Value, "\" [label = \"Allows\"]; edge [style=solid];"); }
                if (route.RouteKind.ToString().Equals("Precludes")) { newRoute = String.Concat("edge [color=red]; \"", route.FromStep.Value, "\" -> \"", route.ToStep.Value, "\" [label = \"Precludes\"]; edge [color=black];"); }
                if (actualizedStepIds.Contains(route.FromStep.Id)) { styling = String.Concat("\"", route.FromStep.Value, "\" [style=filled,color=\"gray\"];"); toGraph += styling; }
                toGraph += newRoute;
            }

            foreach (RouteItem route in lastActualizedRouteItems)
            {
                if (!blackOutStepIds.Contains(route.FromStep.Id) & (int)route.RouteKind!=3) { toGraph += String.Concat("\"", route.ToStep.Value, "\" [style=filled,peripheries=2,color=\"orange\"];"); }
            }

            // Add a legend
            toGraph += "subgraph cluster_key {" +
                "label=\"Key\"; labeljust=\"l\" " +
                "k1[label=\"Actualised step\", style=filled, color=\"gray\"]" +
                "k2[label=\"Possible next step\", style=filled, color=\"orange\", peripheries=2]; node [shape=plaintext];" +
            "k3 [label=<<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" cellborder=\"0\"> " +
            "<tr><td align=\"right\" port=\"i1\" > Allows </td></tr>" +
            "<tr><td align=\"right\" port=\"i2\"> Causes </td></tr>" +
            "<tr><td align=\"right\" port=\"i3\" > Precludes </td></tr> </table>>];" +
            "k3e [label =<<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" cellborder=\"0\">" +
            "<tr><td port=\"i1\" > &nbsp;</td></tr> <tr><td port=\"i2\"> &nbsp;</td></tr> <tr><td port=\"i3\"> &nbsp;</td></tr> </table>>];" +
             "k3:i1:e -> k3e:i1:w [style=dashed] k3:i2:e->k3e:i2:w k3:i3:e->k3e:i3:w[color = red] { rank = same; k3 k3e } {rank = source; k1 k2}};";

            byte[] output = wrapper.GenerateGraph(String.Concat("digraph{", toGraph, "}"), Enums.GraphReturnType.Svg);
            // Alternatively you could save the image on the server as a file.
            string graph = string.Format("data:image/svg+xml;base64,{0}", Convert.ToBase64String(output));
            return File(output, "image/svg+xml");
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