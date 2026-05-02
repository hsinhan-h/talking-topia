using Web.Models.MongoDB;
using Web.Models.MongoDB.Entities;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using ApplicationCore.Interfaces;

namespace Web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly MongoRepository _mongoRepository;
        private static Dictionary<string, string> _connections = new Dictionary<string, string>();
        private readonly IRepository<ApplicationCore.Entities.Member> _memberRepository;

        public ChatHub(MongoRepository repository, IRepository<ApplicationCore.Entities.Member> memberRepository)
        {
            _mongoRepository = repository;
            _memberRepository = memberRepository;
        }

        /// <summary>
        /// 連線SignalR伺服器時觸發
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var senderId = httpContext.Request.Query["senderId"];
            var receiverId = httpContext.Request.Query["receiverId"];

            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId))
            {
                await Clients.Caller.SendAsync("Error", "無效的使用者 ID。");
                Context.Abort();
                return;
            }

            _connections[Context.ConnectionId] = senderId;

            var groupName = GetGroupName(senderId, receiverId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            Console.WriteLine($"使用者 {senderId} 已連線。");
            await base.OnConnectedAsync();
        }

        private string GetGroupName(string user1, string user2)
        {
            return string.CompareOrdinal(user1, user2) < 0 ? $"{user1}-{user2}" : $"{user2}-{user1}";
        }

        /// <summary>
        /// 斷開SignalR伺服器時觸發
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"使用者已斷線：{Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendPrivateMessage(string receiverId, string message)
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var senderId = httpContext.Request.Query["senderId"];

                if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId))
                {
                    await Clients.Caller.SendAsync("Error", "無效的使用者 ID。");
                    return;
                }


                var sender = await _memberRepository.GetByIdAsync(int.Parse(senderId.ToString()));
                var receiver = await _memberRepository.GetByIdAsync(int.Parse(receiverId)); ;

                var newMessage = new Message
                {
                    MessageId = ObjectId.GenerateNewId(),
                    ConversationId = CreateConversationId(senderId, receiverId),
                    SenderId = senderId,
                    SenderName = sender.FirstName + " " + sender.LastName,
                    ReceiverId = receiverId,
                    ReceiverName = receiver.FirstName + " " + receiver.LastName,
                    Content = message,
                    Timestamp = DateTime.UtcNow,
                    MessageType = MessageType.Sent,
                    Visibility = new Visibility { Sender = true, Receiver = true }
                };
                await _mongoRepository.CreateMessageByConversationIdAsync(newMessage.ConversationId, newMessage);

                var groupName = GetGroupName(senderId, receiverId);

                await Clients.Group(groupName).SendAsync("ReceiveMessage", senderId, message);
                Console.WriteLine($"訊息已發送至群組 {groupName}：{senderId} 對 {receiverId} 說 {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"發送訊息錯誤：{ex.Message}");
                await Clients.Caller.SendAsync("Error", "伺服器發生錯誤，請稍後再試。");
            }
        }

        private ObjectId CreateConversationId(string user1, string user2)
        {
            var sortedUsers = string.Compare(user1, user2) < 0 ? $"{user1}_{user2}" : $"{user2}_{user1}";
            return new ObjectId(HashStringToObjectId(sortedUsers));
        }

        private string HashStringToObjectId(string input)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                var hash = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 24);
            }
        }
    }
}
