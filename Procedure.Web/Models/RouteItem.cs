using Parliament.Model;
using System;

namespace Procedure.Web.Models
{
    public class RouteItem
    {
        public int Id { get; set; }

        public string TripleStoreId { get; set; }

        public int FromStepId { get; set; }

        public string FromStepTripleStoreId { get; set; }

        public string FromStepName { get; set; }

        public int ToStepId { get; set; }

        public string ToStepTripleStoreId { get; set; }

        public string ToStepName { get; set; }

        public string RouteTypeName { get; set; }

        public RouteType RouteKind
        {
            get
            {
                return (RouteType)Enum.Parse(typeof(RouteType), RouteTypeName ?? RouteType.None.ToString());
            }
        }

        public ProcedureRoute GiveMeMappedObject()
        {
            ProcedureRoute result = new ProcedureRoute();
            result.Id = new System.Uri($"https://id.parliament.uk/{TripleStoreId}");
            result.ProcedureRouteIsToProcedureStep = new ProcedureStep[]
            {
                new ProcedureStep()
                {
                    Id=new System.Uri($"https://id.parliament.uk/{FromStepTripleStoreId}"),
                    ProcedureStepName=FromStepName
                }
            };
            result.ProcedureRouteIsFromProcedureStep = new ProcedureStep[]
            {
                new ProcedureStep()
                {
                    Id=new System.Uri($"https://id.parliament.uk/{ToStepTripleStoreId}"),
                    ProcedureStepName=ToStepName
                }
            };

            return result;
        }

        public static string ListByProcedureSql = @"select pr.Id, pr.TripleStoreId, fs.Id as FromStepId,
	            fs.ProcedureStepName as FromStepName, fs.TripleStoreId as FromStepTripleStoreId,
                ts.Id as ToStepId, ts.ProcedureStepName as ToStepName, ts.TripleStoreId as ToStepTripleStoreId,
	            rt.ProcedureRouteTypeName as RouteTypeName from ProcedureRoute pr
            join ProcedureStep fs on fs.Id=pr.FromProcedureStepId
            join ProcedureStep ts on fs.Id=pr.ToProcedureStepId
            join ProcedureRouteType rt on rt.Id=pr.ProcedureRouteTypeId
            where pr.IsDeleted=0 and pr.ProcedureId=@ProcedureId";

        public static string ListByStepSql = @"select pr.Id, pr.TripleStoreId, fs.Id as FromStepId,
	            fs.ProcedureStepName as FromStepName, fs.TripleStoreId as FromStepTripleStoreId,
                ts.Id as ToStepId, ts.ProcedureStepName as ToStepName, ts.TripleStoreId as ToStepTripleStoreId,
	            rt.ProcedureRouteTypeName as RouteTypeName from ProcedureRoute pr
            join ProcedureStep fs on fs.Id=pr.FromProcedureStepId
            join ProcedureStep ts on fs.Id=pr.ToProcedureStepId
            join ProcedureRouteType rt on rt.Id=pr.ProcedureRouteTypeId
            where pr.IsDeleted=0 and ((pr.FromProcedureStepId=@StepId) or (pr.ToProcedureStepId=@StepId))";

    }

}