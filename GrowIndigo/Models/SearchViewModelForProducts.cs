using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class SearchViewModelForProducts
    {
        public string culture { get; set; }
        public string searchtxt { get; set; }
        public string MobileNumber { get; set; }
        public long StateId { get; set; }
        public long CategoryId { get; set; }
    }
}