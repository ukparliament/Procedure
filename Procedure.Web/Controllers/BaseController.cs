using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Procedure.Web.Models;
using System.Collections.Generic;
using System.Configuration;
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

        internal HttpResponseMessage GetList(string listId, int limit = 1000, string filter = null)
        {
            HttpClient client = new HttpClient();

            return client.PostAsJsonAsync<ListRequestObject>(listUri, new ListRequestObject()
            {
                ListId = listId,
                Limit = limit,
                Filter = filter
            }).Result;
        }

        internal HttpResponseMessage GetItem(string listId, int id)
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
            using (HttpResponseMessage responseMessage = GetList(listId))
            {
                response = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);
            items = ((JArray)jsonResponse.SelectToken("value")).ToObject<List<T>>();

            return View(items);
        }
    }
}