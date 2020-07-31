using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class CropMasterViewModel
    {
        public string culture { get; set; }
        public bool FilterCategory { get; set; }

        public bool CropAvail { get; set; }
        public string CropStatus { get; set; }
        public Csvfile csvfile { get; set; }

    }
}