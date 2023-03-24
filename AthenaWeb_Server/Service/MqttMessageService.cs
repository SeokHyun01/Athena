using Athena_Business.Repository;
using Athena_Business.Repository.IRepository;
using Athena_DataAccess;
using Athena_Models;
using AthenaWeb_Server.Service.IService;
using Microsoft.JSInterop;
using MQTTnet;
using MQTTnet.Client;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace AthenaWeb_Server.Service
{
	public class MqttMessageService : IMqttMessageService
	{
		private readonly IMqttClient _mqttClient;
		private readonly ICameraRepository _cameraRepository;
		private readonly IEventRepository _eventRepository;
		private readonly IEventVideoRepository _eventVideoRepository;
		private readonly IEventHeaderRepository _eventHeaderRepository;
		private readonly IFCMInfoRepository _fcmInfoRepository;
		private readonly HttpClient _client;
		private readonly ILogger<MqttMessageService> _logger;

		private bool _disposedValue;


		public MqttMessageService(
			IMqttClient mqttClient,
			ICameraRepository cameraRepository,
			IEventRepository eventRepository,
			IEventVideoRepository eventVideoRepository,
			IEventHeaderRepository eventHeaderRepository,
			IFCMInfoRepository fcmInfoRepository,
			HttpClient client,
			ILogger<MqttMessageService> logger
			)
		{
			_mqttClient = mqttClient;
			_cameraRepository = cameraRepository;
			_eventRepository = eventRepository;
			_eventVideoRepository = eventVideoRepository;
			_eventHeaderRepository = eventHeaderRepository;
			_fcmInfoRepository = fcmInfoRepository;
			_client = client;
			_logger = logger;
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

		public async ValueTask<CameraDTO> UpdateCamera(CameraDTO camera) => await _cameraRepository.Update(camera);

		public async ValueTask<EventHeaderDTO> CreateEvent(EventDTO eventObj) => await _eventRepository.Create(eventObj);

		public async ValueTask<IEnumerable<EventHeaderDTO>> GetEventHeaders(IEnumerable<int>? ids = null) => await _eventHeaderRepository.GetAll(ids);

		public async ValueTask<EventVideoDTO> CreateEventVideo(EventVideoDTO eventVideo) => await _eventVideoRepository.Create(eventVideo);

		public async ValueTask<EventHeaderDTO?> UpdateEventHeader(EventHeaderDTO eventHeader) => await _eventHeaderRepository.Update(eventHeader);

		public async ValueTask<IEnumerable<FCMInfoDTO>> GetFCMInfos(string? userId = null) => await _fcmInfoRepository.GetAllByUserId(userId);

		public async ValueTask NotifyUser(string token, IEnumerable<string?> labels, string content)
		{
			var serverKey = "AAAAlAPqkMU:APA91bEpsixt1iwXs5ymw67EvF8urDy9Mi3gVbLEYYlgAit94zctOhQuO12pvsD2tuk5oJtzZ9eGAwblxebKyBM8WEQDhYm2ihhBuud5P7cESyFfAycI--IhY4jJ4m2Yr-lJ27qSGK7w";
			
			var fcmUrl = "https://fcm.googleapis.com/fcm/send";

			var label = "motion";
			if (labels.Any())
			{
				label = string.Join(", ", labels);
			}

			var message = new
			{
				to = token,
				data = new
				{
					title = label,
					body = content
				}
			};

			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={serverKey}");
			var response = await _client.PostAsJsonAsync(fcmUrl, message);
			_logger.LogInformation(await response.Content.ReadAsStringAsync());
		}

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