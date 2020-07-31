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
        public string Culture { get; set; }
        public int orderId { get; set; }
        public string ProductId { get; set; }
        public int IntProductId { get; set; }
        public string CropName { get; set; }
        public string Qty { get; set; }
        public long SellerId { get; set; }
        public string SellerName { get; set; }
        public string SellerContact { get; set; }
        public string SellerAddress { get; set; }
        public string BuyerName { get; set; }
        public string BuyerContact { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerId { get; set; }
      
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
        public int Tr_Id { get; set; }
        public Nullable<int> CropId { get; set; }
        public string CategoryName { get; set; }
        public Nullable<int> VarietyId { get; set; }
        public string VarietyName { get; set; }
        public string ProductAddress { get; set; }
        public string GeoAddress { get; set; }
        public string MobileNumber { get; set; }
        public string NetBankingId { get; set; }
        public string Quantity { get; set; }
        public string QuantityUnit { get; set; }
        public string QualitySpecification { get; set; }
        public string DeliveryLocation { get; set; }
        public string ExpectedPrice { get; set; }
        public string Price { get; set; }
        public Nullable<System.DateTime> ExpectedDate { get; set; }
        public Nullable<bool> IsPriceNegotiable { get; set; }
        public string Remarks { get; set; }



        public Nullable<System.DateTime> AvailabilityDate { get; set; }
        public string PaymentMethod { get; set; }
        public Nullable<bool> IsQualityTestNeeded { get; set; }
        public Nullable<bool> IsLogisticNeeded { get; set; }
        public string ProductImageUrl { get; set; }
        public Nullable<System.DateTime> Tr_Date { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public string Taluka { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string ProductPriority { get; set; }
        public string SecondaryProductImage { get; set; }
        public string ProductDescription { get; set; }
        public string Village { get; set; }
        public Nullable<bool> IsApproved { get; set; }
        public Nullable<int> MaximumPrice { get; set; }
        public Nullable<int> MinimumPrice { get; set; }

    }
}