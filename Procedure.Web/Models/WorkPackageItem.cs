using System;

namespace Procedure.Web.Models
{

    public class WorkPackageItem : BaseSharepointItem
    {
        public int ProcedureId { get; set; }

        public string ProcedureName { get; set; }

        public string TripleStoreId { get; set; }

        public string SIPrefix { get; set; }

        public string SIYear { get; set; }

        public string SINumber { get; set; }

        public string ComingIntoForceNote { get; set; }

        public DateTimeOffset? ComingIntoForceDate { get; set; }

        public string WorkPackageableThingURL { get; set; }

        public static string ListSql = @"select wp.Id,
	            coalesce(si.ProcedureStatutoryInstrumentName, nsi.ProcedureProposedNegativeStatutoryInstrumentName) as Title,
	            wp.ProcedureWorkPackageTripleStoreId as TripleStoreId, si.StatutoryInstrumentNumberPrefix as SIPrefix,
	            si.StatutoryInstrumentNumberYear as SIYear,
	            si.StatutoryInstrumentNumber as SINumber, si.ComingIntoForceNote,
	            si.ComingIntoForceDate, wp.WebLink as WorkPackageableThingURL,
	            p.Id as ProcedureId,
                p.ProcedureName
            from ProcedureWorkPackagedThing wp
            left join ProcedureStatutoryInstrument si on si.Id=wp.Id
            left join ProcedureProposedNegativeStatutoryInstrument nsi on nsi.Id=wp.Id
            join [Procedure] p on p.Id=wp.ProcedureId";

        public static string ItemSql = @"select wp.Id,
	            coalesce(si.ProcedureStatutoryInstrumentName, nsi.ProcedureProposedNegativeStatutoryInstrumentName) as Title,
	            wp.ProcedureWorkPackageTripleStoreId as TripleStoreId, si.StatutoryInstrumentNumberPrefix as SIPrefix,
	            si.StatutoryInstrumentNumberYear as SIYear,
	            si.StatutoryInstrumentNumber as SINumber, si.ComingIntoForceNote,
	            si.ComingIntoForceDate, wp.WebLink as WorkPackageableThingURL,
	            p.Id as ProcedureId,
                p.ProcedureName
            from ProcedureWorkPackagedThing wp
            left join ProcedureStatutoryInstrument si on si.Id=wp.Id
            left join ProcedureProposedNegativeStatutoryInstrument nsi on nsi.Id=wp.Id
            join [Procedure] p on p.Id=wp.ProcedureId
            where wp.Id=@Id";
    }
}