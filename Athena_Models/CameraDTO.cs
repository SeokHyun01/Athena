using Athena_DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class CameraDTO
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		public AppUser User { get; set; }
		public string Name { get; set; }
		public int Angle { get; set; }
	}
}
