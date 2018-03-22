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
            WorkPackagePathwayViewModel viewModel = new WorkPackagePathwayViewModel();

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
            List<BusinessItem> businessItems = ((JArray)jsonBusinessItem.SelectToken("value")).ToObject<List<BusinessItem>>();

            string routeResponse = null;
            using (HttpResponseMessage responseMessage = GetList(ProcedureRouteListId, filter: $"Procedure/ID eq {viewModel.WorkPackage.SubjectTo.Id}"))
            {
                routeResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonRoute = (JObject)JsonConvert.DeserializeObject(routeResponse);
            List<RouteItem> routes = ((JArray)jsonRoute.SelectToken("value")).ToObject<List<RouteItem>>();

            viewModel.Routes = giveMePathway(businessItems, routes);

            return View(viewModel);
        }

        public List<WorkPackageRoute> giveMePathway(List<BusinessItem> businessItems, List<RouteItem> routes)
        {
            RouteItem[] filteredRouteItems = routes.Where(to => to.FromStep.Id != to.ToStep.Id).ToArray();
            int[] entrySteps = filteredRouteItems
                .Where(r => filteredRouteItems.Any(to => to.ToStep.Id == r.FromStep.Id) == false)
                .Select(r => r.FromStep.Id)
                .Distinct()
                .ToArray();
            List<int> stepsDone = businessItems
                .SelectMany(bi => bi.Actualises.Select(s => s.Id))
                .ToList();
            List<WorkPackageRoute> result = new List<WorkPackageRoute>();
            stepsDone.Sort();
            while (stepsDone.Any())
            {
                int stepId = stepsDone.FirstOrDefault();
                RouteItem route = filteredRouteItems.FirstOrDefault(r => r.FromStep.Id == stepId);
                WorkPackageRoute workPackageRoute = new WorkPackageRoute()
                {
                    RouteKind = RouteType.None,
                    FollowingRoutes = new List<WorkPackageRoute>(),
                    Step = route.FromStep.ToSharepointItem<StepItem>(),
                    BusinessItems = businessItems
                    .Where(bi => bi.Actualises.Any(s => s.Id == stepId))
                    .Select(bi => new BaseSharepointItem() { Id = bi.Id, Title = bi.Title })
                    .ToList()
                };
                result.Add(workPackageRoute);
                stepsDone.Remove(stepId);
            }
            WorkPackageRoute lastDone = result.LastOrDefault();
            lastDone.FollowingRoutes = giveMeRoutes(lastDone.Step.Id, businessItems, routes);

            return result;
        }

        public List<WorkPackageRoute> giveMeRoutes(int stepId, List<BusinessItem> businessItems, List<RouteItem> routes)
        {
            List<WorkPackageRoute> result = new List<WorkPackageRoute>();
            IEnumerable<RouteItem> children = routes
                .Where(r => r.FromStep.Id == stepId && r.FromStep.Id != r.ToStep.Id);
            foreach (RouteItem child in children)
            {
                result.Add(new WorkPackageRoute()
                {
                    RouteKind = child.RouteKind.ToSharepointItem<RouteTypeItem>().RouteKind,
                    Step = child.ToStep.ToSharepointItem<StepItem>(),
                    FollowingRoutes = giveMeRoutes(child.ToStep.Id, businessItems, routes),
                    BusinessItems = businessItems
                    .Where(bi => bi.Actualises.Any(s => s.Id == child.ToStep.Id))
                    .Select(bi => new BaseSharepointItem() { Id = bi.Id, Title = bi.Title })
                    .ToList()
                });
            }

            return result;
        }
    }
}