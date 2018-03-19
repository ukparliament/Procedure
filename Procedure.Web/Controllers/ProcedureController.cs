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
            string response = null;
            using (HttpResponseMessage responseMessage = GetList(ProcedureListId))
            {
                response = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);
            procedures = ((JArray)jsonResponse.SelectToken("value")).ToObject<List<ProcedureItem>>();
            
            return View(procedures);
        }

        public ActionResult Details(int id)
        {
            ProcedureDetailViewModel viewModel = new ProcedureDetailViewModel();

            string procedureResponse = null;
            using (HttpResponseMessage responseMessage = GetItem(ProcedureListId, id))
            {
                procedureResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            ProcedureItem procedure = ((JObject)JsonConvert.DeserializeObject(procedureResponse)).ToObject<ProcedureItem>();
            viewModel.ProcedureName = procedure.Title;

            string routeResponse = null;
            using (HttpResponseMessage responseMessage = GetList(ProcedureRouteListId, filter: $"Procedure/ID eq {id}"))
            {
                routeResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonRoute = (JObject)JsonConvert.DeserializeObject(routeResponse);
            viewModel.Routes=((JArray)jsonRoute.SelectToken("value")).ToObject<List<ProcedureRouteItem>>();
            
            return View(viewModel);
        }

    }
}