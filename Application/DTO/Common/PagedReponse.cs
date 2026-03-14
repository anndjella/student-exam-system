using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Common
{
    public sealed class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
    }
}
