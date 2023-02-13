using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class CreateEvent
	{
		public string UserId { get; set; }
		public string CameraId { get; set; }
		public string Created { get; set; }
		[Required(ErrorMessage = "이미지가 누락됐습니다.")]
		public string Image { get; set; }
	}
}
