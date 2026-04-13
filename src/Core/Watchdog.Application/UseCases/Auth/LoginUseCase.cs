using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Watchdog.Application.DTOs.Auth;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.Auth
{
    public class LoginUseCase : IUseCaseAsync<LoginRequest, LoginResponse>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IJwtTokenGenerator _tokenGenerator;

        // YENİ EKLENDİ: Merkezi şifreleme servisimizi Dependency Injection ile içeri alıyoruz
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

            // DİKKAT: Eski kodda burada private bir metot vardı. Onu sildik.
            // Artık kurumsal, merkezi _passwordHasher servisimizi kullanıyoruz.
            var incomingHash = _passwordHasher.HashPassword(request.Password);

            if (user.PasswordHash != incomingHash)
            {
                return new LoginResponse { IsSuccess = false, ErrorMessage = "Kullanıcı adı veya şifre hatalı." };
            }

            // Giriş başarılı, bileti kes!
            var token = _tokenGenerator.GenerateToken(user);
            return new LoginResponse { IsSuccess = true, Token = token };
        }
    }
}
