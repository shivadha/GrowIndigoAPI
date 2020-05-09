using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class MandiUserRequirementViewModel
    {
        public long Id { get; set; }
        public string MobileNumber { get; set; }
        public string BuyerId { get; set; }
        public string BuyerContact { get; set; }
        public string BuyerAddress { get; set; }
        public string CropName { get; set; }
        public string Variety { get; set; }
        public string Quantity { get; set; }
        public string QualitySpecification { get; set; }
        public string DeliveryLocation { get; set; }
        public string ExpectedPrice { get; set; }
        public Nullable<System.DateTime> ExpectedDate { get; set; }
        public Nullable<bool> IsPriceNegotiable { get; set; }
        public string Remarks { get; set; }
    }
}