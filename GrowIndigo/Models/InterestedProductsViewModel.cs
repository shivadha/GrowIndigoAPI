using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class InterestedProductsViewModel
    {

        public long Id { get; set; }
        public string Fk_MobileNumber { get; set; }
        public string Subject { get; set; }
        public int Tr_Id { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }

    }
}