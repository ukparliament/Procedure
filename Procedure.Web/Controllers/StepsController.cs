using Procedure.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Procedure.Web.Controllers
{
    [RoutePrefix("Steps")]
    public class StepsController : BaseController
    {
        [Route]
        public ActionResult Index()
        {
            List<StepItem> steps = GetSqlList<StepItem>(StepItem.ListSql);
            List<ProcedureStepHouse> stepHouses = GetSqlList<ProcedureStepHouse>(ProcedureStepHouse.ListSql);

            steps.ForEach(s => s.Houses = stepHouses.Where(h => h.ProcedureStepId == s.Id).ToArray());

            return View(steps);
        }

        [Route("{id:int}")]
        public ActionResult Details(int id)
        {
            StepDetailViewModel viewModel = new StepDetailViewModel();

            StepItem stepItem = GetSqlItem<StepItem>(StepItem.ItemSql, new { Id = id });
            stepItem.Houses = GetSqlList<ProcedureStepHouse>(ProcedureStepHouse.ListByStepSql, new { StepId = id });

            if (stepItem.Id != 0)
            {
                viewModel.Step = stepItem;

                viewModel.Routes = GetSqlList<RouteItem>(RouteItem.ListByStepSql, new { StepId = id });

                viewModel.BusinessItems = GetSqlList<BusinessItem>(BusinessItem.ListByStepSql, new { StepId = id });
                List<BusinessItemStep> steps = GetSqlList<BusinessItemStep>(BusinessItemStep.ListByStepSql, new { StepId = id});
                viewModel.BusinessItems.ForEach(bi => bi.ActualisesProcedureStep = steps.Where(s => s.BusinessItemId == bi.Id).ToList());

            }

            return View(viewModel);
        }
    }
}