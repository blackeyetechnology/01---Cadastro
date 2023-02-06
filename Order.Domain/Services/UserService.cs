using Order.Domain.Common;
using Order.Domain.Interfaces.Repositories;
using Order.Domain.Interfaces.Services;
using Order.Domain.Models;
using Order.Domain.Validations;
using Order.Domain.Validations.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _UserRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly IGenerators _generators;
        private readonly ISecurityService _securityService;

        public UserService(IUserRepository UserRepository,
                           ITimeProvider timeProvider,
                           IGenerators generators, 
                           ISecurityService securityService)
        {
            _UserRepository = UserRepository;
            _timeProvider = timeProvider;
            _generators = generators;
            _securityService = securityService;
        }

        public async Task<Response<bool>> AutheticationAsync(string password, UserModel user)
        {
            return await _securityService.VerifyPassword(password, user);
        }

        public async Task<Response> CodeVerification(string email, string codeVerification)
        {
            var response = new Response();
            var verification = await _UserRepository.CodeVerification(email, codeVerification);

            if (!verification)
            {
                response.Report.Add(Report.Create("The verification code entered is invalid"));
                return response;
            }

            return response;
        }

        public async Task<Response> CreateAsync(UserModel user)
        {
            var response = new Response();

            var validation = new UserValidation();
            var errors = validation.Validate(user).GetErrors();

            if (errors.Report.Count > 0)
            {
                return errors;
            }

            if (await _UserRepository.CheckRegisteredEmail(user.Email))
            {
                response.Report.Add(Report.Create($"We found an email {user.Email} already registered in our system"));
                return response;
            }

            user.Id = _generators.Generate();
            user.CreatedAt = _timeProvider.utcDateTime();

            await _UserRepository.CreateAsync(user);

            try
            {
                await _UserRepository.SendEmail(user.Email, user.VerificationCode.ToString());
            }
            catch (Exception)
            {
                var resp = Report.Create("There was an error sending the email");

                return Response.Unprocessable(resp);
            }

            return response;
        }

        public async Task<Response> DeleteAsync(string userId)
        {
            var response = new Response();

            var exists = await _UserRepository.ExistsByIdAsync(userId);

            if (!exists)
            {
                response.Report.Add(Report.Create($"User {userId} not exists!"));
                return response;
            }

            await _UserRepository.DeleteAsync(userId);

            return response;
        }

        public async Task<Response<UserModel>> GetByIdAsync(string userId)
        {
            var response = new Response<UserModel>();

            var exists = await _UserRepository.ExistsByIdAsync(userId);

            if (!exists)
            {
                response.Report.Add(Report.Create($"User {userId} not exists!"));
                return response;
            }

            var data = await _UserRepository.GetByIdAsync(userId);
            response.Data = data;
            return response;
        }

        public async Task<Response<UserModel>> GetByLoginAsync(string email)
        {
            var response = new Response<UserModel>();

            var exists = await _UserRepository.ExistsByLoginAsync(email);

            if (!exists)
            {
                response.Report.Add(Report.Create($"User {email} not exists!"));
                return response;
            }

            var data = await _UserRepository.GetByLoginAsync(email);
            response.Data = data;
            return response;
        }

        public async Task<Response<List<UserModel>>> ListByFilterAsync(string userId = null, string email = null)
        {
            var response = new Response<List<UserModel>>();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                var exists = await _UserRepository.ExistsByIdAsync(userId);

                if (!exists)
                {
                    response.Report.Add(Report.Create($"User {userId} not exists!"));
                    return response;
                }
            }

            var data = await _UserRepository.ListByFilterAsync(email);
            response.Data = data;

            return response;
        }

        public async Task<Response> UpdateAsync(UserModel user)
        {
            var response = new Response();

            var validation = new UserValidation();
            var errors = validation.Validate(user).GetErrors();

            if (errors.Report.Count > 0)
            {
                return errors;
            }

            var exists = await _UserRepository.CheckRegisteredEmail(user.Email);

            if (!exists)
            {
                response.Report.Add(Report.Create($"User {user.Email} not exists!"));
                return response;
            }

            await _UserRepository.UpdateAsync(user);

            return response;
        }
    }
}
