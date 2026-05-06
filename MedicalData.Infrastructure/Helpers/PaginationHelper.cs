using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.Helpers
{
    public class PaginationHelper
    {
        public static PagedResult<T> BuildPagedResult<T>(List<T> result,int pageSize,Func<T,int> getId)
        {
            var items = result.Take(pageSize).ToList();
            var hasNext = result.Count > pageSize;
            var nextCursor = hasNext && items.Any() ? getId(items.Last()) : (int?)null;
            return new PagedResult<T>
            {
                Items = items,
                NextCursor = nextCursor,
                HasNext = hasNext
            };
        }
    }
}
