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

        public async Task<List<Message>> GetMessagesByConversationIdAsync(Guid conversationId, int limit = 50)
        {
            // Lấy tin nhắn theo phòng và sắp xếp mới nhất lên đầu
            return await _messagesCollection.Find(x => x.ConversationId == conversationId)
                .SortByDescending(x => x.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }
    }
}