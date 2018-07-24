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

        public DateTimeOffset? ObjectionDeadline { get; set; }

        public static string ListSql = @"select wp.Id, wp.ProcedureWorkPackageableThingName as Title,
	            wp.ProcedureWorkPackageTripleStoreId as TripleStoreId, wp.StatutoryInstrumentNumberPrefix as SIPrefix,
	            wp.StatutoryInstrumentNumberYear as SIYear,
	            wp.StatutoryInstrumentNumber as SINumber, wp.ComingIntoForceNote,
	            wp.ComingIntoForceDate, wp.WebLink as WorkPackageableThingURL,
	            wp.TimeLimitForObjectionEndDate as ObjectionDeadline, p.Id as ProcedureId,
                p.ProcedureName
            from ProcedureWorkPackageableThing wp
            join [Procedure] p on p.Id=wp.ProcedureId
            where wp.IsDeleted=0";

        public static string ItemSql = @"select wp.Id, wp.ProcedureWorkPackageableThingName as Title,
	            wp.ProcedureWorkPackageTripleStoreId as TripleStoreId, wp.StatutoryInstrumentNumberPrefix as SIPrefix,
	            wp.StatutoryInstrumentNumberYear as SIYear,
	            wp.StatutoryInstrumentNumber as SINumber, wp.ComingIntoForceNote,
	            wp.ComingIntoForceDate, wp.WebLink as WorkPackageableThingURL,
	            wp.TimeLimitForObjectionEndDate as ObjectionDeadline, p.Id as ProcedureId,
                p.ProcedureName
            from ProcedureWorkPackageableThing wp
            join [Procedure] p on p.Id=wp.ProcedureId
            where wp.IsDeleted=0 and wp.Id=@Id";
    }
}