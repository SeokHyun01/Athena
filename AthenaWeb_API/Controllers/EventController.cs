using Athena_Business.Repository.IRepository;
using Athena_Models;
using Microsoft.AspNetCore.Mvc;

namespace AthenaWeb_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class EventController : Controller
	{
		private readonly IEventRepository _eventRepository;


		public EventController(IEventRepository eventRepository)
		{
			_eventRepository = eventRepository;
		}

		[HttpPost]
		public async ValueTask<IActionResult> Create([FromBody] EventDTO eventObj)
		{
			try
			{
				if (!string.IsNullOrEmpty(eventObj.EventHeader.Path))
				{
					var imageBytes = Convert.FromBase64String(eventObj.EventHeader.Path.Replace("data:image/png;base64,", string.Empty));

					using (var stream = new MemoryStream(imageBytes))
					{
						var filePath = Path.Combine("/home/shyoun/Desktop/Athena-SHYoun/Athena/AthenaWeb_Server/wwwroot/images", $"{Guid.NewGuid()}.png");
						await stream.CopyToAsync(new FileStream(filePath, FileMode.Create));
						eventObj.EventHeader.Path = filePath;
					}

					var createEvent = new EventDTO
					{
						EventHeader = eventObj.EventHeader,
						EventBodies = eventObj.EventBodies
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
