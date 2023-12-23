using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace idz.Models.ViewModels
{
    public class CareService
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public long Price { get; set; }

        public string EmployeeName { get; set; }
        public string EmployeeSurname { get; set; }
        public string EmployeePatronymyc { get; set; }
        public long EmployeeNumber { get; set; }

    }
}