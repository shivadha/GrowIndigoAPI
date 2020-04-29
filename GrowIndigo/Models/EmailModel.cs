using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class EmailModel
    {
        [Required, Display(Name = "Your name")]
        public string toname { get; set; }
        [Required, Display(Name = "Your email"), EmailAddress]
        public string toemail { get; set; }
        [Required]
        public string subject { get; set; }
        [Required]
        public string message { get; set; }
        public int orderId { get; set; }
        public string ProductId { get; set; }
        public string CropName { get; set; }
        public string Qty { get; set; }
        public long SellerId { get; set; }
        public string SellerName { get; set; }
        public string SellerContact { get; set; }
        public string SellerAddress { get; set; }
        public string BuyerName { get; set; }
        public string BuyerContact { get; set; }
        public string BuyerAddress { get; set; }
        public long BuyerId { get; set; }
        public int Price { get; set; }
        public string ServiceTax { get; set; }
        public string Rate { get; set; }
        public string TotalAmount { get; set; }
        //to be paid by buyer
        public string OrderValue { get; set; }
        //value of money that needs to be transferred to farmer
        public string PaymentToFarmer { get; set; }
        public string Bankdetailsofseller { get; set; }
        public string PaymentStatus { get; set; }
        public string ReceiptStatus { get; set; }
        public string OrderStatus { get; set; }

    }
}