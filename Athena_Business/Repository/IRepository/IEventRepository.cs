using Athena_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Business.Repository.IRepository
{
	public interface IEventRepository
	{
		public ValueTask<EventDTO> Create(EventDTO objDTO);
		public ValueTask<int> Delete(int id);
		public ValueTask<EventDTO?> Get(int id);
		public ValueTask<IEnumerable<EventDTO>> GetAll();
		public ValueTask<IEnumerable<EventDTO>> GetAllByUserId(string userId);
		public ValueTask<IEnumerable<EventDTO>> GetAllByCameraId(int cameraId);
		ValueTask<IEnumerable<EventHeaderDTO>> GetHeader(IEnumerable<int>? ids = null);
		ValueTask<EventHeaderDTO> UpdateHeader(EventHeaderDTO header);
	}
}
