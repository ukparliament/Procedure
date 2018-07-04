using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Procedure.Web.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;

namespace Procedure.Web.Controllers
{
    public class BaseController : Controller
    {
        internal string ProcedureListId = ConfigurationManager.AppSettings["ProcedureListId"];
        internal string ProcedureStepListId = ConfigurationManager.AppSettings["ProcedureStepListId"];
        internal string ProcedureRouteTypeListId = ConfigurationManager.AppSettings["ProcedureRouteTypeListId"];
        internal string ProcedureRouteListId = ConfigurationManager.AppSettings["ProcedureRouteListId"];
        internal string ProcedureWorkPackageListId = ConfigurationManager.AppSettings["ProcedureWorkPackageListId"];
        internal string ProcedureBusinessItemListId = ConfigurationManager.AppSettings["ProcedureBusinessItemListId"];

        private string listUri = ConfigurationManager.AppSettings["GetListUri"];
        private string itemUri = ConfigurationManager.AppSettings["GetItemUri"];

        internal T GetSqlItem<T>(string sql) where T : new()
        {
            T result = new T();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["AzureSQL"]))
            {
                result = connection.Query<T>(sql).SingleOrDefault();
            }
            return result;
        }

        internal IEnumerable<T> GetSqlList<T>(string sql)
        {
            IEnumerable<T> result = null;
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["AzureSQL"]))
            {
                result = connection.Query<T>(sql);
            }
            return result;
        }

        internal HttpResponseMessage GetSharepointList(string listId, int limit = 1000, string filter = null)
        {
            HttpClient client = new HttpClient();

            return client.PostAsJsonAsync<ListRequestObject>(listUri, new ListRequestObject()
            {
                ListId = listId,
                Limit = limit,
                Filter = filter
            }).Result;
        }

        internal HttpResponseMessage GetSharepointItem(string listId, int id)
        {
            string uri = itemUri.Replace("{listId}", listId)
                .Replace("{id}", id.ToString());
            HttpClient client = new HttpClient();

            return client.GetAsync(uri).Result;
        }

        internal ActionResult ShowList<T>(string listId)
        {
            List<T> items = new List<T>();
            string response = null;
            using (HttpResponseMessage responseMessage = GetSharepointList(listId))
            {
                response = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);
            items = ((JArray)jsonResponse.SelectToken("value")).ToObject<List<T>>();

            return View(items);
        }

        internal List<ProcedureRouteTree> GenerateProcedureTree(int procedureId)
        {
            List<ProcedureRouteTree> result = new List<ProcedureRouteTree>();

            string routeResponse = null;

            // Sharepoint
            using (HttpResponseMessage responseMessage = GetSharepointList(ProcedureRouteListId, filter: $"Procedure/ID eq {procedureId}"))
            {
                routeResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonRoute = (JObject)JsonConvert.DeserializeObject(routeResponse);
            List<RouteItem> routes = ((JArray)jsonRoute.SelectToken("value")).ToObject<List<RouteItem>>();
            
            // Azure SQL 
            // List<RouteItem> routesfromSQL = GetSqlList<RouteItem>($"select * from ProcedureRoute where ProcedureId = {procedureId}").ToList();
            
            RouteItem[] filteredRouteItems = routes.Where(to => to.FromStep.Id != to.ToStep.Id).ToArray();
            int[] entrySteps = filteredRouteItems
                .Where(r => filteredRouteItems.Any(to => to.ToStep.Id == r.FromStep.Id) == false)
                .Select(r => r.FromStep.Id)
                .Distinct()
                .ToArray();
            RouteItem[] entryRoutes = entrySteps
                .Select(s => routes.FirstOrDefault(r => r.FromStep.Id == s))
                .ToArray();
            List<ProcedureRouteTree> tree = new List<ProcedureRouteTree>();
            List<int> parents = new List<int>();
            foreach (RouteItem route in entryRoutes)
            {
                RouteItem selfReferenced = routes
                    .FirstOrDefault(r => r.FromStep.Id == route.ToStep.Id && r.FromStep.Id == r.ToStep.Id);
                tree.Add(new ProcedureRouteTree()
                {
                    SelfReferencedRouteKind = selfReferenced?.RouteKind ?? RouteType.None,
                    RouteKind = RouteType.None,
                    Step = route.FromStep,
                    ChildrenRoutes = giveMeChildrenRoutes(route.FromStep.Id, routes, ref parents)
                });
            }

            return tree;
        }

        private List<ProcedureRouteTree> giveMeChildrenRoutes(int parentStepId, List<RouteItem> allRoutes, ref List<int> parents)
        {
            List<ProcedureRouteTree> result = new List<ProcedureRouteTree>();
            parents.Add(parentStepId);
            RouteItem[] children = allRoutes.Where(r => r.FromStep.Id == parentStepId).ToArray();
            foreach (RouteItem route in children)
            {
                if (route.ToStep.Id != parentStepId)
                {
                    RouteItem selfReferenced = allRoutes
                        .FirstOrDefault(r => r.FromStep.Id == route.ToStep.Id && r.FromStep.Id == r.ToStep.Id);
                    result.Add(new ProcedureRouteTree()
                    {
                        SelfReferencedRouteKind = selfReferenced?.RouteKind ?? RouteType.None,
                        RouteKind = route.RouteKind,
                        Step = route.ToStep,
                        ChildrenRoutes = parents.Contains(route.ToStep.Id)?new List<ProcedureRouteTree>(): giveMeChildrenRoutes(route.ToStep.Id, allRoutes, ref parents)
                    });
                }
            }

            return result;
        }

        protected WorkPackageItem getWorkPackage(int id)
        {
            string workPackageResponse = null;
            using (HttpResponseMessage responseMessage = GetSharepointItem(ProcedureWorkPackageListId, id))
            {
                workPackageResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            WorkPackageItem workPackage = ((JObject)JsonConvert.DeserializeObject(workPackageResponse)).ToObject<WorkPackageItem>();
            return workPackage;
        }

        protected List<BusinessItem> getAllBusinessItems(int workPackageId)
        {
            string businessItemResponse = null;

            // Sharepoint 
            using (HttpResponseMessage responseMessage = GetSharepointList(ProcedureBusinessItemListId, filter: $"BelongsTo/ID eq {workPackageId}"))
            {
                businessItemResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonBusinessItem = (JObject)JsonConvert.DeserializeObject(businessItemResponse);
            List<BusinessItem> businessItemList = jsonBusinessItem.SelectToken("value").ToObject<List<BusinessItem>>();

            // Azure SQL
            // List<BusinessItem> businessItemListFromSQL = GetSqlList<BusinessItem>($"select bi.Id as Id, bi.BusinessItemDate as [Date], bi.WebLink as Weblink from ProcedureBusinessItem bi inner join ProcedureBusinessItemProcedureWorkPackage biwp on bi.Id = biwp.ProcedureBusinessItemId where ProcedureWorkPackageId = {workPackageId}").ToList();

            return businessItemList;
        }

        protected List<RouteItem> getAllRoutes(int procedureId)
        {
            string routeResponse = null;
            
            // Sharepoint
            using (HttpResponseMessage responseMessage = GetSharepointList(ProcedureRouteListId, filter: $"Procedure/ID eq {procedureId}"))
            {
                routeResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonRoute = (JObject)JsonConvert.DeserializeObject(routeResponse);
            List<RouteItem> routes = ((JArray)jsonRoute.SelectToken("value")).ToObject<List<RouteItem>>();

            // Azure SQL 
            // List<RouteItem> routesfromSQL = GetSqlList<RouteItem>($"select * from ProcedureRoute where ProcedureId = {procedureId}").ToList();

            return routes;
        }

    }
}