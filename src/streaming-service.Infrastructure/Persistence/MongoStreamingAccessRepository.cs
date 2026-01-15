using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using streaming_service.Domain.Entities;
using streaming_service.Domain.Ports;

namespace streaming_service.Infrastructure.Persistence
{
    public class MongoStreamingAccessRepository : IStreamingAccessRepository
    {
        private readonly IMongoCollection<StreamingAccess> _collection;

        public MongoStreamingAccessRepository(MongoDbContext context)
        {
            _collection = context.GetCollection<StreamingAccess>("StreamingAccesses");
        }

        public async Task AddAsync(StreamingAccess access, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(access, options: null, cancellationToken);
        }

        public async Task UpdateAsync(StreamingAccess access, CancellationToken cancellationToken = default)
        {
            await _collection.ReplaceOneAsync(a => a.Id == access.Id, access, cancellationToken: cancellationToken);
        }

        public async Task<StreamingAccess?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _collection.Find(a => a.Token.Token == token).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<StreamingAccess?> GetByUserIdAndSessionIdAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
        {
            return await _collection.Find(a => a.UserId == userId && a.SessionId == sessionId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<StreamingAccess?> GetByUserAndSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
        {
            return await _collection.Find(a => a.UserId == userId && a.SessionId == sessionId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<int> GetAccessCountBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            // Simplified logic: Count documents where SessionId matches. 
            // In a real scenario, this might need to filter by active connections or similar.
            // For now, we assume one StreamingAccess per user per session.
            // This is "Total Registered Users" for the session.
            // For "Active Users", SignalR is the source of truth, but this supports the port contract.
            return (int)await _collection.CountDocumentsAsync(a => a.SessionId == sessionId, cancellationToken: cancellationToken);
        }
    }
}
