using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace idz.Models.ViewModels
{
    public class CreateClientVM
    {
        public Guid Client_ID { get; set; }
        [Required]
        [DisplayName("Имя")]
        [MinLength(2)]
        public string Name { get; set; }
        [Required]
        [DisplayName("Фамилия")]
        [MinLength(3)]
        public string Surname { get; set; }
        [DisplayName("Отчество")]
        public string Patronymic { get; set; }
        [Required]
        [DisplayName("Номер")]
        [Range(10, long.MaxValue, ErrorMessage = "Номер должен содержать как минимум 10 цифр")]
        public long Number { get; set; }
        [DisplayName("Email")]
        public string Email { get; set; }



    }
}