using Application.Common;
using Domain.Entity;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Teachers
{
    public sealed class UpdateTeacherRequest : IPersonUpdate
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Title? Title { get;set; }
    }
}
