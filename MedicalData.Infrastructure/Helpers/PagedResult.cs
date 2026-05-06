using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.Helpers
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int? NextCursor { get; set; } = 0;

        public bool HasNext { get; set; }
    }
}
