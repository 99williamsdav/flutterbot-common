using Common.Interfaces;
using Common.Models;
using Common.Utils;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Linq;

namespace Common.Persistence
{
    public class ShortIdGenerator<TDocument> : IIdGenerator
    {
        private IPersist<SequenceId> _storage;

        private object _lock = new object();

        public ShortIdGenerator(IPersist<SequenceId> storage)
        {
            _storage = storage;
        }

        public object GenerateId(object container, object document)
        {
            var c = (IMongoCollection<TDocument>)container;
            var collection = c.CollectionNamespace.CollectionName;
            SequenceId sequence;
            lock (_lock)
            {
                sequence = _storage.Find(new [] { Fn.Kvp(nameof(SequenceId.Subject), collection) }).SingleOrDefault();
                if (sequence == null)
                {
                    sequence = new SequenceId
                    {
                        Subject = collection,
                        Sequence = 1
                    };
                }
                else
                {
                    sequence.Sequence++;
                }

                _storage.Save(sequence);
            }

            return sequence.Sequence.EncodeBase36();
        }

        public bool IsEmpty(object id)
        {
            return string.IsNullOrEmpty(id as string);
        }
    }
}
