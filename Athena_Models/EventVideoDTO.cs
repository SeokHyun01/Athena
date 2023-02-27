using Athena_DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class EventVideoDTO
	{
		public int Id { get; set; }
		[Required]
		public string UserId { get; set; }
		[Required]
		public int CameraId { get; set; }
		[Required]
		public string Path { get; set; }
	}
}
