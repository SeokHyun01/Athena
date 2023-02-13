using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_DataAccess
{
	public class Camera
	{
		[Key]
		public int Id { get; set; }
		public string UserId { get; set; }
		public string Name { get; set; }
		public int Angle { get; set; }
	}
}
