using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace idz.Models.ViewModels
{
    public class PetInfo
    {

        public DateTime? BookingStartDate { get; set; }
        public DateTime? BookingEndDate { get; set; }

        public string PetName { get; set; }
        public string PetAdditionalInfo { get; set; }

        public Guid PetID { get; set; }
    }
}