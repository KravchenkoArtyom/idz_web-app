//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace idz.Models.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Bookings
    {
        public System.Guid Booking_ID { get; set; }
        public System.DateTime Date_Start { get; set; }
        public System.DateTime Date_End { get; set; }
        public System.Guid Pet_ID { get; set; }
        public System.Guid Room_ID { get; set; }
    
        public virtual Pets Pets { get; set; }
        public virtual Rooms Rooms { get; set; }
    }
}
