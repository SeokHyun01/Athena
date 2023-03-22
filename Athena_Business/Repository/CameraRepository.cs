using Athena_Business.Repository.IRepository;
using Athena_DataAccess;
using Athena_DataAccess.Data;
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
	public class CameraRepository : ICameraRepository
	{
		private readonly AthenaAppDbContext _db;
		private readonly IMapper _mapper;
		private readonly ILogger<CameraRepository> _logger;


		public CameraRepository(AthenaAppDbContext db, IMapper mapper, ILogger<CameraRepository> logger)
		{
			_db = db;
			_mapper = mapper;
			_logger = logger;
		}

		public async ValueTask<CameraDTO> Create(CameraDTO objDTO)
		{
			try
			{
				var obj = _mapper.Map<CameraDTO, Camera>(objDTO);
				var createdObj = _db.Cameras.Add(obj);
				await _db.SaveChangesAsync();

				return _mapper.Map<Camera, CameraDTO>(createdObj.Entity);
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"타입 \'{ex.GetType().FullName}\'의 에러가 발생했습니다: {ex.Message}");
				throw;
			}
		}

		public async ValueTask<int> Delete(int id)
		{
			var obj = await _db.Cameras.FirstOrDefaultAsync(x => x.Id == id);
			if (obj != null)
			{
				_db.Cameras.Remove(obj);

				return await _db.SaveChangesAsync();
			}
			return 0;
		}

		public async ValueTask<CameraDTO?> Get(int id)
		{
			var obj = await _db.Cameras.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
			if (obj != null)
			{
				return _mapper.Map<Camera, CameraDTO>(obj);
			}
			return null;
		}

		public async ValueTask<IEnumerable<CameraDTO>> GetAll()
		{
			return _mapper.Map<IEnumerable<Camera>, IEnumerable<CameraDTO>>(_db.Cameras.Include(x => x.User));
		}

		public async ValueTask<IEnumerable<CameraDTO>> GetAllByUserId(string? userId = null)
		{
			if (!string.IsNullOrEmpty(userId))
			{
				return _mapper.Map<IEnumerable<Camera>, IEnumerable<CameraDTO>>(_db.Cameras.Where(x => x.UserId == userId).Include(x => x.User));
			}
			else
			{
				return _mapper.Map<IEnumerable<Camera>, IEnumerable<CameraDTO>>(_db.Cameras.Include(x => x.User));
			}
		}

		public async ValueTask<CameraDTO?> Update(CameraDTO objDTO)
		{
			var objFromDb = await _db.Cameras.FirstOrDefaultAsync(x => x.Id == objDTO.Id);
			if (objFromDb != null)
			{
				// 업데이트
				objFromDb.Angle = objDTO.Angle;
				_db.Cameras.Update(objFromDb);
				await _db.SaveChangesAsync();
				return _mapper.Map<Camera, CameraDTO>(objFromDb);
			}

			return null;
		}
	}
}
