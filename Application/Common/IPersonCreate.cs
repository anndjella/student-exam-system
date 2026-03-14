using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public interface IPersonCreate
    {
        string FirstName { get; }
        string LastName { get; }
        string JMBG { get; }
    }
}
