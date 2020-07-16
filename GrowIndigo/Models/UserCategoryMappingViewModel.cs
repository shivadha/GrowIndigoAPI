using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class UserCategoryMappingViewModel
    {
        public long Id { get; set; }
        public string Fk_MobileNumber { get; set; }
        public long ? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Csvfile csvfile { get; set; }
    }

    public class UserCategoryList
    {
        //public IQueryable<ProductMasterViewModel> Products { get; set; }
        public List<UserCategoryMappingViewModel> UserCategory { get; set; }

    }
}