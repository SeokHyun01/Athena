using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class CreateEventRequestDTO
	{
		public IEnumerable<EventHeaderDTO> EventHeaderList { get; set;} = Enumerable.Empty<EventHeaderDTO>();
	}
}
