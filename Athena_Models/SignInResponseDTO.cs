using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class SignInResponseDTO
	{
		public bool IsSucceeded { get; set; }
		public string ErrorMessage { get; set; }
		public string Token { get; set; }
		public UserDTO UserDTO { get; set; }
	}
}
