using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class CountViewModel
    {
        public int SellerCount { get; set; }
        public int ProductCount { get; set; }
        public string CountType { get; set; }
        public string Category { get; set; }
        public string State { get; set; }
        public int CategoryId { get; set; }
        public int StateId { get; set; }
    }
}