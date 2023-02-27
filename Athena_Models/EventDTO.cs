using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class EventDTO
	{
		[Required]
		public EventHeaderDTO EventHeader { get; set; }
		public IEnumerable<EventBodyDTO> EventBodies { get; set; }
	}
}
