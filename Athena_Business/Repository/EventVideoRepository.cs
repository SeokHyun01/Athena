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
				var createdObj = _db.EventVideos.Add(obj);
				await _db.SaveChangesAsync();

				return _mapper.Map<EventVideo, EventVideoDTO>(createdObj.Entity);
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"타입 \'{ex.GetType().FullName}\'의 에러가 발생했습니다: {ex.Message}");
				throw;
			}
		}

		public ValueTask<int> Delete(int id)
		{
			var obj = await _db.EventVideos.FirstOrDefaultAsync(x => x.Id == id);
			if (obj != null)
			{
				_db.EventVideos.Remove(obj);

				return await _db.SaveChangesAsync();
			}
			return 0;
		}

		public ValueTask<EventVideoDTO?> Get(int id)
		{
			throw new NotImplementedException();
		}

		public async ValueTask<IEnumerable<EventVideoDTO>> GetAll()
		{
			return _mapper.Map<IEnumerable<EventVideo>, IEnumerable<EventVideoDTO>>(_db.EventVideos);
		}

		public ValueTask<EventVideoDTO> Update(EventVideoDTO objDTO)
		{
			throw new NotImplementedException();
		}
	}
}
