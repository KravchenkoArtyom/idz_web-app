using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace idz.Models.ViewModels
{
    public class PetDetails
    {
        public string PetName { get; set; }
        public string PetType { get; set; }

        public List<CareService> CareServices { get; set; }



    }
}