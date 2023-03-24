using Athena_Business.Repository.IRepository;
using Athena_Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace AthenaWeb_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class EventController : Controller
	{
		private readonly IEventRepository _eventRepository;
		private readonly HttpClient _client;
		private readonly ILogger<EventController> _logger;


		public EventController(IEventRepository eventRepository, HttpClient client, ILogger<EventController> logger)
		{
			_eventRepository = eventRepository;
			_client = client;
			_logger = logger;
		}

		[HttpPost]
		public async ValueTask<IActionResult> Create([FromBody] EventDTO request)
		{
			try
			{
				var imageBytes = Convert.FromBase64String(request.EventHeader.Path.Replace("data:image/jpeg;base64,", string.Empty));
				if (imageBytes == null || !imageBytes.Any())
				{
					throw new ArgumentNullException(nameof(imageBytes));
				}
				using (var stream = new MemoryStream(imageBytes))
				{
					var dir = "/home/shyoun/Desktop/Athena/AthenaWeb_Server/wwwroot/images";
					var path = Path.Combine(dir, $"{Guid.NewGuid()}.jpeg");
					await stream.CopyToAsync(new FileStream(path, FileMode.Create));
					request.EventHeader.Path = path;
				}

				if (request.EventHeader.IsRequiredObjectDetection)
				{
					var content = new { path = request.EventHeader.Path };
					var bodyContent = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
					var response = await _client.PostAsync("http://localhost:8000/event/create/", bodyContent);
					var contentTemp = await response.Content.ReadAsStringAsync();
					var objectDetectionResponse = JsonConvert.DeserializeObject<ObjectDetectionResponseDTO>(contentTemp);
					if (response.IsSuccessStatusCode)
					{
						request.EventHeader.IsRequiredObjectDetection = false;

						if (!objectDetectionResponse.Results.Any())
						{
							if (System.IO.File.Exists(objectDetectionRequest.Path))
							{
								System.IO.File.Delete(objectDetectionRequest.Path);
							}

							return Ok(0);
						}

						foreach (var result in objectDetectionResponse.Results)
						{
							request.EventBodies.Append(new EventBodyDTO
							{
								Label = result.Label == 0 ? "fire" : "smoke",
								Left = result.Left,
								Right = result.Right,
								Top = result.Top,
								Bottom = result.Bottom,
							});
						}
					}
					else
					{
						throw new Exception(objectDetectionResponse.ErrorMessage);
					}
				}

				var createEvent = new EventDTO
				{
					EventHeader = request.EventHeader,
					EventBodies = request.EventBodies
				};

				var createdEventHeader = await _eventRepository.Create(createEvent);

				return Ok(createdEventHeader.Id);
			}
			catch (Exception ex)
			{
				return Problem($"타입 \'{ex.GetType().FullName}\'의 에러가 발생했습니다: {ex.Message}");
			}
		}
	}
}
