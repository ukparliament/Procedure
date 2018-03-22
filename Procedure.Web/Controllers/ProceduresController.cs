using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Procedure.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;

namespace Procedure.Web.Controllers
{
    [RoutePrefix("Procedures")]
    public class ProceduresController : BaseController
    {
        [Route("~/")]
        [Route]
        public ActionResult Index()
        {
            return ShowList<ProcedureItem>(ProcedureListId);
        }

        [Route("{id:int}")]
        public ActionResult Details(int id)
        {
            ProcedureDetailViewModel viewModel = new ProcedureDetailViewModel();

            string procedureResponse = null;
            using (HttpResponseMessage responseMessage = GetItem(ProcedureListId, id))
            {
                procedureResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            viewModel.Procedure = ((JObject)JsonConvert.DeserializeObject(procedureResponse)).ToObject<ProcedureItem>();

            string routeResponse = null;
            using (HttpResponseMessage responseMessage = GetList(ProcedureRouteListId, filter: $"Procedure/ID eq {id}"))
            {
                routeResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonRoute = (JObject)JsonConvert.DeserializeObject(routeResponse);
            viewModel.Routes = ((JArray)jsonRoute.SelectToken("value")).ToObject<List<RouteItem>>();
            RouteItem[] filteredRouteItems = viewModel.Routes.Where(to => to.FromStep.Id != to.ToStep.Id).ToArray();
            int[] entrySteps = filteredRouteItems
                .Where(r => filteredRouteItems.Any(to => to.ToStep.Id == r.FromStep.Id) == false)
                .Select(r => r.FromStep.Id)
                .Distinct()
                .ToArray();
            viewModel.EntryPoints = entrySteps
                .Select(s => viewModel.Routes.FirstOrDefault(r => r.FromStep.Id == s).FromStep.ToSharepointItem<StepItem>())
                .ToList();

            return View(viewModel);
        }

    }
}