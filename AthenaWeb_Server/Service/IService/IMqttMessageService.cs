using Athena_Models;
using MQTTnet.Client;

namespace AthenaWeb_Server.Service.IService
{
	public interface IMqttMessageService : IDisposable
	{
		ValueTask ConnectAsync(string brokerHost, int brokerPort);
		ValueTask SubscribeAsync(string topic);
		ValueTask SubscribeAsync(MqttClientSubscribeOptions options);
		ValueTask UnsubscribeAsync(string topic);
		ValueTask UnsubscribeAsync(MqttClientUnsubscribeOptions options);
		void RegisterMessageHandler(Func<MqttApplicationMessageReceivedEventArgs, Task> handler);
		ValueTask<CameraDTO> UpdateCamera(CameraDTO camera);
		ValueTask<EventDTO> CreateEvent(EventDTO eventObj);
		ValueTask<IEnumerable<EventHeaderDTO>> GetEventHeader(IEnumerable<int>? ids = null);
		ValueTask<EventVideoDTO> CreateEventVideo(EventVideoDTO eventVideo);
		ValueTask<EventHeaderDTO?> UpdateEventHeader(EventHeaderDTO eventHeader);
		ValueTask<EventHeaderDTO?> DeleteEventHeaderPath(EventHeaderDTO eventHeader);
	}
}
