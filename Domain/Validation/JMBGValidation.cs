using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Validation
{
    public static class JMBGValidation
    {
        public static bool IsAllDigits(string s) => s.All(char.IsDigit) && s.Length == 13;

        public static bool TryParseDate(string jmbg, out DateOnly dob)
        {
            int dd = int.Parse(jmbg[..2]);
            int mm = int.Parse(jmbg.Substring(2, 2));
            int yyy = int.Parse(jmbg.Substring(4, 3)); 


            int year = (yyy >= 800) ? 1000 + yyy : 2000 + yyy;
            int currentYear = DateTime.UtcNow.Year;
            if (year < 1900 || year > currentYear) { dob = default; return false; }

            try
            {
                dob = new DateOnly(year, mm, dd);
                return true;
            }
            catch
            {
                dob = default;
                return false;
            }
        }

        public static bool ChecksumValid(string jmbg)
        {
            var d = jmbg.Select(c => c - '0').ToArray();
            int m = 11 - ((7 * (d[0] + d[6]) + 6 * (d[1] + d[7]) + 5 * (d[2] + d[8]) +
                           4 * (d[3] + d[9]) + 3 * (d[4] + d[10]) + 2 * (d[5] + d[11])) % 11);
            int k = (m >= 1 && m <= 9) ? m : 0;
            return d[12] == k;
        }

        public static bool RegionLooksSerbian(string jmbg)
        {
            int rr = int.Parse(jmbg.Substring(7, 2));
            return (rr >= 70 && rr <= 99);
        }
    }
}
