using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ChargingStationSystem.Hubs;

namespace ChargingStationSystem.Controllers
{
    /// <summary>
    /// Controller demo để trigger gửi tin nhắn qua SignalR Hub
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DemoHubController : ControllerBase
    {
        private readonly IHubContext<ChargingStationDemoHub> _hubContext;

        public DemoHubController(IHubContext<ChargingStationDemoHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Gửi tin nhắn broadcast đến tất cả client đang kết nối
        /// </summary>
        [HttpPost("broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastMessageDto dto)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", 
                dto.User ?? "System", 
                dto.Message, 
                DateTime.Now);

            return Ok(new 
            { 
                success = true, 
                message = "Đã gửi tin nhắn broadcast thành công" 
            });
        }

        /// <summary>
        /// Gửi tin nhắn đến một connection cụ thể
        /// </summary>
        [HttpPost("send-to-user")]
        public async Task<IActionResult> SendToUser([FromBody] SendToUserDto dto)
        {
            await _hubContext.Clients.Client(dto.ConnectionId).SendAsync("ReceiveMessage", 
                "System", 
                dto.Message, 
                DateTime.Now);

            return Ok(new 
            { 
                success = true, 
                message = $"Đã gửi tin nhắn đến connection: {dto.ConnectionId}" 
            });
        }

        /// <summary>
        /// Gửi tin nhắn đến một group
        /// </summary>
        [HttpPost("send-to-group")]
        public async Task<IActionResult> SendToGroup([FromBody] SendToGroupDto dto)
        {
            await _hubContext.Clients.Group(dto.GroupName).SendAsync("ReceiveMessage", 
                "Group", 
                dto.Message, 
                DateTime.Now);

            return Ok(new 
            { 
                success = true, 
                message = $"Đã gửi tin nhắn đến group: {dto.GroupName}" 
            });
        }

        /// <summary>
        /// Simulate cập nhật trạng thái sạc (demo)
        /// </summary>
        [HttpPost("simulate-charging")]
        public async Task<IActionResult> SimulateCharging([FromBody] SimulateChargingDto dto)
        {
            var update = new
            {
                SessionId = dto.SessionId,
                CurrentSoc = dto.CurrentSoc,
                EnergyKwh = dto.EnergyKwh,
                DurationMin = dto.DurationMin,
                Timestamp = DateTime.Now
            };

            await _hubContext.Clients.All.SendAsync("ReceiveChargingUpdate", update);

            return Ok(new 
            { 
                success = true, 
                message = "Đã gửi cập nhật trạng thái sạc",
                data = update
            });
        }

        /// <summary>
        /// Start simulation tự động (gửi cập nhật mỗi 2 giây)
        /// </summary>
        [HttpPost("start-auto-simulation/{sessionId}")]
        public async Task<IActionResult> StartAutoSimulation(int sessionId)
        {
            _ = Task.Run(async () =>
            {
                int currentSoc = 20;
                decimal energyKwh = 0;
                int durationMin = 0;

                for (int i = 0; i < 30; i++) // Simulate 30 updates (60 giây)
                {
                    await Task.Delay(2000); // 2 giây mỗi lần update

                    currentSoc = Math.Min(100, currentSoc + 3);
                    energyKwh += 0.5m;
                    durationMin += 2;

                    var update = new
                    {
                        SessionId = sessionId,
                        CurrentSoc = currentSoc,
                        EnergyKwh = energyKwh,
                        DurationMin = durationMin,
                        Timestamp = DateTime.Now
                    };

                    await _hubContext.Clients.All.SendAsync("ReceiveChargingUpdate", update);
                }

                // Gửi thông báo hoàn thành
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", 
                    "System", 
                    $"✅ Phiên sạc #{sessionId} đã hoàn thành!", 
                    DateTime.Now);
            });

            return Ok(new 
            { 
                success = true, 
                message = $"Đã bắt đầu simulation tự động cho session #{sessionId}" 
            });
        }
    }

    // DTOs
    public class BroadcastMessageDto
    {
        public string? User { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SendToUserDto
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class SendToGroupDto
    {
        public string GroupName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class SimulateChargingDto
    {
        public int SessionId { get; set; }
        public int CurrentSoc { get; set; }
        public decimal EnergyKwh { get; set; }
        public int DurationMin { get; set; }
    }
}

