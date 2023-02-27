using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class UpdateCamera
	{
		[Required]
		public int CameraId { get; set; }
		public string? Thumbnail { get; set; }
		public int? Degree { get; set; }
	}
}
