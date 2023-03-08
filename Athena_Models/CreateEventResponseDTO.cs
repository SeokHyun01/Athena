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
		public EventDTO Event { get; set; }
		public string Error { get; set; }
	}
}
