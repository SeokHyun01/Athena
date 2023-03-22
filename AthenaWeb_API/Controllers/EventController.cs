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
					return NotFound();
				}
				using (var stream = new MemoryStream(imageBytes))
				{
					var dir = "/home/shyoun/Desktop/Athena/AthenaWeb_Server/wwwroot/images";
					var path = Path.Combine(dir, $"{Guid.NewGuid()}.jpeg");
					await stream.CopyToAsync(new FileStream(path, FileMode.Create));
					request.EventHeader.Path = path;
				}

				var createEventHeader = request.EventHeader;
				var createEventBodies = request.EventBodies;

				if (request.EventHeader.IsRequiredObjectDetection)
				{
					var content = JsonConvert.SerializeObject(request);
					var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
					var response = await _client.PostAsync("http://localhost:8000/event/create/", bodyContent);
					var contentTemp = await response.Content.ReadAsStringAsync();
					var result = JsonConvert.DeserializeObject<ObjectDetectionResponseDTO>(contentTemp);
					if (response.IsSuccessStatusCode)
					{
						createEventHeader = result.EventHeader;
						createEventBodies = result.EventBodies;

						if (createEventBodies == null || !createEventBodies.Any())
						{
							if (System.IO.File.Exists(createEventHeader.Path))
							{
								System.IO.File.Delete(createEventHeader.Path);
							}

							return Ok();
						}
					}
					else
					{
						throw new Exception(result.ErrorMessage);
					}
				}

				var createEvent = new EventDTO
				{
					EventHeader = createEventHeader,
					EventBodies = createEventBodies
				};

				var createdEvent = await _eventRepository.Create(createEvent);

				return Ok(createdEvent.EventHeader.Id);
			}
			catch (Exception ex)
			{
				var response = new
				{
					Error = $"타입 \'{ex.GetType().FullName}\'의 에러가 발생했습니다: {ex.Message}",
				};
				return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
			}
		}
	}
}
