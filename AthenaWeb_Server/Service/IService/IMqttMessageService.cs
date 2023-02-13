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
		ValueTask<CameraDTO?> UpdateCamera(CameraDTO camera);
	}
}
