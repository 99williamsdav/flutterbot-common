using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Common.Interfaces
{
    public interface IPersistQueryable<T> : IPersist<T>
    {
        IQueryable<T> GetQueryable();

        List<T> GetQueryable(IEnumerable<KeyValuePair<string, object>> filters,
             IEnumerable<string> javaScriptFilters,
             IList<string> excludeFields = null);
    }
}
