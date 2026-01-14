using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Registrations
{
    public sealed class CreateRegistrationRequest
    {
        public int SubjectID { get; set; }  
        public int TermID {  get; set; }
    }
}
