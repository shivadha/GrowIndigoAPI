using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class MandiUserInfoViewModel
    {
        public int Tr_Id { get; set; }
        public string FullName { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public string Taluka { get; set; }
        public string Pincode { get; set; }
        public string UserType { get; set; }
        public string AdharId { get; set; }
        public string MobileNumber { get; set; }
        public string ProfilePictureUrl { get; set; }
        public int RoleId { get; set; }
        public bool? IsProfileUpdated { get; set; }
        public string EmailId { get; set; }
        public string PanNumber { get; set; }
        public Nullable<bool> IsPermanentAddress { get; set; }
        public Nullable<bool> IsMandiUser { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string ShipAddress { get; set; }
        public string City { get; set; }
        public string ShipMobile { get; set; }
        public string ReceiverName { get; set; }
        public string DeviceToken { get; set; }




    }

    //public class MandiUserMobileNumber
    //{
    //    public string MobileNumber { get; set; }
    //}
    public class MandiUserVerification
    {
        public string MobileNumber { get; set; }
        public string DeviceToken { get; set; }


    }

    public class ProductFilter
    {
        public int? CropId { get; set; }
        public Csvfile csvfile { get; set; }
        public int counter { get; set; }
        public string culture { get; set; }
        public string SCategoryId { get; set; }
        public string MobileNumber { get; set; }
        public bool ProductPagination { get; set; }
        public int? VarietyId { get; set;}
        public string State { get; set; }
        public string District { get; set; }
        public bool FilterByCropId { get; set; }
        public string Taluka { get; set; }

        //public string AvailabilityDate { get; set; }
        //public DateTime? AvailabilityDate { get; set; }

        public string MaxPrice { get; set; }
        public string MinPrice { get; set; }
        public string Quantity { get; set; }
        public string IsQualityTestNeeded { get; set; }
        public string SellerType { get; set; }
        public string IsFilterApplied { get; set; }
        public string IsAllActiveProducts { get; set; }

        public string SortProduct { get; set; }
        public string RecentProduct { get; set; }
        public string OldProduct { get; set; }
        public string RecentAvailability { get; set; }
        public string OldAvailability { get; set; }




        //public string GeoAddress { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }



    }
    public class UserRequirementViewModel
    {
        public int Tr_Id { get; set; }
        public string Usercode { get; set; }
        public string Requirement { get; set; }
        public string Tr_Date { get; set; }
    }


    public class UserEnquiryViewModel
    {
        public int Tr_Id { get; set; }
        public int ProductId { get; set; }
        public string Enquiry { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string Tr_Date { get; set; }
        //public string ProductId { get; set; }
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
    }

    public class Mandi_UserFeedbackViewModel
    {
        public int FeedbackId { get; set; }
        public string MobileNumber { get; set; }
        public string VarietyName { get; set; } 
        public string Feedback { get; set; }
        public string Tr_Date { get; set; }
    }


    public class MandiOrderHistoryViewModel
    {
        // from OrderDetails
        public string Order_Id { get; set; }
        public Nullable<decimal> ServiceTax { get; set; }
        public string TotalAmount { get; set; }
        public string Buyer_Mobile { get; set; }
        public string TotalPrice { get; set; }
        public string Payment_Mode { get; set; }
        public string QuantityUnit { get; set; }
        public string OrderDate { get; set; }
        public string Shipping_Address_Id { get; set; }
        public string Order_Status { get; set; }
        public string TermsAndCondition { get; set; }
        public string SelectedTotalQty { get; set; }
        public string Buyer_Name { get; set; }
        public string GeoCoordinates { get; set; }
        public Nullable<decimal> Logistics_Cost { get; set; }
        public Nullable<decimal> Other_Charges { get; set; }
        public Nullable<decimal> Taxes { get; set; }


        //from UsersAddress
        public string reciver_name { get; set; }
        public string ship_address { get; set; }
        public string city { get; set; }
        public string ProductImageUrl { get; set; }
        public string pincode { get; set; }
        public string ship_mobile { get; set; }
        public string SellerName { get; set; }
        public string SellerState { get; set; }
        public string SellerDistrict { get; set; }
        public string SelletTaluka { get; set; }
        public string SellerVillage { get; set; }
        public string SellerPinCode { get; set; }

        //from OrderProductDetails
        public int Product_Id { get; set; }
        public string TotalQuantity { get; set; }
        public int Price { get; set; }
        public string SelectedQuantity { get; set; }
        public int ? SelectedProductPrice { get; set; }

        public string CropName { get; set; }
        public string VarietyName { get; set; }
        
        

    }

    public class MandiOrderList
    {
        public List<MandiOrderHistoryViewModel> MandiOrders { get; set; }
    }


    public class SellersDashboardProduct
    {
        public int Tr_Id { get; set; }
        public int? CropId { get; set; }
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
        public Nullable<System.DateTime> AvailabilityDate { get; set; }
        public string PaymentMethod { get; set; }
        public Nullable<bool> IsQualityTestNeeded { get; set; }
        public Nullable<bool> IsLogisticNeeded { get; set; }
        public string ProductImageUrl { get; set; }
        public string SecondaryProductImage { get; set; }

        public Nullable<System.DateTime> Tr_Date { get; set; }
        public string StateCode { get; set; }
        public string DistrictCode { get; set; }
        public string TalukaCode { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string ProductPriority { get; set; }
        public string ProductDescription { get; set; }



    }

    public class SellersDashboardProductList
    {
        public List<SellersDashboardProduct> Products { get; set; }
    }
}