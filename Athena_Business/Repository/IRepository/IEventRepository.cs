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
		ValueTask<EventHeaderDTO> Create(EventDTO objDTO);
		ValueTask<int> Delete(int id);
	}
}
