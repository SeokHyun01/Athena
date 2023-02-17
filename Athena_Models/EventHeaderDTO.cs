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
		public string UserId { get; set; }
		public int CameraId { get; set; }
		public string Created { get; set; }
		public string Path { get; set; }
		public bool IsRequiredObjectDetection { get; set; }
		public int? EventVideoId { get; set; }
	}
}
