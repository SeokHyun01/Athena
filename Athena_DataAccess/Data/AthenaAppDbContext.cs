using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_DataAccess.Data
{
	public class AthenaAppDbContext : IdentityDbContext
	{
		public AthenaAppDbContext(DbContextOptions<AthenaAppDbContext> options) : base(options)
		{

		}

		public DbSet<Camera> Cameras { get; set; }
		public DbSet<EventHeader> EventHeaders { get; set; }
		public DbSet<EventBody> EventBodies { get; set; }
	}
}
