using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class MasterData
    {
        public class State
        {
            public string StateCode { get; set; }
            public string StateName { get; set; }
        }

        public class Respose
        {
            public string Status { get; set; }
            public string MobileNumber { get; set; }
        }
        public class StateMaster
        {
            public List<State> States { get; set; }
        }
        public class ResponceMaster
        {
            public List<Respose> Resposes { get; set; }
        }

        public class District
        {
            public string DistrictCode { get; set; }
            public string DistrictName { get; set; }
        }


        public class DistrictMaster
        {
            public List<District> District { get; set; }
        }

        public class Taluka
        {
            public string TalukaCode { get; set; }
            public string TalukaName { get; set; }
        }

        public class TalukaMaster
        {
            public List<Taluka> Taluka { get; set; }
        }

        public class Product
        {
            public int SkuId { get; set; }
            public string SkuName { get; set; }
            public string ItemCode { get; set; }
            public string Description { get; set; }
            public string Price { get; set; }
            public string CurrentQuantity { get; set; }
            public string SkuCreatedDate { get; set; }
            public string GSTPercent { get; set; }
            public string ImageUrl { get; set; }
            public string MinQuantittyToBook { get; set; }
            public string MaxQuantittyToBook { get; set; }
            public string PrdCompanyName { get; set; }
            public int? AllowSAPOrder { get; set; }
            public string PrdState { get; set; }
            public string Status { get; set; }
        }

        public class ProductMaster
        {
            public List<Product> Products { get; set; }
        }


        public class CategoryViewModel
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
            public int RoleId { get; set; }
            public List<SubCategoryViewModel> SubCategories { get; set; }
        }

        public class SubCategoryViewModel
        {
            public int SubCategoryId { get; set; }
            public string SubCategoryName { get; set; }
            public string ImageUrl { get; set; }
            public int CategoryId { get; set; }
        }
    }
}