﻿using System.Collections.Generic;

namespace Procedure.Web.Models
{
    public class RouteViewModel
    {
        public List<ProcedureRouteItem> Routes { get; set; }
        public List<ProcedureRouteItem> Children { get; set; }
    }
}