using Watchdog.Application.DTOs.Auth;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.Auth
{
    public class LoginUseCase : IUseCaseAsync<LoginRequest, LoginResponse>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IPasswordHasher _passwordHasher;

        public LoginUseCase(
            IAuthRepository authRepository,
            IJwtTokenGenerator tokenGenerator,
            IPasswordHasher passwordHasher)
        {
            _authRepository = authRepository;
            _tokenGenerator = tokenGenerator;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResponse> ExecuteAsync(LoginRequest request)
        {
            var user = await _authRepository.GetUserByUsernameAsync(request.Username);

            if (user == null)
            {
                return new LoginResponse { IsSuccess = false, ErrorMessage = "Kullanıcı adı veya şifre hatalı." };
            }

            var passwordVerificationResult = _passwordHasher.VerifyPassword(user.PasswordHash, request.Password);

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return new LoginResponse { IsSuccess = false, ErrorMessage = "Kullanıcı adı veya şifre hatalı." };
            }

            if (passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(request.Password);
                await _authRepository.UpdateUserAsync(user);
            }

            var token = _tokenGenerator.GenerateToken(user);
            return new LoginResponse { IsSuccess = true, Token = token };
        }
    }
}
