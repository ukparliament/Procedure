using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Procedure.Web.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Mvc;

namespace Procedure.Web.Controllers
{
    public class ProcedureController : BaseController
    {
        public ActionResult Index()
        {
            List<ProcedureItem> procedures = new List<ProcedureItem>();
            HttpResponseMessage responseMessage = GetList(ProcedureListId);
            string response = responseMessage.Content.ReadAsStringAsync().Result;
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);
            foreach (JObject item in (JArray)jsonResponse.SelectToken("value"))
            {
                procedures.Add(new ProcedureItem()
                {
                    Id = item["ID"].ToObject<int>(),
                    Title = item["Title"].ToString()
                });
            }

            return View(procedures);
        }

        public ActionResult Details(int id)
        {
            List<ProcedureRouteItem> routes = new List<ProcedureRouteItem>();
            HttpResponseMessage responseMessage = GetList(ProcedureRouteListId, filter: $"Procedure/ID eq {id}");
            string response = responseMessage.Content.ReadAsStringAsync().Result;
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);
            foreach (JObject item in (JArray)jsonResponse.SelectToken("value"))
            {
                routes.Add(new ProcedureRouteItem()
                {
                    Id = item["ID"].ToObject<int>(),
                    FromStepId = item["FromStep"]["Id"].ToObject<int>(),
                    FromStepText = item["FromStep"]["Value"].ToString(),
                    RouteTypeId = item["RouteType"]["Id"].ToObject<int>(),
                    RouteTypeText = item["RouteType"]["Value"].ToString(),
                    ToStepId = item["ToStep"]["Id"].ToObject<int>(),
                    ToStepText = item["ToStep"]["Value"].ToString()
                });
            }
            return View(routes);
        }

    }
}