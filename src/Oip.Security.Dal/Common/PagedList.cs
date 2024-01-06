using System.Collections.Generic;

namespace Oip.Security.Dal.Common;

public class PagedList<T> where T : class
{
    public List<T> Data { get; } = new();

    public int TotalCount { get; set; }

    public int PageSize { get; set; }
}