using Athena_Business.Repository.IRepository;
using Athena_DataAccess;
using Athena_DataAccess.Data;
using Athena_DataAccess.ViewModel;
using Athena_Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Business.Repository
{
	public class EventVideoRepository : IEventVideoRepository
	{
		private readonly AthenaAppDbContext _db;
		private readonly IMapper _mapper;
		private readonly ILogger<EventVideoRepository> _logger;


		public EventVideoRepository(AthenaAppDbContext db, IMapper mapper, ILogger<EventVideoRepository> logger)
		{
			_db = db;
			_mapper = mapper;
			_logger = logger;
		}

		public async ValueTask<EventVideoDTO> Create(EventVideoDTO objDTO)
		{
			try
			{
				var obj = _mapper.Map<EventVideoDTO, EventVideo>(objDTO);
				var addedObj = _db.EventVideos.Add(obj);
				await _db.SaveChangesAsync();

				return _mapper.Map<EventVideo, EventVideoDTO>(addedObj.Entity);

			}
			catch (Exception ex)
			{
				_logger.LogInformation($"타입 \'{ex.GetType().FullName}\'의 에러가 발생했습니다: {ex.Message}");
				throw;
			}
		}

		public ValueTask<int> Delete(int id)
		{
			throw new NotImplementedException();
		}

		public ValueTask<EventVideoDTO?> Get(int id)
		{
			throw new NotImplementedException();
		}

		public ValueTask<IEnumerable<EventVideoDTO>> GetAll()
		{
			throw new NotImplementedException();
		}

		public async ValueTask<IEnumerable<EventVideoDTO>> GetAllByCameraId(int cameraId)
		{
			if (cameraId > 0)
			{
				return _mapper.Map<IEnumerable<EventVideo>, IEnumerable<EventVideoDTO>>(_db.EventVideos.Where(u => u.CameraId == cameraId));
			}
			else
			{
				return Enumerable.Empty<EventVideoDTO>();
			}
		}

		public async ValueTask<IEnumerable<EventVideoDTO>> GetAllByUserId(string userId)
		{
			if (!string.IsNullOrEmpty(userId))
			{
				return _mapper.Map<IEnumerable<EventVideo>, IEnumerable<EventVideoDTO>>(_db.EventVideos.Where(u => u.UserId == userId));
			} else
			{
				return Enumerable.Empty<EventVideoDTO>();
			}
		}

		public ValueTask<EventVideoDTO> Update(EventVideoDTO objDTO)
		{
			throw new NotImplementedException();
		}
	}
}
