using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_DataAccess
{
	public class EventHeader
	{
		[Key]
		public int Id { get; set; }
		public string Created { get; set; }
		public string? Path { get; set; }
		public bool IsRequiredObjectDetection { get; set; }
		public int CameraId { get; set; }
		[ForeignKey(nameof(CameraId))]
		public Camera Camera { get; set; }
		public IEnumerable<EventBody> EventBodies { get; set; }
		public int EventVideoId { get; set; }
	}
}
