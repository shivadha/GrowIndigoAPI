using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class UsersAddressViewModel
    {
        public class Address
        {
            public int tr_id { get; set; }
            public string retailer_id { get; set; }
            public string retailer_mobile { get; set; }
            public string reciver_name { get; set; }
            public string ship_address { get; set; }
            public string city { get; set; }
            public string pincode { get; set; }
            public string ship_mobile { get; set; }
            public string email_id { get; set; }
            public string PanNumber { get; set; }
            public string tr_date { get; set; }
            public Nullable<bool> IsPermanentAddress { get; set; }
            public Nullable<bool> IsMandiUser { get; set; }

          




        }

        public class AddressMaster
        {
            public List<Address> Addresses { get; set; }
        }

    }
}