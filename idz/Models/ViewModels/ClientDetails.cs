using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using idz.Models.Entities;


namespace idz.Models.ViewModels
{
    public class ClientDetails
    {
        public Clients Client { get; set; }
        public List<long> RoomNumbers { get; set; }
        public DateTime Date_Start { get; set; }
        public DateTime Date_End { get; set; }
        public List<PetInfo> Pets { get; set; }

        public List<RoomBookingInfo> RoomBookings { get; set; }

    }
}