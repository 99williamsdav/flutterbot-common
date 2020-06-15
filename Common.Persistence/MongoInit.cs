
using Common.ModelInterfaces;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using System;

namespace Common.Persistence
{
    public static class MongoInit
    {
        public static void Initialize<T>(IIdGenerator idGenerator = null, Action<BsonClassMap<T>> mapAction = null) where T : IIdentifier
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                return;
            }

            BsonClassMap.RegisterClassMap<T>(
                map =>
                {
                    map.AutoMap();
                    if (map.IdMemberMap != null)
                    {
                        map.SetIdMember(map.GetMemberMap(c => c.Id));
                        map.IdMemberMap.SetIdGenerator(idGenerator ?? StringObjectIdGenerator.Instance);
                    }

                    mapAction?.Invoke(map);
                });
        }

        public static void InitializeBase<T>(IIdGenerator idGenerator = null) where T : IIdentifier
        {
            BsonClassMap.RegisterClassMap<T>(
                map =>
                {
                    map.AutoMap();
                    map.SetIsRootClass(true);
                    map.SetIdMember(map.GetMemberMap(c => c.Id));
                    map.IdMemberMap.SetIdGenerator(idGenerator ?? StringObjectIdGenerator.Instance);
                });
        }
    }
}
