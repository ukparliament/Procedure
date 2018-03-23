using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Procedure.Web.Models;
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