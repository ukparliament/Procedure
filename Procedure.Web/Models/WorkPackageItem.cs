namespace Procedure.Web.Models
{
    // * Commented code for when work package items no longer have Titles and cannot inherit from BaseSharepointItem class
    
        // public class WorkpackageItem
    public class WorkPackageItem : BaseSharepointItem
    {
        public SharepointLookupItem SubjectTo { get; set; }

        // public string TripleStoreId { get; set;} 
        // public SharepointLookupItem WorkPackageableThing { get; set;}

        //public IWorkPackage GiveMeMappedObject()
        //{
        //    IWorkPackage result = new Parliament.Model.WorkPackage();
        //    result.Id = new System.Uri($"https://id.parliament.uk/{TripleStoreId}");
           
        //    return result;
        //}
    }
}