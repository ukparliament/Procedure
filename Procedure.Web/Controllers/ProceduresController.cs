using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Procedure.Web.Models;
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
            
            viewModel.Tree = GenerateProcedureTree(id);

            return View(viewModel);
        }

    }
}