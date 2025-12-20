using Domain.Entities.Requests.Clients;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace Application.Utils
{
    public static class DataGridUtil
    {
        public class SortDescriptor
        {
            public string selector { get; set; }
            public bool desc { get; set; }
        }

        public static object Apply<T>(IEnumerable<T> source, DevExtremeFilterParams filterParams)
        {
            var query = source.AsQueryable();

            // Filtering (basic contains search)
            if (!string.IsNullOrEmpty(filterParams.filter))
            {
                var filterValue = filterParams.filter.ToLower();
                var stringProps = typeof(T).GetProperties()
                    .Where(p => p.PropertyType == typeof(string))
                    .ToList();

                query = query.Where(item =>
                    stringProps.Any(prop =>
                        ((string)prop.GetValue(item) ?? string.Empty)
                            .ToLower()
                            .Contains(filterValue)
                    )
                ).AsQueryable();
            }

            // Sorting
            if (!string.IsNullOrEmpty(filterParams.sort))
            {
                var sortInfo = JsonSerializer.Deserialize<List<SortDescriptor>>(filterParams.sort);
                if (sortInfo?.Any() == true)
                {
                    var sortExpr = string.Join(", ",
                        sortInfo.Select(s => $"{s.selector} {(s.desc ? "descending" : "ascending")}")
                    );
                    query = query.OrderBy(sortExpr);
                }
            }

            // Total count before paging
            var totalCount = query.Count();

            // Paging
            if (filterParams.skip.HasValue) query = query.Skip(filterParams.skip.Value);
            if (filterParams.take.HasValue) query = query.Take(filterParams.take.Value);

            return new
            {
                data = query.ToList(),
                totalCount = filterParams.requireTotalCount == true ? totalCount : (int?)null
            };
        }
    }
}
