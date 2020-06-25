using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class UserInfoViewModel
    {
        public string Type { get; set; }
        public string MobileNumber { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public string Taluka { get; set; }
        public string Village { get; set; }
        public string Firm_Name { get; set; }
        public Nullable<bool> IsProfileUpdated { get; set; }
        public string RetailerId { get; set; }

    }
    public class UserMobileNumber
    {
        public string MobileNumber { get; set; }
    }

    public class OtpVerification
    {
        public string MobileNumber { get; set; }
        public string Otp { get; set; }
        public string UserId { get; set; }

    }

    public class UserVerification
    {
        public string MobileNumber { get; set; }

    }

    public class UserProfile
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string BirthDate { get; set; }
        public string MobileNumber { get; set; }
        public string Address { get; set; }
        public string StreetLine1 { get; set; }
        public string StreetLine2 { get; set; }
        public string Pincode { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public string Taluka { get; set; }
        public string Town { get; set; }
        public string FirmName { get; set; }
        public string GstNumber { get; set; }
        public string PanNumber { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseValidity { get; set; }
        public bool IsProfileUpdate { get; set; }

    }

    public class UserMaster
    {
        public UserProfile User { get; set; }
    }

    public class SuccessResponse
    {
        public string Message { get; set; }
        public string InnerException { get; set; }

    }

    public class OrderResponse
    {
      public string DAP { get; set; }
        public string OrderId { get; set; }
        public string RazorPayOrderId { get; set; }
        public string Message { get; set; }
        //public string ServiceTax { get; set; }
        //public string TotalAmount { get; set; }
        public int UPI { get; set; }
    }
   

    public class UserRole
    {
        public int RoleId { get; set; }
        public int RoleName { get; set; }
    }
    public class UserFeedbackViewModel
    {
        public int Tr_Id { get; set; }
        public string Usercode { get; set; }
        public string Product { get; set; }
        public string Feedback { get; set; }
        public string Tr_Date { get; set; }
    }
    public class OrderDetailsViewModel
    {
        public string Order_Id { get; set; }
        public string WalletCurrentBalance { get; set; }
        public string AmountToPayOnline { get; set; }
        
        public string Retailer_Id { get; set; }
        public string Retailer_Mobile { get; set; }
        public string Totalprice { get; set; }
        public string Payment_Mode { get; set; }
        public string OrderDate { get; set; }
        public Nullable<int> Shipping_Address_Id { get; set; }
        public string Order_Status { get; set; }
        public Csvfile csvfile { get; set; }
        public string Qty { get; set; }
        public string SAP_Order_ID { get; set; }
        public string GeoCoordinates { get; set; }
        public string Rzp_Order_Id { get; set; }
        public string Rzp_Payment_Id { get; set; }
        public string Rzp_Signature { get; set; }
        public string Rzp_Payment_Status { get; set; }
        public string Rzp_payment_message { get; set; }

    }

    public class Csvfile
    {
        public List<Table1> Table1 { get; set; }
    }

    public class Table1
    {
        public string OrderQuantity { get; set; }
        public string product_Id { get; set; }
        public string price { get; set; }

   
        public long Id { get; set; }
        public string Fk_MobileNumber { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }




    }


    public class OrderList
    {
        public List<OrderHistoryViewModel> Orders { get; set; }
    }


    public class TransactionResponse
    {
        public string Order_Id { get; set; }

        public string Rzp_Order_Id { get; set; }
        public string Rzp_Payment_Id { get; set; }
        public string Rzp_Signature { get; set; }
        public string Rzp_Payment_Status { get; set; }
        public string Rzp_payment_message { get; set; }
        public string Totalprice { get; set; }

    }

    public class OrderHistoryViewModel
    {
        // from OrderDetails
        public string Order_Id { get; set; }
        public string Retailer_Id { get; set; }
        public string Retailer_Mobile { get; set; }
        public string Totalprice { get; set; }
        public string Payment_Mode { get; set; }
        public string OrderDate { get; set; }
        public string Shipping_Address_Id { get; set; }
        public string Order_Status { get; set; }

        //public Csvfile csvfile { get; set; }
       
        //from UsersAddress
        public string reciver_name { get; set; }
        public string ship_address { get; set; }
        public string city { get; set; }
        public string pincode { get; set; }
        public string ship_mobile { get; set; }

        //from OrderProductDetails
        public string Qty { get; set; }
        public string ProductName { get; set; }


    }


}