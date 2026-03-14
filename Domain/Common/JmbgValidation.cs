using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public static class JmbgValidation
    {
        public static bool IsAllDigits(string s) => s.All(char.IsDigit) && s.Length == 13;

        public static bool RegionLooksSerbian(string jmbg)
        {
            int rr = int.Parse(jmbg.Substring(7, 2));
            return (rr >= 70 && rr <= 99);
        }
    }
}
