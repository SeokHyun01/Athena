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
	public class EventHeaderDTO
	{
		public int Id { get; set; }
		[Required]
		public string UserId { get; set; }
		[Required]
		public int CameraId { get; set; }
		[Required]
		public string Created { get; set; }
		public string? Path { get; set; }
		[Required]
		public bool IsRequiredObjectDetection { get; set; }
		public int? EventVideoId { get; set; }
	}
}
