using Application.Common;
using Application.DTO.Students;
using Domain.Entity;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.DTO.Teachers
{
    public class CreateTeacherRequest : IPersonCreate
    {
        public string JMBG { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string EmployeeNumber { get; set; } = "";
        public Title Title { get; set; }
    }
}
