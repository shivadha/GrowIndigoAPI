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

        public class MandiCrop
        {
            public int CropId { get; set; }
            public string CropName { get; set; }
            public string CategoryName { get; set; }
            public string CategoryId { get; set; }
        }
        public class Mandi_CropMaster
        {
            public List<MandiCrop> MandiCrops { get; set; }
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