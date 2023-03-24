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
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Business.Repository
{
	public class EventHeaderRepository : IEventHeaderRepository
	{
		private readonly AthenaAppDbContext _db;
		private readonly IMapper _mapper;
		private readonly ILogger<EventHeaderRepository> _logger;


		public EventHeaderRepository(AthenaAppDbContext db, IMapper mapper, ILogger<EventHeaderRepository> logger)
		{
			_db = db;
			_mapper = mapper;
			_logger = logger;
		}

		public async ValueTask<EventHeaderDTO?> Update(EventHeaderDTO objDTO)
		{
			var obj = await _db.EventHeaders.FirstOrDefaultAsync(u => u.Id == objDTO.Id);
			if (obj != null)
			{
				// 업데이트
				obj.Path = objDTO.Path;
				obj.EventVideoId = objDTO.EventVideoId;
				_db.EventHeaders.Update(obj);
				await _db.SaveChangesAsync();
				return _mapper.Map<EventHeader, EventHeaderDTO>(obj);
			}

			return null;
		}

		public async ValueTask<EventHeaderDTO?> Get(int id)
		{
			var obj = await _db.EventHeaders.Include(x => x.EventBodies).FirstOrDefaultAsync(x => x.Id == id);
			return obj != null ? _mapper.Map<EventHeader, EventHeaderDTO>(obj) : null;
		}

		public async ValueTask<IEnumerable<EventHeaderDTO>> GetAll(IEnumerable<int>? ids = null)
		{
			if (ids != null && ids.Any())
			{
				return _mapper.Map<IEnumerable<EventHeader>, IEnumerable<EventHeaderDTO>>(_db.EventHeaders.Include(x => x.Camera).Include(x => x.EventBodies).Where(x => ids.Contains(x.Id)));
			}

			return _mapper.Map<IEnumerable<EventHeader>, IEnumerable<EventHeaderDTO>>(_db.EventHeaders.Include(x => x.Camera).Include(x => x.EventBodies));
		}

		public async ValueTask<IEnumerable<EventHeaderDTO>> GetAllByCameraId(int cameraId)
		{
			if (cameraId > 0)
			{
				return _mapper.Map<IEnumerable<EventHeader>, IEnumerable<EventHeaderDTO>>(_db.EventHeaders.Include(x => x.EventBodies).Where(x => x.CameraId == cameraId));
			}

			return Enumerable.Empty<EventHeaderDTO>();
		}
	}
}
