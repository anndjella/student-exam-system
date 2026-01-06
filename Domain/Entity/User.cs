using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Enums;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public sealed class User:IEntity
    {
        public int ID { get; set; }
        public UserRole Role { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool MustChangePassword { get; set; }
        public int PersonID { get; set; }
        public Person Person { get; private set; } = null!;
        public bool isActive { get; private set; } = true;
        public void Deactivate()
        {
            isActive=false;
        }
        private User() { }

        public User( UserRole role, string username, string passwordHash)
        {
            //PersonID = personId;
            Username = username;
            PasswordHash = passwordHash;
            MustChangePassword = true;
            Role = role;
        }
        public void SetPasswordHash(string newHash)
        {
            PasswordHash = newHash;
        }

        public void MarkPasswordChanged()
        {
            MustChangePassword = false;
        }
    }
}
