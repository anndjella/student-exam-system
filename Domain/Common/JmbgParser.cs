using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public static class JmbgParser
    {
        public static DateOnly GetDateOfBirth(string jmbg)
        {
            jmbg = jmbg.Trim();
            int day = int.Parse(jmbg.Substring(0, 2));
            int month = int.Parse(jmbg.Substring(2, 2));
            int yyy = int.Parse(jmbg.Substring(4, 3));

            int year = yyy >= 900 ? 1000 + yyy : 2000 + yyy;

            return new DateOnly(year, month, day);
        }
    }

}
