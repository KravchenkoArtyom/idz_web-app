using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using idz.Models.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace idz.Models.ViewModels
{
    public class CreatePetVM
    {

        public Guid Pet_ID { get; set; }


        [Required]
        [DisplayName("Имя")]
        [StringLength(30, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [DisplayName("Тип животного")]
        [StringLength(30, MinimumLength = 2)]
        public string Type { get; set; }

        [Required]
        [DisplayName("Возраст")]
        [Range(1, 20)]
        public long? Age { get; set; } 

        [DisplayName("Дополнительная информация")]
        [StringLength(100)] 
        public string Additional_Info { get; set; }

        [Required]
        [DisplayName("Хозяин/Клиент")]
        public Guid Client_ID { get; set; }

        // Список клиентов для выпадающего списка
        public IEnumerable<SelectListItem> ClientsList { get; set; }
    }
}

