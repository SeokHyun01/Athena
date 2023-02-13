using Athena_Models;

namespace AthenaWeb_Client.Service.IService
{
	public interface IAuthenticationService
	{
		ValueTask<SignUpResponseDTO> RegisterUser(SignUpRequestDTO signUpRequestDTO);
		ValueTask<SignInResponseDTO> Login(SignInRequestDTO signInRequestDTO);
		ValueTask Logout();
	}
}
