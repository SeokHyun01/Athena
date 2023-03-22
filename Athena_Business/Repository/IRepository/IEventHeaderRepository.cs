using Athena_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Business.Repository.IRepository
{
	public interface IEventHeaderRepository
	{
		ValueTask<EventHeaderDTO?> Update(EventHeaderDTO objDTO);
		ValueTask<EventHeaderDTO?> Get(int id);
		ValueTask<IEnumerable<EventHeaderDTO>> GetAll(IEnumerable<int> ids);
		ValueTask<IEnumerable<EventHeaderDTO>> GetAllByCameraId(int cameraId);
	}
}
