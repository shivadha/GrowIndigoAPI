using GrowIndigo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class MandiMasterData
    {
        public class MandiRoles
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; }
        }
        public class Role_Master
        {
            public List<MandiRoles> Roles { get; set; }
        }

        public class MandiRolesForUser
        {
            
                   public string RoleName { get; set; }
        }

        public class Cart_InfoForUser
        {
            public long CartId { get; set; }
            public string CartType { get; set; }
            public Nullable<int> Fk_EnquiryId { get; set; }
            public Nullable<long> Fk_InterestedProductId { get; set; }
            public string Deal_Id { get; set; }
            public string Product { get; set; }
            public string Quantity { get; set; }
            public string QuantityUnit { get; set; }
            public Nullable<int> Price { get; set; }
            public Nullable<decimal> Taxes { get; set; }
          
         
            public string ProductAddress { get; set; }
            public string ProductImage { get; set; }
            public Nullable<long> Product_Id { get; set; }
            public long ProducStatusId { get; set; }
            public string ProductStatus { get; set; }
            public string BuyerName { get; set; }
            public string BuyerNumber { get; set; }
            public string BuyerAddress { get; set; }
            public string SellerName { get; set; }
            public string TermsAndCondition { get; set; }
            public string Seller_MobileNumber { get; set; }
            public string SellerAddress { get; set; }
            public Nullable<decimal> Logistics_Cost { get; set; }
            public Nullable<decimal> ServiceTax { get; set; }
            public Nullable<decimal> Other_Charges { get; set; }
            public Nullable<bool> Status { get; set; }
        }
        public class CartInfoViewModel
        {
            public string Deal_Id { get; set; }
            public int CartId { get; set; }
            public int CropId { get; set; }
            public string CropName { get; set; }
        }

        public class MandiBanners
        {
            public long Id { get; set; }
            public string BannerTitle { get; set; }
            public string Description { get; set; }
            public string BannerImage { get; set; }
            public Nullable<bool> IsDefault { get; set; }
            public Nullable<bool> IsActive { get; set; }
            public string ImageType { get; set; }
            public Nullable<System.DateTime> CreatedDate { get; set; }
            public Nullable<System.DateTime> ModifiedDate { get; set; }
        }
            public class MandiCrop
        {
            public int CropId { get; set; }
            public string CropName { get; set; }
            public string CropStatus { get; set; }
            public string CropImage { get; set; }
            public string CategoryName { get; set; }
            public string Hi_CropName { get; set; }
            public string Mr_CropName { get; set; }
            public string Te_CropName { get; set; }
          
            public Nullable<bool> IsActive { get; set; }
            public Nullable<bool> IsApproved { get; set; }
      
            public Nullable<System.DateTime> CropEndDate { get; set; }

            public string EnCategoryName { get; set; }
            public string EnCropName { get; set; }
            public long ? CategoryId { get; set; }

       
            public string CategoryImage { get; set; }

        }
        public class Mandi_CropMaster
        {
            public List<MandiCrop> MandiCrops { get; set; }
        }

        public class Mandi_BannerViewModel
        {
            public List<MandiBanners> Mandi_Banner { get; set; }
        }

        public class Mandi_CartInfoViewModel
        {
            public List<Cart_InfoForUser> MandiCart_Info { get; set; }
        }

        public class MandiUserRoles
        {
            public List<MandiRoles> MandiRoles { get; set; }
        }
        public class MandiVariety
        {
            public int VarietyId { get; set; }
            public string VarietyName { get; set; }
        }
        public class Mandi_VarietyMaster
        {
            public List<MandiVariety> MandiVarieties { get; set; }
        }


    }
}