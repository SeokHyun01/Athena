using Athena_Business.Repository;
using Athena_Business.Repository.IRepository;
using Athena_DataAccess;
using Athena_Models;
using AthenaWeb_Server.Service.IService;
using MQTTnet;
using MQTTnet.Client;

namespace AthenaWeb_Server.Service
{
	public class MqttMessageService : IMqttMessageService
	{
		private readonly IMqttClient _mqttClient;
		private readonly ICameraRepository _cameraRepository;

		private bool _disposedValue;


		public MqttMessageService(IMqttClient mqttClient, ICameraRepository cameraRepository)
		{
			_mqttClient = mqttClient;
			_cameraRepository = cameraRepository;
		}

		public async ValueTask ConnectAsync(string brokerHost, int brokerPort)
		{
			var options = new MqttClientOptionsBuilder()
				.WithTcpServer(brokerHost, brokerPort)
				.WithCleanSession(true)
				.Build();

			// 재연결
			_mqttClient.DisconnectedAsync += async (e) =>
			{
				if (e.ClientWasConnected)
				{
					await _mqttClient.ConnectAsync(_mqttClient.Options);
				}
			};

			await _mqttClient.ConnectAsync(options);
		}

		public async ValueTask SubscribeAsync(string topic)
		{
			await _mqttClient.SubscribeAsync(topic);
		}

		public async ValueTask SubscribeAsync(MqttClientSubscribeOptions options)
		{
			await _mqttClient.SubscribeAsync(options);
		}

		public async ValueTask UnsubscribeAsync(string topic)
		{
			await _mqttClient.UnsubscribeAsync(topic);
		}

		public async ValueTask UnsubscribeAsync(MqttClientUnsubscribeOptions options)
		{
			await _mqttClient.UnsubscribeAsync(options);
		}

		public void RegisterMessageHandler(Func<MqttApplicationMessageReceivedEventArgs, Task> handler)
		{
			_mqttClient.ApplicationMessageReceivedAsync += handler;
		}

		public async ValueTask<CameraDTO?> UpdateCamera(CameraDTO camera) => await _cameraRepository.Update(camera);

		public void Dispose() => Dispose(true);

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					_mqttClient?.Dispose();
				}

				_disposedValue = true;
			}
		}
	}
}