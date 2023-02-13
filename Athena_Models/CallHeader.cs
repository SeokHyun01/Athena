using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class CallHeader
	{
		[Required(ErrorMessage = "User ID가 누락됐습니다.")]
		public string? UserId { get; set; }
		public int CameraId { get; set; }
	}
}
