using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class CameraDTO
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		[Required(ErrorMessage = "카메라 이름을 입력해 주세요.")]
		public string Name { get; set; }
		public int Angle { get; set; }
	}
}
