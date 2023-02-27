using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class CreateEventResponseDTO
	{
		public bool IsSucceeded { get; set; }
		public IEnumerable<EventDTO> EventList { get; set; } = Enumerable.Empty<EventDTO>();
		public string Error { get; set; } = string.Empty;
	}
}
