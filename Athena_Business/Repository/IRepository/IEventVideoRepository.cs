using Athena_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Business.Repository.IRepository
{
	public interface IEventVideoRepository
	{
		ValueTask<EventVideoDTO> Create(EventVideoDTO objDTO);
		ValueTask<EventVideoDTO> Update(EventVideoDTO objDTO);
		ValueTask<int> Delete(int id);
		ValueTask<EventVideoDTO?> Get(int id);
		ValueTask<IEnumerable<EventVideoDTO>> GetAll();
		ValueTask<IEnumerable<EventVideoDTO>> GetAllByUserId(string userId);
		ValueTask<IEnumerable<EventVideoDTO>> GetAllByCameraId(int cameraId);
	}
}
