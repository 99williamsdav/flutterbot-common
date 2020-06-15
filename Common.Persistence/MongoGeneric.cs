namespace Common.Persistence
{
    using Common.Interfaces;
    using Common.ModelInterfaces;
    using Common.Models;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading.Tasks;

    public class MongoGeneric<T> : IPersist<T>, IPersistQueryable<T> where T : class, IIdentifier
    {
        private readonly IMongoClient _client;

        private readonly string _collectionName;

        private readonly string _dbName;

        public MongoGeneric(IMongoClient client, string dbName, string collectionName, IIdGenerator idGenerator = null)
        {
            this._client = client;
            this._dbName = dbName;
            this._collectionName = collectionName;
            MongoInit.Initialize<T>(idGenerator);
        }

        public MongoGeneric(string connectionString, string dbName, string collectionName) : 
            this(new MongoClient(connectionString), dbName, collectionName)
        {
        }

        public List<T> GetAll()
        {
            return this.GetAllAsync().Result;
        }

        public async Task<List<T>> GetAllAsync()
        {
            var database = this._client.GetDatabase(this._dbName);
            var col = database.GetCollection<T>(this._collectionName);
            return await col.FindSync(FilterDefinition<T>.Empty).ToListAsync();
        }

        public List<T> Find(
            IEnumerable<KeyValuePair<string, object>> filters,
            IList<string> excludeFields = null,
            string sortBy = null,
            int? limit = null)
        {
            return this.FindAsync(filters, excludeFields, sortBy, limit).Result;
        }

        public async Task<List<T>> FindAsync(
            IEnumerable<KeyValuePair<string, object>> filters, 
            IList<string> excludeFields = null, 
            string sortBy = null, 
            int? limit = null)
        {
            if (!filters.Any())
            {
                return await this.GetAllAsync();
            }

            var database = this._client.GetDatabase(this._dbName);
            var col = database.GetCollection<T>(this._collectionName);

            var qry = ConstructMongoQuery(filters);
            var cursor = col.Find(qry);
            if (excludeFields != null)
            {
                cursor = cursor.Project<T>(ProjectExclude(excludeFields));
            }

            // TODO test
            if (sortBy != null)
            {
                var sort = Builders<T>.Sort.Ascending(sortBy);
                cursor = cursor.Sort(sort);
            }

            if (limit != null)
            {
                cursor.Limit(limit);
            }

            return await cursor.ToListAsync();
        }

        private static string ProjectExclude(IList<string> fields)
        {
            var ret = fields.ToDictionary(f => f, _ => 0);
            return JsonConvert.SerializeObject(ret);
        }

        public long Remove(IEnumerable<KeyValuePair<string, object>> filters)
        {
            return this.RemoveAsync(filters).Result;
        }

        public async Task<long> RemoveAsync(IEnumerable<KeyValuePair<string, object>> filters)
        {
            var database = this._client.GetDatabase(this._dbName);
            var col = database.GetCollection<T>(this._collectionName);

            var qry = ConstructMongoQuery(filters);
            var result = await col.DeleteManyAsync(qry);
            return result.DeletedCount;
        }

        public void Save(T value)
        {
            this.SaveAsync(value).Wait();
        }

        public async Task SaveAsync(T value)
        {
            var database = this._client.GetDatabase(this._dbName);
            var col = database.GetCollection<T>(this._collectionName);
            try
            {
                if (string.IsNullOrEmpty(value.Id))
                {
                    await col.InsertOneAsync(value);
                }
                else
                {
                    await col.ReplaceOneAsync(
                        new BsonDocument("_id", value.Id),
                        options: new UpdateOptions { IsUpsert = true },
                        replacement: value);
                }
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                if (e.InnerException.Message.Contains("duplicate key"))
                {
                    throw new Exception("Duplicate Record.");
                }
            }
        }

        public IQueryable<T> GetQueryable()
        {
            var database = this._client.GetDatabase(this._dbName);
            var col = database.GetCollection<T>(this._collectionName);

            return col.AsQueryable();
        }

        public List<T> GetQueryable(IEnumerable<KeyValuePair<string, object>> filters,
            IEnumerable<string> javaScriptFilters,
            IList<string> excludeFields = null)
        {
            var database = this._client.GetDatabase(this._dbName);
            var col = database.GetCollection<T>(this._collectionName);

            var qry = ConstructMongoQuery(filters, javaScriptFilters);
            var cursor = col.Find(qry);

            var exclJson = excludeFields == null ? null : ProjectExclude(excludeFields);
            //var x = cursor.ToString();
            //var cursor = col.AsQueryable();
            //return cursor.Where(d => qry.Inject());
            return excludeFields == null
                ? cursor.ToList()
                : cursor.Project<T>(exclJson).ToList();
        }

        public List<T> Find(string jsonText, Dictionary<string, int> sortDic = null)
        {
            return this.FindAsync(jsonText, sortDic).Result;
        }

        public async Task<List<T>> FindAsync(string jsonText, Dictionary<string, int> sortDic = null)
        {
            var database = this._client.GetDatabase(this._dbName);
            var col = database.GetCollection<T>(this._collectionName);

            BsonDocument query = BsonSerializer.Deserialize<BsonDocument>(jsonText);
            var sortQuery = ConstructSortQuery(sortDic);

            var filter = Builders<T>.Filter.JsonSchema(query);

            return await col.Find(filter).Sort(sortQuery).ToListAsync();
        }

        private static SortDefinition<T> ConstructSortQuery(Dictionary<string, int> sortDic)
        {
            if (sortDic != null)
            {
                var sortDefinitions = sortDic.Select(kv =>
                {
                    switch (kv.Value)
                    {
                        case 1:
                            return Builders<T>.Sort.Ascending(kv.Key);
                        case -1:
                            return Builders<T>.Sort.Descending(kv.Key);
                        default:
                            throw new IndexOutOfRangeException();
                    }
                });
                return Builders<T>.Sort.Combine(sortDefinitions);
            }
            return null;
        }

        private static FilterDefinition<T> ConstructMongoQuery(string key, object value)
        {
            if (value == null)
            {
                return Builders<T>.Filter.Type(key, BsonType.Null);
            }

            switch (value)
            {
                case DateTime filterDate:
                    return Builders<T>.Filter.And(
                        Builders<T>.Filter.Gte(key, new BsonDateTime(filterDate)),
                        Builders<T>.Filter.Lt(key, new BsonDateTime(filterDate.AddDays(1))));
                case Range<DateTime> dtRange:
                    return ConvertDateRange(key, dtRange);
                case Range<double> dbRange:
                    if (dbRange.Minimum == null && dbRange.Maximum == null) return null;

                    return dbRange.Minimum == null
                        ? Builders<T>.Filter.Lte(key, new BsonDouble(dbRange.Maximum.Value))
                        : dbRange.Maximum == null
                            ? Builders<T>.Filter.Gte(key, new BsonDouble(dbRange.Minimum.Value))
                            : Builders<T>.Filter.And(
                                Builders<T>.Filter.Gte(key, new BsonDouble(dbRange.Minimum.Value)),
                                Builders<T>.Filter.Lte(key, new BsonDouble(dbRange.Maximum.Value)));
                case NotNull _:
                    return Builders<T>.Filter.Not(Builders<T>.Filter.Type(key, BsonType.Null));
                case IEnumerable t:
                {
                    if (t is JArray)
                    {
                        t = t.Cast<JValue>().Select(v => v.Value).ToList();
                    }

                    var enumerableType = t.GetType().GetGenericArguments()[0];
                    if (enumerableType == typeof(Range<DateTime>))
                    {
                        var ranges = (IEnumerable<Range<DateTime>>) t;
                        return Builders<T>.Filter.Or(ranges.Select(rg => ConvertDateRange(key, rg)));
                    }
                    
                    IEnumerable<BsonValue> values = t.Cast<object>().Select(BsonValue.Create);
                    return Builders<T>.Filter.In(key, values);
                }
                default:
                    return Builders<T>.Filter.Eq(key, BsonValue.Create(value));
            }
        }

        private static FilterDefinition<T> ConvertDateRange(string key, Range<DateTime> dateRange)
        {
            if (dateRange.Minimum == null && dateRange.Maximum == null) return null;
            var exactTime = dateRange.Maximum.HasValue && dateRange.Maximum.Value.TimeOfDay > new TimeSpan();

            return dateRange.Minimum == null
                ? Builders<T>.Filter.Lt(key, dateRange.Maximum.Value.AddDays(!exactTime ? 1 : 0))
                : dateRange.Maximum == null
                    ? Builders<T>.Filter.Gte(key, dateRange.Minimum.Value)
                    : Builders<T>.Filter.And(
                            Builders<T>.Filter.Gte(key, dateRange.Minimum.Value),
                            Builders<T>.Filter.Lt(key, dateRange.Maximum.Value.AddDays(!exactTime ? 1 : 0)));
        }

        private static FilterDefinition<T> ConstructMongoQuery(IEnumerable<KeyValuePair<string, object>> filters, IEnumerable<string> javaScriptFilters = null)
        {
            Contract.Requires(filters != null);
            //var queries = TransfromRangeFilter(filters)
            //                .Select(kvp => ConstructMongoQuery(kvp.Key, kvp.Value))
            //                .Where(f => f != null)
            //                .ToList();
            var queries = filters.Select(kvp => ConstructMongoQuery(kvp.Key, kvp.Value)).Where(f => f != null).ToList();
            //var x = filterPredicates.Select(f => (FilterDefinition<T>)(Expression<Func<T, bool>>)(d => f(d)))
            if (javaScriptFilters != null)
            {
                queries.AddRange(javaScriptFilters.Select(f => (FilterDefinition<T>)new BsonDocument("$where", new BsonJavaScript(f))));
            }
            return queries.Any() ? Builders<T>.Filter.And(queries) : Builders<T>.Filter.Empty;
        }

        //private static IEnumerable<KeyValuePair<string, object>> TransfromRangeFilter(
        //    IEnumerable<KeyValuePair<string, object>> oldfilters)
        //{
        //    var filters = new Dictionary<string, object>();
        //    foreach (var filter in oldfilters)
        //    {
        //        var lst = filter.Key.Split('-');
        //        if (lst.Count() == 2)
        //        {
        //            var key = lst[0];
        //            Range rg;

        //            object value;
        //            if (filters.TryGetValue(key, out value))
        //            {
        //                rg = (Range)value;
        //            }
        //            else
        //            {
        //                rg = new Range();
        //            }

        //            if (lst[1] == "From")
        //            {
        //                rg.Minimum = (DateTime)filter.Value;
        //            }
        //            else if (lst[1] == "To")
        //            {
        //                rg.Maximum = (DateTime)filter.Value;
        //            }

        //            filters[key] = rg;
        //        }
        //        else
        //        {
        //            filters.Add(filter.Key, filter.Value);
        //        }
        //    }

        //    return filters;
        //}
    }
}