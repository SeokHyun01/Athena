using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class SignUpResponseDTO
	{
		public bool IsSucceeded { get; set; }
		public IEnumerable<string> Errors { get; set; }
	}
}
