using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class ProductMasterViewModel
    {
        public int Tr_Id { get; set; }
        public int ProductId { get; set; }
        public int? CropId { get; set; }
        public string CategoryName { get; set; }
        public int? VarietyId { get; set; }
        public string CropName { get; set; }
        public string VarietyName { get; set; }
        public string NewVariety { get; set; }
        public string ProductAddress { get; set; } 
        public string GeoAddress { get; set; }
        public string MobileNumber { get; set; }
        public string NetBankingId { get; set; }
        public string Quantity { get; set; }
        public string QuantityUnit { get; set; }
        public int? Price { get; set; }
        public Nullable<decimal> ServiceTax { get; set; }
        public Nullable<int> TotalAmount { get; set; }
        public Nullable<System.DateTime> AvailabilityDate { get; set; }
        public string PaymentMethod { get; set; }
        public Nullable<bool> IsQualityTestNeeded { get; set; }
        public Nullable<bool> IsLogisticNeeded { get; set; }
        public string ProductImageUrl { get; set; }
        public string SecondaryProductImage { get; set; }

        public Nullable<System.DateTime> Tr_Date { get; set; }
        public string StateCode { get; set; }
        public string DistrictCode{ get; set; }
        public string TalukaCode { get; set; }
        public int OrderId { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string ProductPriority { get; set; }
        public string ProductDescription { get; set; }


        //public double Latitude { get; set; }
        //public double Longitude { get; set; }


    }

    public class MandiProduct
    {
        public List<ProductMasterViewModel> Products { get; set; }

    }

    public class FilterMandiProduct
    {
        //public IQueryable<ProductMasterViewModel> Products { get; set; }
        public List<ProductMasterViewModel> Products { get; set; }

    }

    public class ProductDetail
    {
        public string MobileNumber { get; set; }
        public int Tr_Id { get; set; }


    }

    public class ProductInfo
    {
        public string MobileNumber { get; set; }
        public int Tr_Id { get; set; }
        public int? CropId { get; set; }
        public int? VarietyId { get; set; }
        public string CropName { get; set; }
        public string CategoryName { get; set; }

        public string VarietyName { get; set; }
        public string ProductAddress { get; set; }
        public string GeoAddress { get; set; }
        public string Quantity { get; set; }
        public string QuantityUnit { get; set; }
        public int? Price { get; set; }
        public Nullable<decimal> ServiceTax { get; set; }
        public Nullable<int> TotalAmount { get; set; }
        public Nullable<System.DateTime> AvailabilityDate { get; set; }
        public Nullable<bool> IsQualityTestNeeded { get; set; }
        public string ProductImageUrl { get; set; }
        public string SecondaryProductImage { get; set; }
        public string StateCode { get; set; }
        public string DistrictCode { get; set; }
        public string TalukaCode { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string PaymentMethod { get; set; }
        public string ProductDescription { get; set; }

        public Nullable<bool> IsLogisticNeeded { get; set; }
    }
}

