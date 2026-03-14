using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Application.Common;

namespace Application.DTO.Students
{
    public sealed class CreateStudentRequest: IPersonCreate
    {
        public string JMBG { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string IndexNumber { get; set; } = "" ;
    }
    }


