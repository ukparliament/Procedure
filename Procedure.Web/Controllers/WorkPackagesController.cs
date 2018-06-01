using Procedure.Web.Extensions;
using Procedure.Web.Models;
using System.Collections.Generic;
using System.Linq;
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
            WorkPackageItem workPackage = getWorkPackage(id);
            if (workPackage.Id != 0)
            {
                viewModel.WorkPackage = workPackage;
                viewModel.BusinessItems = getAllBusinessItems(id);
                viewModel.Tree = giveMeTheTree(id, viewModel.WorkPackage.SubjectTo.Id);
            }

            return View(viewModel);
        }

        [Route("{id:int}/graph")]
        public ActionResult GraphViz(int id)
        {
            GraphVizViewModel viewmodel = new GraphVizViewModel();
            viewmodel.DotString = GiveMeDotString(id, showLegend: true);

            return View(viewmodel);
        }

        [Route("{id:int}/graph.dot")]
        public ContentResult GraphDot(int id)
        {
            return Content(GiveMeDotString(id, showLegend: false), "text/plain");
        }

        private string GiveMeDotString(int workPackageId, bool showLegend)
        {
            WorkPackageItem workPackage = getWorkPackage(workPackageId);
            if (workPackage.Id != 0)
            {
                int procedureId = workPackage.SubjectTo.Id;

                List<BusinessItem> businessItemList = getAllBusinessItems(workPackageId);
                int[] actualizedStepIds = businessItemList
                .SelectMany(bi => bi.ActualisesProcedureStep.Select(s => s.Id))
                .ToArray();

                List<RouteItem> routes = getAllRoutes(procedureId);

                List<RouteItem> routesWithActualizedFromSteps = routes.Where(route => actualizedStepIds.Contains(route.FromStep.Id)).ToList();
                List<RouteItem> routesWithActualizedToSteps = routes.Where(route => actualizedStepIds.Contains(route.ToStep.Id)).ToList();

                List<RouteItem> nonSelfReferencedRoutesWithBothEndsActualized = routesWithActualizedFromSteps.Where(route => actualizedStepIds.Contains(route.ToStep.Id) && actualizedStepIds.Contains(route.FromStep.Id) && route.FromStep.Id != route.ToStep.Id).ToList();
                List<RouteItem> precludeOrRequireRoutes = routes.Where(route => route.RouteKind == RouteType.Precludes || route.RouteKind == RouteType.Requires).ToList();
                List<RouteItem> routesWithStepsPrecludingThemselves = routes.Where(r => r.FromStep.Id == r.ToStep.Id && r.RouteKind == RouteType.Precludes).ToList();

                int[] allStepIds = routes.Select(r => r.FromStep.Id).Union(routes.Select(r => r.ToStep.Id)).Distinct().ToArray();
                int[] precludeSelfStepIds = routesWithStepsPrecludingThemselves.Select(r => r.FromStep.Id).ToArray();
                int[] canActualizeSelfAgainStepIds = allStepIds.Except(precludeSelfStepIds).ToArray();

                int[] blackOutFromStepIds = nonSelfReferencedRoutesWithBothEndsActualized.Select(r => r.FromStep.Id).ToArray();
                int[] blackOutToStepsIds = routesWithActualizedFromSteps.Where(r => r.RouteKind == RouteType.Precludes).Select(r => r.ToStep.Id).ToArray();

                IEnumerable<int> unBlackOut = routes.Except(precludeOrRequireRoutes).Except(routesWithActualizedToSteps).GroupBy(r => r.FromStep.Id).Select(group => new { fromStep = group.Key, routeCount = group.Count()}).Where(g => g.routeCount >= 1).Select(g => g.fromStep);

                StringBuilder builder = new StringBuilder("graph [fontname = \"calibri\"]; node[fontname = \"calibri\"]; edge[fontname = \"calibri\"];");

                foreach (RouteItem route in routesWithActualizedFromSteps)
                {
                    if (nonSelfReferencedRoutesWithBothEndsActualized.Contains(route) || (!blackOutToStepsIds.Contains(route.ToStep.Id) && !new[] { RouteType.Precludes, RouteType.Requires }.Contains(route.RouteKind)))
                    {
                        if (route.RouteKind == RouteType.Causes)
                        {
                            builder.Append($"\"{route.FromStep.Value.ProcessName()}\" -> \"{route.ToStep.Value.ProcessName()}\" [label = \"Causes\"]; ");
                        }
                        if (route.RouteKind == RouteType.Allows)
                        {
                            builder.Append($"edge [color=red]; \"{route.FromStep.Value.ProcessName()}\" -> \"{route.ToStep.Value.ProcessName()}\" [label = \"Allows\"]; edge [color=black];");
                        }
                        if (route.RouteKind == RouteType.Precludes)
                        {
                            builder.Append($"edge [color=blue]; \"{route.FromStep.Value.ProcessName()}\" -> \"{route.ToStep.Value.ProcessName()}\" [label = \"Precludes\"]; edge [color=black];");
                        }
                        if (route.RouteKind == RouteType.Requires)
                        {
                            builder.Append($"edge [color=yellow]; \"{route.FromStep.Value.ProcessName()}\" -> \"{route.ToStep.Value.ProcessName()}\" [label = \"Requires\"]; edge [color=black];");
                        }
                    }
                    if (!blackOutFromStepIds.Except(unBlackOut).Contains(route.FromStep.Id) && !blackOutToStepsIds.Contains(route.ToStep.Id) && !new[] { RouteType.Precludes, RouteType.Requires }.Contains(route.RouteKind))
                    {
                        builder.Append($"\"{route.ToStep.Value.ProcessName()}\" [style=filled,fillcolor=white,color=orange,peripheries=2];");
                    }
                    if (actualizedStepIds.Contains(route.FromStep.Id))
                    {
                        builder.Append($"\"{route.FromStep.Value.ProcessName()}\" [style=filled,color=gray];");
                    }
                    if (actualizedStepIds.Contains(route.FromStep.Id) && canActualizeSelfAgainStepIds.Contains(route.FromStep.Id))
                    {
                        builder.Replace($"\"{route.FromStep.Value.ProcessName()}\" [style=filled,color=gray];", $"\"{route.FromStep.Value.ProcessName()}\" [style=filled,color=lemonchiffon2];");
                    }
                    if (actualizedStepIds.Contains(route.ToStep.Id) && canActualizeSelfAgainStepIds.Contains(route.ToStep.Id))
                    {
                        builder.Replace($"\"{route.ToStep.Value.ProcessName()}\" [style=filled,fillcolor=white,color=orange,peripheries=2];", $"\"{route.FromStep.Value.ProcessName()}\" [style=filled,color=lemonchiffon2];");
                    }
                }

                builder.Append($"labelloc=\"t\"; fontsize = \"25\"; label = \"{workPackage.Title} \\n Subject to: {workPackage.SubjectTo.Value}\"");

                if (showLegend == true)
                {
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
                }

                builder.Insert(0, "digraph{");
                builder.Append("}");

                return builder.ToString();
            }
            else
            {
                return "";
            }
        }

        private List<WorkPackageRouteTree> giveMeTheTree(int workPackageId, int procedureId)
        {
            List<WorkPackageRouteTree> result = new List<WorkPackageRouteTree>();

            List<BusinessItem> allBusinessItems = getAllBusinessItems(workPackageId);

            int[] stepsDone = allBusinessItems
                .SelectMany(bi => bi.ActualisesProcedureStep.Select(s => s.Id))
                .ToArray();

            List<ProcedureRouteTree> procedureTree = GenerateProcedureTree(procedureId);
            List<int> precludedSteps = giveMePrecludedSteps(null, stepsDone, procedureTree);

            foreach (ProcedureRouteTree procedureRouteTreeItem in procedureTree)
            {
                List<BusinessItem> businessItems = allBusinessItems
                    .Where(bi => bi.ActualisesProcedureStep.Any(s => s.Id == procedureRouteTreeItem.Step.Id))
                    .ToList();
                if (businessItems.Any())
                {
                    foreach (BusinessItem businessItem in allBusinessItems.Where(bi => businessItems.Exists(b => b.Id == bi.Id)))
                        businessItem.ActualisesProcedureStep.RemoveAll(s => s.Id == procedureRouteTreeItem.Step.Id);
                    allBusinessItems.RemoveAll(bi => bi.ActualisesProcedureStep.Any() == false);
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
                List<BusinessItem> businessItems = allBusinessItems
                    .Where(bi => bi.ActualisesProcedureStep.Any(s => s.Id == procedureRouteTreeItem.Step.Id))
                    .ToList();
                bool isPrecluded = precludedSteps.Contains(procedureRouteTreeItem.Step.Id);
                foreach (BusinessItem businessItem in allBusinessItems.Where(bi => businessItems.Exists(b => b.Id == bi.Id)))
                    businessItem.ActualisesProcedureStep.RemoveAll(s => s.Id == procedureRouteTreeItem.Step.Id);
                allBusinessItems.RemoveAll(bi => bi.ActualisesProcedureStep.Any() == false);
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