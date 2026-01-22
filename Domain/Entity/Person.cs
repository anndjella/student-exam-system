using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public abstract class Person : IEntity
    {
        public int ID { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string JMBG { get; set; } = null!;
        public DateOnly DateOfBirth { get; set; }
        public User? User { get;  set; } = null!;
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public void MarkDeleted()
        {
            if (IsDeleted) return;

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }
}

