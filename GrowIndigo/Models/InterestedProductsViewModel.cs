using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class InterestedProductsViewModel
    {

        public long Id { get; set; }
        public long Tr_Id { get; set; }
        public string Fk_MobileNumber { get; set; }
        public string BuyerId { get; set; }
        public string ProductId { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string BuyerAddress { get; set; }
        public string CropName { get; set; }
        public string VarietyName { get; set; }
        public string Quantity { get; set; }
        public string QualitySpecification { get; set; }
        public string DeliveryLocation { get; set; }
        public string ExpectedPrice { get; set; }
        public string IsPriceNegotiable { get; set; }
        public string Remarks { get; set; }

    }
}