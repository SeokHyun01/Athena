using Athena_Business.Repository;
using Athena_Business.Repository.IRepository;
using Athena_DataAccess;
using Athena_Models;
using AthenaWeb_Server.Service.IService;
using Microsoft.JSInterop;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using Syncfusion.Blazor.Kanban.Internal;
using System.Net.Http;
using System.Text;

namespace AthenaWeb_Server.Service
{
	public class MqttMessageService : IMqttMessageService
	{
		private readonly IMqttClient _mqttClient;
		private readonly ICameraRepository _cameraRepository;
		private readonly IEventRepository _eventRepository;
		private readonly IEventVideoRepository _eventVideoRepository;
		private readonly HttpClient _httpClient;
		private readonly ILogger<MqttMessageService> _logger;

		private bool _disposedValue;


		public MqttMessageService(IMqttClient mqttClient, ICameraRepository cameraRepository, IEventRepository eventRepository, IEventVideoRepository eventVideoRepository, HttpClient httpClient, ILogger<MqttMessageService> logger)
		{
			_mqttClient = mqttClient;
			_cameraRepository = cameraRepository;
			_eventRepository = eventRepository;
			_eventVideoRepository = eventVideoRepository;
			_httpClient = httpClient;
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

		public async ValueTask<EventDTO> CreateEvent(EventDTO eventObj) => await _eventRepository.Create(eventObj);

		public async ValueTask<IEnumerable<EventHeaderDTO>> GetEventHeader(IEnumerable<int> ids) => await _eventRepository.GetHeader(ids);

		public async ValueTask<EventVideoDTO> CreateEventVideo(EventVideoDTO eventVideo) => await _eventVideoRepository.Create(eventVideo);

		public async ValueTask<EventHeaderDTO?> UpdateEventHeader(EventHeaderDTO eventHeader) => await _eventRepository.UpdateHeader(eventHeader);

		public async ValueTask<CreateEventResponseDTO> PredictEvent(CreateEventRequestDTO createEventRequest)
		{
			var content = JsonConvert.SerializeObject(createEventRequest);
			var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync("http://localhost:8102/api/event/events/", bodyContent);
			var contentTemp = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<IEnumerable<EventDTO>>(contentTemp);
			if (response.IsSuccessStatusCode && result != null)
			{
				return new CreateEventResponseDTO { IsSucceeded = true, EventList = result };
			}
			else
			{
				return new CreateEventResponseDTO { IsSucceeded = false };
			}
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