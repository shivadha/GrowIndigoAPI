using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class Mandi_OrderViewModel
    {
        public class OrderBookingViewModel
        {
            public int  Order_Id { get; set; }
            //public string Retailer_Id { get; set; }
            public string Buyer_Mobile { get; set; }
            public string Totalprice { get; set; }
            public string Prices { get; set; }
  
            public string PaymentStatus { get; set; }

            public string Type { get; set; }
            public string Payment_Mode { get; set; }
            public string ShippingAddress { get; set; }
            public string OrderDate { get; set; }
            public Nullable<int> Shipping_Address_Id { get; set; }
            public string Order_Status { get; set; }
            public string rzp_order_id { get; set; }
            public string rzp_payment_status { get; set; }
            public string rzp_payment_signature { get; set; }
            public string CropName { get; set; }
            public string ProductId { get; set; }
           
            public int Product_Id { get; set; }
            
            public string PickupAddress { get; set; }


            public OrderProductDetail orderProductDetail { get; set; }

            public Csvfile csvfile { get; set; }
            public string Qty { get; set; }
            public string SAP_Order_ID { get; set; }
            public string GeoCoordinates { get; set; }
            public string SelectedTotalQty { get; set; }
            public string Buyer_Name { get; set; }
            public Nullable<decimal> ServiceTax { get; set; }
            public string TotalAmount { get; set; }
            public string TransactionId { get; set; }
            public string ResponseCode { get; set; }
            public string TransactionStatus { get; set; }

        }

        public class Csvfile
        {
            public List<Table1> Table1 { get; set; }
        }

        public class OrderProductDetail
        {
            public List<OnOrderProductDetail> OnOrderProductDetail { get; set; }
        }
        public class OnOrderProductDetail
        {
            public string TotalQuantity { get; set; }
            public int Order_Id { get; set; }
            public int Product_Id { get; set; }
            public string CropName { get; set; }
            public string PickUpAddress { get; set; }

            public int Price { get; set; }
            public string SelectedQuantity { get; set; }

            public string SelectedProductPrice { get; set; }
        }
        public class Table1
        {
            public int Order_Id { get; set; }
            public int Product_Id { get; set; }
            public string PickupAddress { get; set; }
            public string ProductId { get; set; }

            public string CropName  { get; set; }
            public string TotalQuantity { get; set; }
            public string BuyerName { get; set; }
            public string BuyerContact { get; set; }
            public string PriceS { get; set; }
            public string ServiceTax { get; set; }
            public string TotalAmount { get; set; }
            public string PaymentStatus { get; set; }
            public int  Price { get; set; }
            public string SelectedQuantity { get; set; }

            public string SelectedProductPrice { get; set; }

        }

        public class NotificationViewModel
        {
            public int SerialNumber { get; set; }
            public string MobileNumber { get; set; }
            public string Message { get; set; }
            public Nullable<System.DateTime> Tr_Date { get; set; }
        }

        public class NotificationList
        {
            public List<NotificationViewModel> Notifications { get; set; }
        }


    }
}