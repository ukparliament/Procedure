using Procedure.Web.Models;
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
    }
}