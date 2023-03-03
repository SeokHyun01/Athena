using Athena_Business.Repository.IRepository;
using Athena_Models;
using Microsoft.AspNetCore.Mvc;
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
		public async ValueTask<IActionResult> Create([FromBody] EventDTO eventObj)
		{
			try
			{
				if (!string.IsNullOrEmpty(eventObj.EventHeader.Path))
				{
					var imageBytes = Convert.FromBase64String(eventObj.EventHeader.Path.Replace("data:image/jpeg;base64,", string.Empty));

					using (var stream = new MemoryStream(imageBytes))
					{
						//var root = "C:\\Users\\hisn16.DESKTOP-HGVGADP\\source\\repos\\Athena\\AthenaWeb_Server\\wwwroot\\images";
						var root = "/home/shyoun/Desktop/athena-feature-obj-detection/Athena/AthenaWeb_Server/wwwroot/images";
						var filePath = Path.Combine(root, $"{Guid.NewGuid()}.jpeg");
						await stream.CopyToAsync(new FileStream(filePath, FileMode.Create));
						eventObj.EventHeader.Path = filePath;
					}

					var insertEventHeader = eventObj.EventHeader;
					var insertEventBodies = eventObj.EventBodies;

					if (eventObj.EventHeader.IsRequiredObjectDetection)
					{
						var content = JsonConvert.SerializeObject(eventObj);
						var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
						var response = await _client.PostAsync("", bodyContent);
						var contentTemp = await response.Content.ReadAsStringAsync();
						var result = JsonConvert.DeserializeObject<CreateEventResponseDTO>(contentTemp);
						if (result == null)
						{
							_logger.LogInformation("CreateEventResponseDTO 객체를 Deserialize 하는 데 실패했습니다.");
						}
						if (response.IsSuccessStatusCode && result != null)
						{
							insertEventHeader = result.EventHeader;
							insertEventBodies = result.EventBodies;
						}
						else
						{
							throw new Exception(result.Error);
						}
					}

					var createEvent = new EventDTO
					{
						EventHeader = insertEvent.EventHeader,
						EventBodies = insertEvent.EventBodies
					};

					var createdEvent = await _eventRepository.Create(createEvent);

					return Ok(createdEvent.EventHeader.Id);
				}
				else
				{
					throw new ArgumentNullException(eventObj.EventHeader.Path);
				}
			}
			catch (Exception ex)
			{
				return BadRequest($"타입 \'{ex.GetType().FullName}\'의 에러가 발생했습니다: {ex.Message}");
			}
		}
	}
}
