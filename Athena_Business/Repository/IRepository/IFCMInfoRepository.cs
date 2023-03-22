using Athena_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Business.Repository.IRepository
{
	public interface IFCMInfoRepository
	{
		ValueTask<FCMInfoDTO> Create(FCMInfoDTO objDTO);
		ValueTask<FCMInfoDTO?> Update(FCMInfoDTO objDTO);
		ValueTask<int> Delete(int id);
		ValueTask<FCMInfoDTO?> Get(int id);
		ValueTask<IEnumerable<FCMInfoDTO>> GetAll();
	}
}
