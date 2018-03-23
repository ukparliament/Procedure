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

            string businessItemResponse = null;
            using (HttpResponseMessage responseMessage = GetList(ProcedureBusinessItemListId, filter: $"BelongsTo/ID eq {id}"))
            {
                businessItemResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonBusinessItem = (JObject)JsonConvert.DeserializeObject(businessItemResponse);
            List<BusinessItem> allBusinessItems = ((JArray)jsonBusinessItem.SelectToken("value")).ToObject<List<BusinessItem>>();

            viewModel.Tree = new List<WorkPackageRouteTree>();
            List<ProcedureRouteTree> procedureTree = GenerateProcedureTree(viewModel.WorkPackage.SubjectTo.Id);
            foreach (ProcedureRouteTree procedureRouteTreeItem in procedureTree)
            {
                List<BaseSharepointItem> businessItems = allBusinessItems
                    .Where(bi => bi.Actualises.Any(s => s.Id == procedureRouteTreeItem.Step.Id))
                    .Select(bi => new BaseSharepointItem() { Id = bi.Id, Title = bi.Title })
                    .ToList();
                if (businessItems.Any())
                {
                    allBusinessItems.RemoveAll(bi => businessItems.Exists(b => b.Id == bi.Id));
                    viewModel.Tree.Add(new WorkPackageRouteTree()
                    {
                        BusinessItems = businessItems,
                        SelfReferencedRouteKind = procedureRouteTreeItem.SelfReferencedRouteKind,
                        RouteKind = procedureRouteTreeItem.RouteKind,
                        Step = procedureRouteTreeItem.Step,
                        ChildrenRoutes = giveMeFilteredChildren(allBusinessItems, procedureRouteTreeItem.ChildrenRoutes)
                    });
                }
            }

            return View(viewModel);
        }

        public List<WorkPackageRouteTree> giveMeFilteredChildren(List<BusinessItem> allBusinessItems, List<ProcedureRouteTree> procedureTree)
        {
            List<WorkPackageRouteTree> result = new List<WorkPackageRouteTree>();

            foreach (ProcedureRouteTree procedureRouteTreeItem in procedureTree)
            {
                List<BaseSharepointItem> businessItems = allBusinessItems
                    .Where(bi => bi.Actualises.Any(s => s.Id == procedureRouteTreeItem.Step.Id))
                    .Select(bi => new BaseSharepointItem() { Id = bi.Id, Title = bi.Title })
                    .ToList();
                if (businessItems.Any() || (allBusinessItems.Any() == false))
                {
                    allBusinessItems.RemoveAll(bi => businessItems.Exists(b => b.Id == bi.Id));
                    result.Add(new WorkPackageRouteTree()
                    {
                        BusinessItems = businessItems,
                        SelfReferencedRouteKind = procedureRouteTreeItem.SelfReferencedRouteKind,
                        RouteKind = procedureRouteTreeItem.RouteKind,
                        Step = procedureRouteTreeItem.Step,
                        ChildrenRoutes = giveMeFilteredChildren(allBusinessItems, procedureRouteTreeItem.ChildrenRoutes)
                    });
                }
            }
            return result;
        }
    }
}