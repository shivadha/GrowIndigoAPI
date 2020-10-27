using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrowIndigo.Models
{
    public class CheckOutNotificationViewModel
    {
        public string DeviceToken { get; set; }
        public string CropName { get; set; }
        public string Type { get; set; }
    }
}