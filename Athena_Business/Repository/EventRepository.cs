using Athena_Business.Repository.IRepository;
using Athena_DataAccess;
using Athena_DataAccess.Data;
using Athena_DataAccess.ViewModel;
using Athena_Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Business.Repository
{
	public class EventRepository : IEventRepository
	{
		private readonly AthenaAppDbContext _db;
		private readonly IMapper _mapper;
		private readonly ILogger<EventRepository> _logger;

		public EventRepository(AthenaAppDbContext db, IMapper mapper, ILogger<EventRepository> logger)
		{
			_db = db;
			_mapper = mapper;
			_logger = logger;
		}

		public async ValueTask<EventDTO> Create(EventDTO objDTO)
		{
			try
			{
				var obj = _mapper.Map<EventDTO, Event>(objDTO);
				_logger.LogInformation($"EventHeader Id: {obj.EventHeader.Id}");
				if (obj.EventHeader.Id == null || obj.EventHeader.Id == 0)
				{
					_db.EventHeaders.Add(obj.EventHeader);
					await _db.SaveChangesAsync();
				}

				if (obj.EventBodies != null && obj.EventBodies.Any())
				{
					foreach (var body in obj.EventBodies)
					{
						body.EventHeaderId = obj.EventHeader.Id;
					}
					_db.EventBodies.AddRange(obj.EventBodies);
					await _db.SaveChangesAsync();
				}

				return new EventDTO()
				{
					EventHeader = _mapper.Map<EventHeader, EventHeaderDTO>(obj.EventHeader),
					EventBodies = _mapper.Map<IEnumerable<EventBody>, IEnumerable<EventBodyDTO>>(obj.EventBodies)
				};
			}

			catch (Exception ex)
			{
				_logger.LogInformation($"타입 \'{ex.GetType().FullName}\'의 에러가 발생했습니다: {ex.Message}");
				throw;
			}

			return objDTO;
		}

		public async ValueTask<int> Delete(int id)
		{
			var objHeader = await _db.EventHeaders.FirstOrDefaultAsync(u => u.Id == id);
			if (objHeader != null)
			{
				IEnumerable<EventBody> objBodies = _db.EventBodies.Where(u => u.EventHeaderId == id);
				_db.EventBodies.RemoveRange(objBodies);
				_db.EventHeaders.Remove(objHeader);

				return _db.SaveChanges();
			}
			return 0;
		}

		public async ValueTask<EventHeaderDTO?> UpdateHeader(EventHeaderDTO header)
		{
			var objFromDb = await _db.EventHeaders.FirstOrDefaultAsync(u => u.Id == header.Id);
			if (objFromDb != null)
			{
				// 업데이트
				objFromDb.EventVideoId = header.EventVideoId ?? objFromDb.EventVideoId;
				objFromDb.Path = header.Path ?? objFromDb.Path;
				_db.EventHeaders.Update(objFromDb);
				await _db.SaveChangesAsync();
				return _mapper.Map<EventHeader, EventHeaderDTO>(objFromDb);
			}

			return null;
		}

		public async ValueTask<EventDTO?> Get(int id)
		{
			Event eventObj = new()
			{
				EventHeader = await _db.EventHeaders.FirstOrDefaultAsync(u => u.Id == id),
				EventBodies = _db.EventBodies.Where(u => u.EventHeaderId == id),
			};

			if (eventObj != null)
			{
				return _mapper.Map<Event, EventDTO>(eventObj);
			}
			return null;
		}

		public async ValueTask<IEnumerable<EventDTO>> GetAll()
		{
			var EventsFromDb = new List<Event>();

			IEnumerable<EventHeader> eventHeaderList = _db.EventHeaders;
			IEnumerable<EventBody> EventBodyList = _db.EventBodies;
			foreach (var header in eventHeaderList)
			{
				var eventObj = new Event()
				{
					EventHeader = header,
					EventBodies = EventBodyList.Where(u => u.EventHeaderId == header.Id),
				};
				EventsFromDb.Add(eventObj);
			}

			return _mapper.Map<IEnumerable<Event>, IEnumerable<EventDTO>>(EventsFromDb);
		}

		public async ValueTask<IEnumerable<EventHeaderDTO>> GetHeader(IEnumerable<int> ids)
		{
			if (ids != null && ids.Any())
			{
				return _mapper.Map<IEnumerable<EventHeader>, IEnumerable<EventHeaderDTO>>(_db.EventHeaders.Where(u => ids.Contains(u.Id)));
			}
			else
			{
				return Enumerable.Empty<EventHeaderDTO>();
			}
		}

		public async ValueTask<IEnumerable<EventDTO>> GetAllByCameraId(int cameraId)
		{
			List<Event> EventsFromDb = new List<Event>();

			if (cameraId > 0)
			{
				IEnumerable<EventHeader> evntHeaderList = _db.EventHeaders.Where(u => u.CameraId == cameraId);

				foreach (var header in evntHeaderList)
				{
					Event eventObj = new()
					{
						EventHeader = header,
						EventBodies = _db.EventBodies.Where(u => u.EventHeaderId == header.Id),
					};
					EventsFromDb.Add(eventObj);
				}
			}

			return _mapper.Map<IEnumerable<Event>, IEnumerable<EventDTO>>(EventsFromDb);
		}

		public async ValueTask<IEnumerable<EventDTO>> GetAllByUserId(string userId)
		{
			List<Event> EventsFromDb = new List<Event>();

			if (!string.IsNullOrEmpty(userId))
			{
				IEnumerable<EventHeader> evntHeaderList = _db.EventHeaders.Where(u => u.UserId == userId);

				foreach (var header in evntHeaderList)
				{
					Event eventObj = new()
					{
						EventHeader = header,
						EventBodies = _db.EventBodies.Where(u => u.EventHeaderId == header.Id),
					};
					EventsFromDb.Add(eventObj);
				}
			}

			return _mapper.Map<IEnumerable<Event>, IEnumerable<EventDTO>>(EventsFromDb);
		}
	}
}
