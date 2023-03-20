using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_DataAccess
{
	public class FCMInfo
	{
		[Key]
		public int Id { get; set; }
		public string UserId { get; set; }
		[ForeignKey("UserId")]
		public AppUser User { get; set; }
		public string Token { get; set; }
	}
}
