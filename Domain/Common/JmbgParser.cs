using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public static class JmbgParser
    {
        public static bool TryGetDateOfBirth(
       string? jmbg,
       out DateOnly dateOfBirth,
       out string error)
        {
            dateOfBirth = default;
            error = "";

            if (string.IsNullOrWhiteSpace(jmbg))
            {
                error = "JMBG is required.";
                return false;
            }

            jmbg = jmbg.Trim();

            if (jmbg.Length != 13 || !jmbg.All(char.IsDigit))
            {
                error = "JMBG must contain exactly 13 digits.";
                return false;
            }

            int day = int.Parse(jmbg.Substring(0, 2));
            int month = int.Parse(jmbg.Substring(2, 2));
            int yyy = int.Parse(jmbg.Substring(4, 3));

            int year = yyy >= 900 ? 1000 + yyy : 2000 + yyy;

            if (month < 1 || month > 12)
            {
                error = "Invalid month in JMBG.";
                return false;
            }

            int maxDay = DateTime.DaysInMonth(year, month);
            if (day < 1 || day > maxDay)
            {
                error = "Invalid day in JMBG.";
                return false;
            }

            dateOfBirth = new DateOnly(year, month, day);
            return true;
        }

        public static DateOnly GetDateOfBirth(string jmbg)
        {
            if (!TryGetDateOfBirth(jmbg, out var dob, out var error))
                throw new ArgumentException(error);

            return dob;
        }
    }

}
