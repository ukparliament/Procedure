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
	            coalesce(si.ProcedureStatutoryInstrumentName, nsi.ProcedureProposedNegativeStatutoryInstrumentName) as WorkPackageName,
                bi.BusinessItemDate as [Date], bi.WebLink, lb.LayingBodyName
            from ProcedureBusinessItem bi
            join ProcedureWorkPackagedThing wp on wp.Id=bi.ProcedureWorkPackageId
            left join ProcedureStatutoryInstrument si on si.Id=wp.Id
            left join ProcedureProposedNegativeStatutoryInstrument nsi on nsi.Id=wp.Id
            left join ProcedureLaying l on l.ProcedureBusinessItemId=bi.Id
            left join LayingBody lb on lb.Id=l.LayingBodyId
            where wp.Id=@WorkPackageId";

        public static string ListByStepSql = @"select bi.Id, bi.TripleStoreId, wp.Id as WorkPackageId,
	            coalesce(si.ProcedureStatutoryInstrumentName, nsi.ProcedureProposedNegativeStatutoryInstrumentName) as WorkPackageName,
                bi.BusinessItemDate as [Date], bi.WebLink, lb.LayingBodyName
            from ProcedureBusinessItem bi
            join ProcedureWorkPackagedThing wp on wp.Id=bi.ProcedureWorkPackageId
            join ProcedureBusinessItemProcedureStep bips on bips.ProcedureBusinessItemId=bi.Id
            left join ProcedureStatutoryInstrument si on si.Id=wp.Id
            left join ProcedureProposedNegativeStatutoryInstrument nsi on nsi.Id=wp.Id
            left join ProcedureLaying l on l.ProcedureBusinessItemId=bi.Id
            left join LayingBody lb on lb.Id=l.LayingBodyId
            where bips.ProcedureStepId=@StepId";

    }
}