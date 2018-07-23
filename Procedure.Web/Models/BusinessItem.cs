using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Web.Models
{
    public class BusinessItem : BaseSharepointItem
    {
        public int WorkPackageId { get; set; }

        public string WorkPackageName { get; set; }
        
        public List<BusinessItemStep> ActualisesProcedureStep { get; set; }

        public DateTimeOffset? Date { get; set; }

        public string Weblink { get; set; } 

        public string LayingBodyName { get; set; }

        public string AllData()
        {
            StringBuilder sb = new StringBuilder();
            if (Date != null)
            {
                sb.Append($"{Date.Value.ToString("dd/MM/yyyy")}_");
            }
            if (!String.IsNullOrEmpty(Weblink))
            {
                sb.Append($"{Weblink}_");
            } 
            if (string.IsNullOrWhiteSpace(LayingBodyName))
            {
                sb.Append(LayingBodyName);
            }
            return sb.ToString();
        }

        public static string ListByWorkPackageSql = @"select bi.Id, bi.TripleStoreId, wp.Id as WorkPackageId,
	            wp.ProcedureWorkPackageableThingName as WorkPackageName,
                bi.BusinessItemDate as [Date], bi.WebLink, lb.LayingBodyName
            from ProcedureBusinessItem bi
            join ProcedureWorkPackageableThing wp on wp.Id=bi.ProcedureWorkPackageId
            left join LayingBody lb on lb.Id=bi.LayingBodyId
            where bi.IsDeleted=0 and wp.Id=@WorkPackageId";

        public static string ListByStepSql = @"select bi.Id, bi.TripleStoreId, wp.Id as WorkPackageId,
	            wp.ProcedureWorkPackageableThingName as WorkPackageName,
	            ps.Id, ps.ProcedureStepName as [Value], bi.BusinessItemDate as [Date],
	            bi.WebLink, lb.LayingBodyName
            from ProcedureBusinessItem bi
            join ProcedureWorkPackageableThing wp on wp.Id=bi.ProcedureWorkPackageId
            join ProcedureBusinessItemProcedureStep bips on bips.ProcedureBusinessItemId=bi.Id
            join ProcedureStep ps on ps.Id=bips.ProcedureStepId
            left join LayingBody lb on lb.Id=bi.LayingBodyId
            where bi.IsDeleted=0 and bips.ProcedureStepId=@StepId";

    }
}