using Athena_Business.Repository.IRepository;
using Athena_Models;
using AthenaWeb_Server.Service.IService;
using MQTTnet;
using MQTTnet.Client;
using System.Text.Json;

namespace AthenaWeb_Server.Service
{
	public class HostedMqttMessageService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<HostedMqttMessageService> _logger;
		private readonly CancellationTokenSource _cancellationTokenSource = new();

		private IMqttMessageService? _mqttMessageService;


		public HostedMqttMessageService(ILogger<HostedMqttMessageService> logger, IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Mqtt Message BackgroundService가 실행 중입니다...");

			using (var scope = _serviceProvider.CreateScope())
			{
				_mqttMessageService = scope.ServiceProvider.GetRequiredService<IMqttMessageService>();

				// 연결
				await _mqttMessageService.ConnectAsync("ictrobot.hknu.ac.kr", 8085);

				// 핸들러
				_mqttMessageService.RegisterMessageHandler(async (e) =>
				{
					var payload = e.ApplicationMessage.Payload;
					_logger.LogInformation($"메시지가 도착했습니다: {e.ApplicationMessage.Topic}");
					if (payload != null && payload.Length > 0)
					{
						if (e.ApplicationMessage.Topic == "event/create")
						{
							var eventObj = JsonSerializer.Deserialize<CreateEvent>(payload);

							if (eventObj != null)
							{
								string filePath;
								if (!string.IsNullOrEmpty(eventObj.Image))
								{
									var imageBytes = Convert.FromBase64String(eventObj.Image.Replace("data:image/png;base64,", string.Empty));

									using (var stream = new MemoryStream(imageBytes))
									{
										filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", $"{Guid.NewGuid()}.png");
										await stream.CopyToAsync(new FileStream(filePath, FileMode.Create));
									}
								}
								else
								{
									throw new ArgumentNullException(eventObj.Image);
								}

								var eventDTO = new EventDTO
								{
									EventHeader = new EventHeaderDTO()
									{
										UserId = eventObj.UserId,
										CameraId = int.Parse(eventObj.CameraId),
										Created = eventObj.Created,
										Path = filePath,
										IsRequiredObjectDetection = true
									},

									EventBodies = new List<EventBodyDTO>()
								};
							}
						}
						else if (e.ApplicationMessage.Topic == "camera/update/degree/syn")
						{
							var updateCamera = JsonSerializer.Deserialize<UpdateCamera>(payload);
							if (updateCamera != null)
							{
								var camera = new CameraDTO
								{
									Id = updateCamera.CameraId,
									Angle = updateCamera.Degree
								};

								await _mqttMessageService.UpdateCamera(camera);
								_logger.LogInformation($"카메라 {updateCamera.CameraId}이 업데이트 됐습니다.");
							}
						}
					}
				});

				// 구독
				var mqttFactory = new MqttFactory();
				var subscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
					.WithTopicFilter(f =>
					{
						f.WithTopic("event/create");
					})
					.WithTopicFilter(f =>
					{
						f.WithTopic("camera/update/degree/syn");
					})
					.Build();
				await _mqttMessageService.SubscribeAsync(subscribeOptions);

				while (!_cancellationTokenSource.IsCancellationRequested)
				{
					await Task.Delay(5000, _cancellationTokenSource.Token);
				}
			}
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			if (_mqttMessageService != null)
			{
				_mqttMessageService.Dispose();
				_logger.LogInformation("Hosted Mqtt Message Service가 정상적으로 종료됐습니다.");
			}
			_cancellationTokenSource.Cancel();

			await Task.CompletedTask;
		}
	}
}
