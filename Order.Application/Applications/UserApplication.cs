using AutoMapper;
using Order.Application.DataContract.Request.User;
using Order.Application.DataContract.Response.Client;
using Order.Application.DataContract.Response.User;
using Order.Application.Interfacds;
using Order.Application.Interfacds.Security;
using Order.Domain.Interfaces.Services;
using Order.Domain.Models;
using Order.Domain.Validations.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Application.Applications
{
    public class UserApplication : IUserApplication
    {
        private readonly IUserService _UserService;
        private readonly IMapper _mapper;
        private readonly ISecurityService _securityService;
        private readonly ITokenManager _tokenManager;

        public UserApplication(IUserService UserService, IMapper mapper, ISecurityService securityService, ITokenManager tokenManager)
        {
            _UserService = UserService;
            _mapper = mapper;
            _securityService = securityService;
            _tokenManager = tokenManager;
        }

        public async Task<Response<AuthResponse>> AuthAsync(AuthRequest auth)
        {
            var user = await _UserService.GetByLoginAsync(auth.Email);

            if (user.Report.Any())
            {
                return Response.Unprocessable<AuthResponse>(user.Report);
            }

            var isAuthenticated = await _UserService.AutheticationAsync(auth.Password, user.Data);

            if (!isAuthenticated.Data)
            {
                return Response.Unprocessable<AuthResponse>(new List<Report>() { Report.Create("Invalid password or email") });
            }

            var token = await _tokenManager.GenerateTokenAsync(user.Data);

            return new Response<AuthResponse>(token);
        }

        public async Task<Response> CreateAsync(CreateUserRequest user)
        {
            try
            {
                var isEquals = await _securityService.ComparePassword(user.Password, user.ConfirmPassword);

                if (!isEquals.Data)
                {
                    return Response.Unprocessable(Report.Create("Passwords do not match"));
                }

                var passwordEncripted = await _securityService.EncryptPassword(user.Password);

                user.Password = passwordEncripted.Data;

                var userModel = _mapper.Map<UserModel>(user);

                return await _UserService.CreateAsync(userModel);
            }
            catch (Exception ex)
            {
                var response = Report.Create(ex.Message);

                return Response.Unprocessable(response);
            }
        }

        public async Task<Response> CodeVerification(CodeVerificationRequest request)
        {
            try
            {
                var codeExists = await _UserService.CodeVerification(request.Email, request.CodeVerification);

                if (codeExists.Report.Any())
                {
                    return Response.Unprocessable(codeExists.Report);
                }

                return Response.OK(codeExists);
            }
            catch (Exception ex)
            {
                var response = Report.Create(ex.Message);

                return Response.Unprocessable(response);
            }
        }

        public async Task<Response<List<UserResponse>>> ListByFilterAsync(string userId = null, string email = null)
        {
            try
            {
                Response<List<UserModel>> user = await _UserService.ListByFilterAsync(userId, email);

                if (user.Report.Any())
                {
                    return Response.Unprocessable<List<UserResponse>>(user.Report);
                }

                var response = _mapper.Map<List<UserResponse>>(user.Data);

                return Response.OK(response);
            }
            catch (Exception ex)
            {
                var response = Report.Create(ex.Message);

                return Response.Unprocessable<List<UserResponse>>(new List<Report>() { response });
            }
        }

        public async Task<Response> UpdateAsync(CreateUserRequest user)
        {
            try
            {
                var isEquals = await _securityService.ComparePassword(user.Password, user.ConfirmPassword);

                if (!isEquals.Data)
                {
                    return Response.Unprocessable(Report.Create("Passwords do not match"));
                }

                var passwordEncripted = await _securityService.EncryptPassword(user.Password);

                user.Password = passwordEncripted.Data;

                var userModel = _mapper.Map<UserModel>(user);

                return await _UserService.UpdateAsync(userModel);
            }
            catch (Exception ex)
            {

                var response = Report.Create(ex.Message);

                return Response.Unprocessable(response);
            }
        }
    }
}
