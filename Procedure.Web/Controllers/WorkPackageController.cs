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
        // GET: WorkPackage
        public ActionResult Index()
        {
            List<WorkPackage> workPackages = new List<WorkPackage>();
            workPackages.Add(new WorkPackage { Id = 1, Title = "Passage of public bill 03d3" });
            workPackages.Add(new WorkPackage { Id = 3, Title = "Passage of SI 1qw2" });
            workPackages.Add(new WorkPackage { Id = 5, Title = "Passage of public bill fg43" });

            //HttpResponseMessage responseMessage = GetList(WorkPackageListId);
            //string response = responseMessage.Content.ReadAsStringAsync().Result;
            //JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);
            //foreach (JObject item in (JArray)jsonResponse.SelectToken("value"))
            //{
            //    workPackages.Add(new WorkPackage()
            //    {
            //        Id = item["ID"].ToObject<int>(),
            //        Title = item["Title"].ToString(),
            //    });
            //}

            return View(workPackages);
        }

        public ActionResult Details(int id)
        {
            List<WorkPackageProcedureItem> procedures = new List<WorkPackageProcedureItem>();

            procedures.Add(new WorkPackageProcedureItem { Id = 1, WorkPackageId = id, SubjectToProcedureId = 473 });

            //HttpResponseMessage responseMessage = GetList(WorkPackageProcedureListId, filter: $"WorkPackage/ID eq {id}");
            //string response = responseMessage.Content.ReadAsStringAsync().Result;
            //JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);
            //foreach (JObject item in (JArray)jsonResponse.SelectToken("value"))
            //{
            //    procedures.Add(new WorkPackageProcedureItem()
            //    {
            //        Id = item["ID"].ToObject<int>(),
            //        WorkPackageId = item["WorkPackage"].ToObject<int>(),
            //        SubjectToProcedureId = item["SubjectToProcedure"].ToObject<int>()
            //    });
            //}

            return View(procedures);
        }
    }
}