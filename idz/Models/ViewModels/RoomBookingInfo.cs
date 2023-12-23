using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace idz.Models.ViewModels
{
    public class RoomBookingInfo
    {
        public long RoomNumber { get; set; }
        public DateTime? BookingStartDate { get; set; }
        public DateTime? BookingEndDate { get; set; }
    }
}