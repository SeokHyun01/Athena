using Microsoft.AspNetCore.SignalR;

namespace AthenaWeb_Server.Hubs
{
	public class SignalingHub : Hub
	{
		public static Dictionary<string, List<string>> Rooms = new();

		private readonly ILogger<SignalingHub> _logger;


		public SignalingHub(ILogger<SignalingHub> logger)
		{
			_logger = logger;
		}

		public override async Task OnConnectedAsync()
		{
			var senderId = Context.ConnectionId;
			await Clients.Caller.SendAsync("OnConnected", senderId);

			await base.OnConnectedAsync();
		}

		public async ValueTask JoinRoom(string roomName)
		{
			if (!Rooms.ContainsKey(roomName))
			{
				Rooms[roomName] = new List<string>();
			}
			else
			{
				await Clients.Group(roomName).SendAsync("Welcome");
			}

			if (!(Rooms[roomName].Count >= 2))
			{
				Rooms[roomName].Add(Context.ConnectionId);
				await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
				_logger.LogInformation($"클라이언트 {Context.ConnectionId}가 방 {roomName}에 입장했습니다.");
			}
			else
			{
				throw new Exception($"Room {Rooms[roomName]}.Count = {Rooms[roomName].Count}, 정원 초과입니다.");
			}
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			var value = Context.ConnectionId;

			foreach (var room in Rooms)
			{
				if (room.Value.Contains(value))
				{
					var roomName = room.Key;
					room.Value.Remove(value);
					await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
					_logger.LogInformation($"클라이언트 {Context.ConnectionId}가 방 {roomName}에서 퇴장했습니다.");
					if (room.Value.Count == 0)
					{
						Rooms.Remove(roomName);
						_logger.LogInformation($"방 {roomName}이 사라졌습니다.");
					}
					break;
				}
			}

			await base.OnDisconnectedAsync(exception);
		}

		public async ValueTask SendOffer(string offer, string roomName)
		{
			var senderId = Context.ConnectionId;
			var group = Clients.GroupExcept(roomName, senderId);
			await group.SendAsync("ReceiveOffer", offer);
		}

		public async ValueTask SendAnswer(string answer, string roomName)
		{
			var senderId = Context.ConnectionId;
			var group = Clients.GroupExcept(roomName, senderId);
			await group.SendAsync("ReceiveAnswer", answer);
		}

		public async ValueTask SendIce(string ice, string senderId)
		{
			var roomNames = Rooms.Where(x => x.Value.Contains(senderId)).Select(x => x.Key);
			foreach (var roomName in roomNames)
			{
				var group = Clients.GroupExcept(roomName, senderId);
				await group.SendAsync("ReceiveIce", ice);
				_logger.LogInformation($"방 {roomName}으로 {senderId}가 ICE를 전송했습니다.");
			}
		}
	}
}
