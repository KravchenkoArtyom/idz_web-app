using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace idz.Models.ViewModels
{
    public class CreateCareLogVM
    {


        public Guid careLogID { get; set; }

        [Required]
        [DisplayName("Дата")]
        public DateTime Time { get; set; }

        [Required]
        [DisplayName("Питомец")]
        public Guid Pet_ID { get; set; }
        
        [Required]
        [DisplayName("Работник")]

        public Guid Employee_ID { get; set; }
       
        [Required]
        [DisplayName("Услуга")]

        public Guid CareType_ID { get; set; }
        
        public IEnumerable<SelectListItem> CareTypesList { get; set; }
        public IEnumerable<SelectListItem> EmployeesList { get; set; }
        public IEnumerable<SelectListItem> PetsList { get; set; }


    }
}