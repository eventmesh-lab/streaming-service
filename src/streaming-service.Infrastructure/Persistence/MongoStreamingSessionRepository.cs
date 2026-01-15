using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using streaming_service.Domain.Entities;
using streaming_service.Domain.Ports;

namespace streaming_service.Infrastructure.Persistence
{
    public class MongoStreamingSessionRepository : IStreamingSessionRepository
    {
        private readonly IMongoCollection<StreamingSession> _collection;

        public MongoStreamingSessionRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<StreamingSession>("StreamingSessions");
        }

        public async Task AddAsync(StreamingSession session, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(session, options: null, cancellationToken);
        }

        public async Task<StreamingSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _collection.Find(s => s.Id == id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpdateAsync(StreamingSession session, CancellationToken cancellationToken = default)
        {
            await _collection.ReplaceOneAsync(s => s.Id == session.Id, session, cancellationToken: cancellationToken);
        }

        public async Task<StreamingSession?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _collection.Find(s => s.EventId == eventId).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
