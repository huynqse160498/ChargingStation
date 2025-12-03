using Microsoft.AspNetCore.SignalR;

namespace ChargingStationSystem.Hubs
{
    /// <summary>
    /// Hub demo để test WebSocket/SignalR real-time communication
    /// </summary>
    public class ChargingStationDemoHub : Hub
    {
        /// <summary>
        /// Gửi tin nhắn đến tất cả client đang kết nối
        /// </summary>
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message, DateTime.Now);
        }

        /// <summary>
        /// Gửi tin nhắn đến một user cụ thể (theo ConnectionId)
        /// </summary>
        public async Task SendToUser(string connectionId, string message)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", "System", message, DateTime.Now);
        }

        /// <summary>
        /// Gửi cập nhật trạng thái sạc giả lập
        /// </summary>
        public async Task SendChargingUpdate(int sessionId, int currentSoc, decimal energyKwh, int durationMin)
        {
            var update = new
            {
                SessionId = sessionId,
                CurrentSoc = currentSoc,
                EnergyKwh = energyKwh,
                DurationMin = durationMin,
                Timestamp = DateTime.Now
            };

            await Clients.All.SendAsync("ReceiveChargingUpdate", update);
        }

        /// <summary>
        /// Khi client kết nối thành công
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", 
                $"✅ Kết nối thành công! ConnectionId: {Context.ConnectionId}", DateTime.Now);
            await Clients.AllExcept(Context.ConnectionId).SendAsync("ReceiveMessage", "System", 
                $"👤 User mới đã kết nối (ConnectionId: {Context.ConnectionId})", DateTime.Now);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Khi client ngắt kết nối
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Clients.All.SendAsync("ReceiveMessage", "System", 
                $"👋 User đã ngắt kết nối (ConnectionId: {Context.ConnectionId})", DateTime.Now);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join vào một group (ví dụ: theo sessionId)
        /// </summary>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("ReceiveMessage", "System", 
                $"✅ Đã tham gia group: {groupName}", DateTime.Now);
        }

        /// <summary>
        /// Rời khỏi group
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("ReceiveMessage", "System", 
                $"👋 Đã rời khỏi group: {groupName}", DateTime.Now);
        }

        /// <summary>
        /// Gửi tin nhắn đến một group cụ thể
        /// </summary>
        public async Task SendToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "Group", message, DateTime.Now);
        }
    }
}

