using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using ChatService.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ChatService.Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly IMongoCollection<Message> _messagesCollection;
        public MessageRepository(IOptions<MongoDbSettings> mongoDbSettings)
        {
            // Khởi tạo kết nối đến MongoDB
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _messagesCollection = mongoDatabase.GetCollection<Message>(mongoDbSettings.Value.CollectionName);
        }

        public async Task AddAsync(Message message)
        {
            // MongoDB Insert cực kỳ nhanh
            await _messagesCollection.InsertOneAsync(message);
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(Guid conversationId, int limit = 50, DateTime? before = null)
        {
            // Lấy tin nhắn theo phòng và sắp xếp mới nhất lên đầu
            var filterBuilder = Builders<Message>.Filter;
            var filter = filterBuilder.Eq(x => x.ConversationId, conversationId);

            if (before.HasValue)
            {
                filter &= filterBuilder.Lt(x => x.CreatedAt, before.Value);
            }

            return await _messagesCollection.Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task MarkMessagesAsReadAsync(Guid conversationId, Guid userId)
        {
            // Cập nhật tất cả tin nhắn trong phòng đã đọc bởi userId
            var filterBuilder = Builders<Message>.Filter;
            var filter = filterBuilder.Eq(x => x.ConversationId, conversationId) &
                         filterBuilder.Ne(x => x.SenderId, userId) &
                         filterBuilder.Not(filterBuilder.AnyEq(x => x.ReadBy, userId));

            var update = Builders<Message>.Update.AddToSet(x => x.ReadBy, userId);

            await _messagesCollection.UpdateManyAsync(filter, update);
        }
    }
}