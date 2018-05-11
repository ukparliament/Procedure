using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Procedure.Web.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Mvc;

namespace Procedure.Web.Controllers
{
    [RoutePrefix("Steps")]
    public class StepsController : BaseController
    {
        [Route]
        public ActionResult Index()
        {
            return ShowList<StepItem>(ProcedureStepListId);
        }

        [Route("{id:int}")]
        public ActionResult Details(int id)
        {
            StepDetailViewModel viewModel = new StepDetailViewModel();

            string stepResponse = null;
            using (HttpResponseMessage responseMessage = GetItem(ProcedureStepListId, id))
            {
                stepResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }

            StepItem stepItem = ((JObject)JsonConvert.DeserializeObject(stepResponse)).ToObject<StepItem>();

            if (stepItem.Id != 0)
            {
                viewModel.Step = stepItem;

                string routeResponse = null;
                using (HttpResponseMessage responseMessage = GetList(ProcedureRouteListId, filter: $"FromStep/ID eq {id} or ToStep/ID eq {id}"))
                {
                    routeResponse = responseMessage.Content.ReadAsStringAsync().Result;
                }
                JObject jsonRoute = (JObject)JsonConvert.DeserializeObject(routeResponse);
                viewModel.Routes = ((JArray)jsonRoute.SelectToken("value")).ToObject<List<RouteItem>>();

                string businessItemResponse = null;
                using (HttpResponseMessage responseMessage = GetList(ProcedureBusinessItemListId, filter: $"ActualisesProcedureStep/ID eq {id}"))
                {
                    businessItemResponse = responseMessage.Content.ReadAsStringAsync().Result;
                }
                JObject jsonBusinessItem = (JObject)JsonConvert.DeserializeObject(businessItemResponse);
                viewModel.BusinessItems = ((JArray)jsonBusinessItem.SelectToken("value")).ToObject<List<BusinessItem>>();
            }

            return View(viewModel);
        }
    }
}