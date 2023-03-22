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
	public class FCMInfoRepository : IFCMInfoRepository
	{
		private readonly AthenaAppDbContext _db;
		private readonly IMapper _mapper;
		private readonly ILogger<FCMInfoRepository> _logger;


		public FCMInfoRepository(AthenaAppDbContext db, IMapper mapper, ILogger<FCMInfoRepository> logger)
		{
			_db = db;
			_mapper = mapper;
			_logger = logger;
		}

		public async ValueTask<FCMInfoDTO> Create(FCMInfoDTO objDTO)
		{
			try
			{
				var obj = _mapper.Map<FCMInfoDTO, FCMInfo>(objDTO);
				var createdObj = _db.FCMInfos.Add(obj);
				await _db.SaveChangesAsync();

				return _mapper.Map<FCMInfo, FCMInfoDTO>(createdObj.Entity);
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"타입 \'{ex.GetType().FullName}\'의 에러가 발생했습니다: {ex.Message}");
				throw;
			}
		}

		public async ValueTask<int> Delete(int id)
		{
			var obj = await _db.FCMInfos.FirstOrDefaultAsync(x => x.Id == id);
			if (obj != null)
			{
				_db.FCMInfos.Remove(obj);

				return await _db.SaveChangesAsync();
			}
			return 0;
		}

		public async ValueTask<FCMInfoDTO?> Get(int id)
		{
			var obj = await _db.FCMInfos.FirstOrDefaultAsync(x => x.Id == id);
			if (obj != null)
			{
				return _mapper.Map<FCMInfo, FCMInfoDTO>(obj);
			}
			return null;
		}

		public async ValueTask<IEnumerable<FCMInfoDTO>> GetAll()
		{
			return _mapper.Map<IEnumerable<FCMInfo>, IEnumerable<FCMInfoDTO>>(_db.FCMInfos);
		}

		public async ValueTask<FCMInfoDTO?> Update(FCMInfoDTO objDTO)
		{
			var objFromDb = await _db.FCMInfos.FirstOrDefaultAsync(x => x.Id == objDTO.Id);
			if (objFromDb != null)
			{
				// 업데이트
				_db.FCMInfos.Update(objFromDb);
				await _db.SaveChangesAsync();
				return _mapper.Map<FCMInfo, FCMInfoDTO>(objFromDb);
			}

			return null;
		}
	}
}
