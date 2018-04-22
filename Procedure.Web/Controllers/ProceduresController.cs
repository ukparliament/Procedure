using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Procedure.Web.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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

        [Route("{id:int}/graph")]
        public ActionResult Graph(int id)
        {
            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);

            byte[] output = wrapper.GenerateGraph(GiveMeDotString(id), Enums.GraphReturnType.Svg);
            string graph = string.Format("data:image/svg+xml;base64,{0}", Convert.ToBase64String(output));
            return File(output, "image/svg+xml");
        }

        [Route("{id:int}/graph.dot")]
        public ActionResult GraphDot(int id)
        {
            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);

            byte[] output = wrapper.GenerateGraph(GiveMeDotString(id), Enums.GraphReturnType.Plain);
            return File(output, "text/plain");
        }

        private string GiveMeDotString(int procedureId)
        {
            string routeResponse = null;
            using (HttpResponseMessage responseMessage = GetList(ProcedureRouteListId, filter: $"Procedure/ID eq {procedureId}"))
            {
                routeResponse = responseMessage.Content.ReadAsStringAsync().Result;
            }
            JObject jsonRoute = (JObject)JsonConvert.DeserializeObject(routeResponse);
            List<RouteItem> routes = ((JArray)jsonRoute.SelectToken("value")).ToObject<List<RouteItem>>();

            StringBuilder builder = new StringBuilder("graph [fontname = \"calibri\"]; node[fontname = \"calibri\"]; edge[fontname = \"calibri\"];");
            foreach (RouteItem route in routes)
            {
                if ((int)route.RouteKind == 1) { builder.Append("\"" + route.FromStep.Value + "\" -> \"" + route.ToStep.Value + "\" [label = \"Causes\"]; "); }
                if ((int)route.RouteKind == 2) { builder.Append("edge [style=dashed]; \"" + route.FromStep.Value + "\" -> \"" + route.ToStep.Value + "\" [label = \"Allows\"]; edge [style=solid];"); }
                if ((int)route.RouteKind == 3) { builder.Append("edge [color=red]; \"" + route.FromStep.Value + "\" -> \"" + route.ToStep.Value + "\" [label = \"Precludes\"]; edge [color=black];"); }
            }

            // Add a legend
            builder.Append("subgraph cluster_key {" +
                "label=\"Key\"; labeljust=\"l\";" +
                "k1[label=\"Step\"]; node [shape=plaintext];" +
                "k3 [label=<<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" cellborder=\"0\"> " +
                "<tr><td align=\"right\" port=\"i1\" > Allows </td></tr>" +
                "<tr><td align=\"right\" port=\"i2\"> Causes </td></tr>" +
                "<tr><td align=\"right\" port=\"i3\" > Precludes </td></tr> </table>>];" +
                "k3e [label =<<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" cellborder=\"0\">" +
                "<tr><td port=\"i1\" > &nbsp;</td></tr> <tr><td port=\"i2\"> &nbsp;</td></tr> <tr><td port=\"i3\"> &nbsp;</td></tr> </table>>];" +
                "k3:i1:e -> k3e:i1:w [style=dashed] k3:i2:e->k3e:i2:w k3:i3:e->k3e:i3:w[color = red] { rank = same; k3 k3e k1 } };");

            builder.Insert(0, "digraph{");
            builder.Append("}");

            return builder.ToString();
        }

    }
}