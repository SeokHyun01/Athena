﻿using Athena_Common;
using Athena_DataAccess;
using Athena_DataAccess.Data;
using AthenaWeb_Server.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AthenaWeb_Server.Service
{
	public class DbInitializer : IDbInitializer
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly AthenaAppDbContext _db;
		private readonly ILogger<DbInitializer> _logger;

		public DbInitializer(UserManager<AppUser> userManager,
			RoleManager<IdentityRole> roleManager,
			AthenaAppDbContext db,
			ILogger<DbInitializer> logger)
		{
			_db = db;
			_roleManager = roleManager;
			_userManager = userManager;
			_logger = logger;
		}

		public async ValueTask Initialize()
		{
			try
			{
				if (_db.Database.GetPendingMigrations().Count() > 0)
				{
					_db.Database.Migrate();
				}

				if (!await _roleManager.RoleExistsAsync(SD.ROLE_ADMIN))
				{
					await _roleManager.CreateAsync(new IdentityRole(SD.ROLE_ADMIN));
					await _roleManager.CreateAsync(new IdentityRole(SD.ROLE_CUSTOMER));
				}
				else
				{
					return;
				}

				AppUser user = new()
				{
					UserName = "admin@athnea.com",
					Email = "admin@athnea.com",
					EmailConfirmed = true
				};
				await _userManager.CreateAsync(user, "95fur6u?_!deQ%8");
				await _userManager.AddToRoleAsync(user, SD.ROLE_ADMIN);
				await _userManager.AddToRoleAsync(user, SD.ROLE_CUSTOMER);
			}

			catch (Exception ex)
			{
				_logger.LogInformation($"타입 \'{ex.GetType().FullName}\'의 에러가 발생했습니다: {ex.Message}");
			}
		}
	}
}
