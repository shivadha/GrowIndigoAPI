using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class MandiCartViewModel
    {

        public long CartId { get; set; }
        public string CartType { get; set; }
        public Nullable<int> Fk_EnquiryId { get; set; }
        public int Tr_Id { get; set; }
        public Nullable<long> Fk_InterestedProductId { get; set; }
        public string Deal_Id { get; set; }
        public string StatusType { get; set; }

        public string Product { get; set; }
        public Nullable<long> Product_Id { get; set; }
        public string Quantity_Unit { get; set; }
        public Nullable<int> Price { get; set; }
        public string ProductAddress { get; set; }
        public string BuyerName { get; set; }
        public string ProductImage { get; set; }
        public string BuyerContact { get; set; }
        public string BuyerAddress { get; set; }
        public string SellerName { get; set; }
        public string Seller_MobileNumber { get; set; }
        public string SellerAddress { get; set; }
        public Nullable<decimal> Logistics_Cost { get; set; }
        public Nullable<decimal> Other_Charges { get; set; }
        public Nullable<bool> Status { get; set; }

      
    }
}