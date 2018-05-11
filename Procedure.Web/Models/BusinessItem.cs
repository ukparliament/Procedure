using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Web.Models
{
    public class BusinessItem : BaseSharepointItem
    {
        public List<SharepointLookupItem> BelongsTo { get; set; }

        public List<SharepointLookupItem> ActualisesProcedureStep { get; set; }

        [JsonProperty(PropertyName = "Businessitem_x0020_date")]
        public DateTime Date { get; set; }

        public string Weblink { get; set; } 

        public SharepointLookupItem LayingBody { get; set; }

        public string AllData()
        {
            StringBuilder sb = new StringBuilder();
            if (Date != null)
            {
                sb.Append($"{Date.ToShortDateString()}_");
            }
            if (!String.IsNullOrEmpty(Weblink))
            {
                sb.Append($"{Weblink}_");
            } 
            if (LayingBody != null)
            {
                sb.Append(LayingBody.Value);
            }
            return sb.ToString();
        }

    }
}