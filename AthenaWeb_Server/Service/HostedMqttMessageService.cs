﻿using Athena_Business.Repository.IRepository;
using Athena_DataAccess.ViewModel;
using Athena_Models;
using AthenaWeb_Server.Service.IService;
using FFmpegBlazor;
using Microsoft.JSInterop;
using MQTTnet;
using MQTTnet.Client;
using System.Diagnostics;
using System.Text.Json;

namespace AthenaWeb_Server.Service
{
	public class HostedMqttMessageService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<HostedMqttMessageService> _logger;
		private readonly CancellationTokenSource _cancellationTokenSource = new();

		private IMqttMessageService? _mqttMessageService;

		private const string outputFile = "output.mp4";
		private string ProgressMessage { get; set; }
		private FFMPEG? ffMpeg;


		public HostedMqttMessageService(ILogger<HostedMqttMessageService> logger, IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Mqtt Message BackgroundService가 실행 중입니다...");

			try
			{
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
							if (e.ApplicationMessage.Topic == "camera/update/degree/syn")
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
							else if (e.ApplicationMessage.Topic == "video/create")
							{
								var createVideo = JsonSerializer.Deserialize<CreateVideo>(payload);
								if (createVideo != null)
								{
									var imagePathList = (await _mqttMessageService.GetEventHeaderPath(createVideo.EventHeaderIds)).ToList();
									if (imagePathList != null && imagePathList.Any())
									{
										var identifier = Guid.NewGuid().ToString();
										for (int i = 0; i < imagePathList.Count; i++)
										{
											var destinationPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", $"{identifier}_{i + 1}.png");
											File.Copy(imagePathList[i], destinationPath);
										}

										var ffMpeg = new Process
										{
											StartInfo = new ProcessStartInfo
											{
												FileName = "ffmpeg",
												Arguments = $"-i {Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", identifier)}_%d.png -r 1 -pix_fmt yuv420p -c:v libx264 {Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos", Guid.NewGuid().ToString())}.mp4",
												UseShellExecute = false,
												RedirectStandardOutput = true,
												CreateNoWindow = false,
												RedirectStandardError = true
											},
											EnableRaisingEvents = true
										};
										ffMpeg.Start();
										await ffMpeg.WaitForExitAsync();

										var processOutput = string.Empty;
										while ((processOutput = ffMpeg.StandardError.ReadLine()) != null)
										{
											_logger.LogInformation(processOutput);
										}
									}
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
							f.WithTopic("video/create");
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
			catch (Exception ex)
			{
				_logger.LogInformation($"타입 \'{ex.GetType().FullName}\'의 에러가 발생했습니다: {ex.Message}");
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
