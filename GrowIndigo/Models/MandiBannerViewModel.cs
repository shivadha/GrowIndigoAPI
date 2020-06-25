using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class MandiBannerViewModel
    {

        public long Id { get; set; }
        public string BannerTitle { get; set; }
        public string Description { get; set; }
        public Nullable<bool> IsDefault { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string ImageType { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string BannerImage { get; set; }
    }
}