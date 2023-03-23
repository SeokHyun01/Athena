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

		public async ValueTask<EventHeaderDTO> Create(EventDTO objDTO)
		{
			try
			{
				var obj = _mapper.Map<EventDTO, Event>(objDTO);
				var createdObj = _db.EventHeaders.Add(obj.EventHeader);
				await _db.SaveChangesAsync();

				if (obj.EventBodies != null && obj.EventBodies.Any())
				{
					foreach (var body in obj.EventBodies)
					{
						body.EventHeaderId = obj.EventHeader.Id;
					}
					_db.EventBodies.AddRange(obj.EventBodies);
					await _db.SaveChangesAsync();
				}

				return _mapper.Map<EventHeader, EventHeaderDTO>(createdObj.Entity);
			}

			catch (Exception ex)
			{
				_logger.LogInformation($"타입 \'{ex.GetType().FullName}\'의 에러가 발생했습니다: {ex.Message}");
				throw;
			}
		}

		public async ValueTask<int> Delete(int id)
		{
			var header = await _db.EventHeaders.FirstOrDefaultAsync(u => u.Id == id);
			if (header != null)
			{
				IEnumerable<EventBody> bodies = _db.EventBodies.Where(u => u.EventHeaderId == id);
				_db.EventBodies.RemoveRange(bodies);
				_db.EventHeaders.Remove(header);

				return _db.SaveChanges();
			}
			return 0;
		}
	}
}
