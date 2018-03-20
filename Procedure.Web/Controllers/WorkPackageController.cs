using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Procedure.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace Procedure.Web.Controllers
{
    public class WorkPackageController : BaseController
    {
        public ActionResult Index()
        {
            return ShowList<WorkPackageItem>(ProcedureWorkPackageListId);
        }

        public ActionResult Details(int id)
        {
            WorkPackageDetailViewModel viewModel = new WorkPackageDetailViewModel();

            string workPackageResponse = null;
            using (HttpResponseMessage responseMessage = GetItem(ProcedureWorkPackageListId, id))
            {
                workPackageResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            WorkPackageItem workPackage = ((JObject)JsonConvert.DeserializeObject(workPackageResponse)).ToObject<WorkPackageItem>();
            viewModel.WorkPackageName = workPackage.Title;

            string businessItemResponse = null;
            using (HttpResponseMessage responseMessage = GetList(ProcedureBusinessItemListId, filter: $"BelongsTo/ID eq {id}"))
            {
                businessItemResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonRoute = (JObject)JsonConvert.DeserializeObject(businessItemResponse);
            viewModel.BusinessItems= ((JArray)jsonRoute.SelectToken("value")).ToObject<List<BusinessItem>>();

            return View(viewModel);
        }
    }
}