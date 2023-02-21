using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class EventBodyDTO
	{
		public int Id { get; set; }
		public int? EventHeaderId { get; set; }
		[Required]
		public string Label { get; set; }
		[Required]
		public int Left { get; set; }
		[Required]
		public int Right { get; set; }
		[Required]
		public int Top { get; set; }
		[Required]
		public int Bottom { get; set; }
	}
}
