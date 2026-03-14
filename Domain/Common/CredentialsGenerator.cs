using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public static class CredentialsGenerator
    {
        public static string StudentUsername(string firstName, string lastName, string indexNumber)
            => $"{Initials(firstName)}{Initials(lastName)}{Normalize(indexNumber)}".ToLowerInvariant();

        public static string TeacherUsername(string firstName, string lastName, string employeeNumber)
            => $"{Initials(firstName)}{Initials(lastName)}{Normalize(employeeNumber)}".ToLowerInvariant();

        public static string InitialPasswordPlain(string jmbg)
            => $"PasS!{jmbg}";

        private static string Initials(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            var t = s.Trim();
            return  t.Substring(0,1);
        }

        private static string Normalize(string s)
            => string.IsNullOrWhiteSpace(s) ? "" : s.Trim().Replace("/", "");
    }
}
