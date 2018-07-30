using Dapper;
using Procedure.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Procedure.Web.Controllers
{
    public class BaseController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["Sql"].ConnectionString;

        internal T GetSqlItem<T>(string sql, object parameters = null) where T : new()
        {
            T result = new T();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                CommandDefinition command = new CommandDefinition(sql, parameters);
                result = connection.Query<T>(command).SingleOrDefault();
            }
            return result;
        }

        internal List<T> GetSqlList<T>(string sql, object parameters = null)
        {
            List<T> result = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                CommandDefinition command = new CommandDefinition(sql, parameters);
                result = connection.Query<T>(command).AsList();
            }
            return result;
        }

        internal Tuple<List<T>, List<K>> GetSqlList<T, K>(string sql, object parameters = null)
        {
            List<T> resultT = new List<T>();
            List<K> resultK = new List<K>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                CommandDefinition command = new CommandDefinition(sql, parameters);
                using (SqlMapper.GridReader grid = connection.QueryMultiple(command))
                {
                    resultT = grid.Read<T>().ToList();
                    resultK = grid.Read<K>().ToList();
                }
            }
            return Tuple.Create<List<T>, List<K>>(resultT, resultK);
        }

        internal ActionResult ShowList<T>(string sql)
        {
            List<T> items = GetSqlList<T>(sql);

            return View(items);
        }

        internal List<ProcedureRouteTree> GenerateProcedureTree(int procedureId)
        {
            List<ProcedureRouteTree> result = new List<ProcedureRouteTree>();

            List<RouteItem> routes = getAllRoutes(procedureId);
            RouteItem[] filteredRouteItems = routes.Where(to => to.FromStepId != to.ToStepId).ToArray();
            int[] entrySteps = filteredRouteItems
                .Where(r => filteredRouteItems.Any(to => to.ToStepId == r.FromStepId) == false)
                .Select(r => r.FromStepId)
                .Distinct()
                .ToArray();
            RouteItem[] entryRoutes = entrySteps
                .Select(s => routes.FirstOrDefault(r => r.FromStepId == s))
                .ToArray();
            List<ProcedureRouteTree> tree = new List<ProcedureRouteTree>();
            List<int> parents = new List<int>();
            foreach (RouteItem route in entryRoutes)
            {
                RouteItem selfReferenced = routes
                    .FirstOrDefault(r => r.FromStepId == route.ToStepId && r.FromStepId == r.ToStepId);
                tree.Add(new ProcedureRouteTree()
                {
                    SelfReferencedRouteKind = selfReferenced?.RouteKind ?? RouteType.None,
                    RouteKind = RouteType.None,
                    Step = new SharepointLookupItem()
                    {
                        Id = route.FromStepId,
                        Value = route.FromStepName
                    },
                    ChildrenRoutes = giveMeChildrenRoutes(route.FromStepId, routes, ref parents)
                });
            }

            return tree;
        }

        private List<ProcedureRouteTree> giveMeChildrenRoutes(int parentStepId, List<RouteItem> allRoutes, ref List<int> parents)
        {
            List<ProcedureRouteTree> result = new List<ProcedureRouteTree>();
            parents.Add(parentStepId);
            RouteItem[] children = allRoutes.Where(r => r.FromStepId == parentStepId).ToArray();
            foreach (RouteItem route in children)
            {
                if (route.ToStepId != parentStepId)
                {
                    RouteItem selfReferenced = allRoutes
                        .FirstOrDefault(r => r.FromStepId == route.ToStepId && r.FromStepId == r.ToStepId);
                    result.Add(new ProcedureRouteTree()
                    {
                        SelfReferencedRouteKind = selfReferenced?.RouteKind ?? RouteType.None,
                        RouteKind = route.RouteKind,
                        Step = new SharepointLookupItem()
                        {
                            Id = route.ToStepId,
                            Value = route.ToStepName
                        },
                        ChildrenRoutes = parents.Contains(route.ToStepId) ? new List<ProcedureRouteTree>() : giveMeChildrenRoutes(route.ToStepId, allRoutes, ref parents)
                    });
                }
            }

            return result;
        }

        protected WorkPackageItem getWorkPackage(int id)
        {
            WorkPackageItem workPackage = GetSqlItem<WorkPackageItem>(WorkPackageItem.ItemSql, new { Id = id });
            return workPackage;
        }

        protected List<BusinessItem> getAllBusinessItems(int workPackageId)
        {
            List<BusinessItem> businessItemList = GetSqlList<BusinessItem>(BusinessItem.ListByWorkPackageSql, new { WorkPackageId = workPackageId });
            List<BusinessItemStep> steps = GetSqlList<BusinessItemStep>(BusinessItemStep.ListByWorkPackageSql, new { WorkPackageId = workPackageId });
            businessItemList.ForEach(bi => bi.ActualisesProcedureStep = steps.Where(s => s.BusinessItemId == bi.Id).ToList());

            return businessItemList;
        }

        protected List<RouteItem> getAllRoutes(int procedureId)
        {
            Tuple<List<RouteItem>, List<StepHouse>> routesAndSteps = GetSqlList<RouteItem, StepHouse>(RouteItem.ListByProcedureSql, new { ProcedureId = procedureId });

            routesAndSteps.Item1
                .ForEach(r => {
                    r.FromStepHouseName =
                        string.Join(",", routesAndSteps.Item2
                            .Where(s => s.ProcedureStepId == r.FromStepId).Select(s => s.HouseName));
                    r.ToStepHouseName = string.Join(",", routesAndSteps.Item2
                        .Where(s => s.ProcedureStepId == r.ToStepId).Select(s => s.HouseName));
                });

            return routesAndSteps.Item1;
        }

    }
}