﻿using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class WorkPackageDetailViewModel
    {
        public WorkPackageItem WorkPackage { get; set; }
        public List<BusinessItem> BusinessItems { get; set; }
    }
}