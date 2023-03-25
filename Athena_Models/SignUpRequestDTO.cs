using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class SignUpRequestDTO
	{
		[Required(ErrorMessage = "Id를 입력해 주세요.")]
		[RegularExpression("^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$", ErrorMessage = "올바르지 않은 형식의 이메일입니다.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "비밀번호를 입력해 주세요.")]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Required(ErrorMessage = "비밀번호를 한번 더 입력해 주세요.")]
		[DataType(DataType.Password)]
		[Compare("Password", ErrorMessage = "비밀번호가 일치하지 않습니다.")]
		public string ConfirmPassword { get; set; }
		public string? FCMToken { get; set; }
	}
}
