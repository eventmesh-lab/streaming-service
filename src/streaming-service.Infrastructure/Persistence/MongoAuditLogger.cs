using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using streaming_service.Domain.Ports;

namespace streaming_service.Infrastructure.Persistence
{
    public class MongoAuditLogger : IAuditLogger
    {
        private readonly IMongoCollection<object> _collection;

        public MongoAuditLogger(MongoDbContext context)
        {
            _collection = context.GetCollection<object>("AuditLogs");
        }

        public async Task LogAccessAsync(object accessLog, CancellationToken cancellationToken = default)
        {
            // MongoDB driver can serialize the anonymous object directly
            await _collection.InsertOneAsync(accessLog, options: null, cancellationToken);
        }
    }
}
