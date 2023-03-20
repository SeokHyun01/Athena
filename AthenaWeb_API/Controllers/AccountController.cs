using Athena_Common;
using Athena_DataAccess;
using Athena_Models;
using AthenaWeb_API.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AthenaWeb_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class AccountController : Controller
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ILogger<AccountController> _logger;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly APISettings _apiSettings;


		public AccountController(SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			ILogger<AccountController> logger,
			RoleManager<IdentityRole> roleManager,
			IOptions<APISettings> options)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_logger = logger;
			_roleManager = roleManager;
			_apiSettings = options.Value;
		}

		[HttpPost]
		public async ValueTask<IActionResult> SignUp([FromBody] SignUpRequestDTO signUpRequest)
		{
			if (signUpRequest == null || !ModelState.IsValid)
			{
				return BadRequest();
			}

			var user = new ApplicationUser
			{
				UserName = signUpRequest.Email,
				Email = signUpRequest.Email,
				EmailConfirmed = true,
				FCMKey = new HashSet<string>()
				{
					signUpRequest.FCMKey
				}	
			};
			var result = await _userManager.CreateAsync(user, signUpRequest.Password);
			if (!result.Succeeded)
			{
				return BadRequest(new SignUpResponseDTO()
				{
					IsSucceeded = false,
					Errors = result.Errors.Select(u => u.Description)
				});
			}

			var roleResult = await _userManager.AddToRoleAsync(user, SD.ROLE_CUSTOMER);
			if (!roleResult.Succeeded)
			{
				return BadRequest(new SignUpResponseDTO()
				{
					IsSucceeded = false,
					Errors = result.Errors.Select(u => u.Description)
				});
			}

			return Ok(201);
		}

		[HttpPost]
		public async ValueTask<IActionResult> SignIn([FromBody] SignInRequestDTO signInRequest)
		{
			if (signInRequest == null || !ModelState.IsValid)
			{
				return BadRequest();
			}

			var result = await _signInManager.PasswordSignInAsync(signInRequest.Email, signInRequest.Password, false, false);
			if (result.Succeeded)
			{
				var user = await _userManager.FindByNameAsync(signInRequest.Email);
				if (user == null)
				{
					return Unauthorized(new SignInResponseDTO
					{
						IsSucceeded = false,
						ErrorMessage = "유저를 찾을 수 없습니다."
					});
				}

				user.FCMKey.Add(signInRequest.FCMKey);
				await _userManager.UpdateAsync(user);

				var claims = await GetClaims(user);
				var signInCredentials = GetSigningCredentials();
				var tokenOptions = new JwtSecurityToken(
					issuer: _apiSettings.ValidIssuer,
					audience: _apiSettings.ValidAudience,
					claims: claims,
					expires: DateTime.Now.AddDays(30),
					signingCredentials: signInCredentials);
				var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

				return Ok(new SignInResponseDTO()
				{
					IsSucceeded = true,
					Token = token,
					UserDTO = new UserDTO()
					{
						Id = user.Id,
						Name = user.UserName,
						Email = user.Email,
					}
				});
			}
			else
			{
				return Unauthorized(new SignInResponseDTO
				{
					IsSucceeded = false,
					ErrorMessage = "Id 또는 비밀번호를 잘못 입력했습니다."
				});
			}
		}

		private SigningCredentials GetSigningCredentials()
		{
			var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSettings.SecretKey));

			return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
		}

		private async ValueTask<List<Claim>> GetClaims(IdentityUser user)
		{
			var claims = new List<Claim>
			{
				new Claim("Id", user.Id),
				new Claim(ClaimTypes.Name, user.Email),
				new Claim(ClaimTypes.Email, user.Email),
			};

			var roles = await _userManager.GetRolesAsync(await _userManager.FindByEmailAsync(user.Email));
			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			return claims;
		}
	}
}
