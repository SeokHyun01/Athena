using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_DataAccess
{
	public class AppUser : IdentityUser
	{
		public IEnumerable<FCMInfo> FCMInfos { get; set; }
	}
}
