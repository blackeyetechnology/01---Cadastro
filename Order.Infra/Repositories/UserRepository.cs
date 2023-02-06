using Microsoft.Extensions.Configuration;
using Dapper;
using Order.Domain.Interfaces.Repositories;
using Order.Domain.Interfaces.Repositories.DataConnector;
using Order.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Net;
using System.Runtime.CompilerServices;
using System.Configuration;
using System.Web.Mvc;
using System.Diagnostics.Contracts;

namespace Order.Infra.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnector _dbConnector;
        private readonly IConfiguration _configuration;

        public UserRepository(IDbConnector dbConnector, IConfiguration configuration)
        {
            _dbConnector = dbConnector;
            _configuration = configuration;
        }

        const string baseSql = @"SELECT [Id]
                                      ,[Email]
                                      ,[PasswordHash]
                                      ,[CreatedAt]
                                      ,[ActiveUser]
                                  FROM [dbo].[Users]
                                  WHERE 1 = 1 ";

        public async Task<bool> CheckRegisteredEmail(string email)
        {
            string sql = @"SELECT 2 FROM Users WHERE Email = @Email";

            var emailExists = await _dbConnector.dbConnection.QueryAsync<bool>(sql, new { Email = email }, _dbConnector.dbTransaction);

            return emailExists.FirstOrDefault();
        }

        public async Task CreateAsync(UserModel user)
        {
            int min = 1000;
            int max = 9999;

            Random code = new Random();
            int codeVerification = code.Next(min, max);

            user.ActiveUser = "Inativo";
            user.VerificationCode = codeVerification;

            string sql = @"INSERT INTO [dbo].[Users]
                                ([Id]
                                ,[Email]
                                ,[PasswordHash]
                                ,[CreatedAt]
                                ,[ActiveUser]
                                ,[VerificationCode])
                          VALUES
                                (@Id
                                ,@Email
                                ,@PasswordHash
                                ,@CreatedAt
                                ,@ActiveUser
                                ,@VerificationCode)";

            await _dbConnector.dbConnection.ExecuteAsync(sql, new
            {
                Id = user.Id,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                CreatedAt = user.CreatedAt,
                ActiveUser = user.ActiveUser,
                VerificationCode = user.VerificationCode
            }, _dbConnector.dbTransaction);
        }

        public async Task SendEmail(string email, string codeVerification)
        {
            string sql = $"{baseSql} AND Email = @Email";

            var user = await _dbConnector.dbConnection.QueryAsync<UserModel>(sql, new { Email = email }, _dbConnector.dbTransaction);

            string host = _configuration["SMTP:Host"];
            string name = _configuration["SMTP:Name"];
            string userName = _configuration["SMTP:UserName"];
            string passswordEmail = _configuration["SMTP:Senha"];
            string door = _configuration["SMTP:Porta"];

            MailMessage mail = new MailMessage()
            {
                From = new MailAddress(userName, name)
            };

            mail.To.Add(user.FirstOrDefault().Email);
            mail.Subject = "Código de Verificação";
            mail.Body = $"Códgio de Verificação: {codeVerification}";
            mail.IsBodyHtml = true;
            //email.Priority = MailPriority.High;

            using (SmtpClient smtpClient = new SmtpClient(host, Convert.ToInt32(door)))
            {
                smtpClient.Credentials = new NetworkCredential(userName, passswordEmail);
                smtpClient.EnableSsl = true;

                smtpClient.Send(mail);
            }
        }

        public async Task<bool> CodeVerification(string email, string codeVerification)
        {
            string sqlUser = $"{baseSql} AND Email = @Email";

            string sqlEmail = @"SELECT 4 FROM Users WHERE Email = @Email AND VerificationCode = @VerificationCode";

            string sqlUserActive = @"UPDATE [dbo].[Users]
                                    SET [ActiveUser] = @ActiveUser
                                  WHERE Email = @Email";

            var users = await _dbConnector.dbConnection.QueryAsync<bool>(sqlEmail, new { Email = email, VerificationCode = codeVerification }, _dbConnector.dbTransaction);

            if (users.FirstOrDefault())
            {
                var userUpdate = await _dbConnector.dbConnection.QueryAsync<UserModel>(sqlUser, new { Email = email }, _dbConnector.dbTransaction);

                userUpdate.FirstOrDefault().ActiveUser = "Ativo";

                await _dbConnector.dbConnection.ExecuteAsync(sqlUserActive, new
                {
                    Id = userUpdate.FirstOrDefault().Id,
                    Email = userUpdate.FirstOrDefault().Email,
                    PasswordHash = userUpdate.FirstOrDefault().PasswordHash,
                    VerificationCode = userUpdate.FirstOrDefault().VerificationCode,
                    ActiveUser = userUpdate.FirstOrDefault().ActiveUser,

                }, _dbConnector.dbTransaction);
            }

            return users.FirstOrDefault();
        }

        public async Task UpdateAsync(UserModel user)
        {
            string sql = @"UPDATE [dbo].[Users]
                             SET [Email] = @Email
                                ,[PasswordHash] = @PasswordHash
                           WHERE Email = @Email";

            await _dbConnector.dbConnection.ExecuteAsync(sql, new
            {
                Id = user.Id,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                VerificationCode = user.VerificationCode,
                ActiveUser = user.ActiveUser,
                CreatedAt = user.CreatedAt
            }, _dbConnector.dbTransaction);
        }

        public async Task DeleteAsync(string userId)
        {
            string sql = $"DELETE FROM [dbo].[Users] WHERE id = @id";

            await _dbConnector.dbConnection.ExecuteAsync(sql, new { Id = userId }, _dbConnector.dbTransaction);
        }

        public async Task<bool> ExistsByIdAsync(string userId)
        {
            string sql = $"SELECT 1 FROM Users WHERE Id = @Id ";

            var users = await _dbConnector.dbConnection.QueryAsync<bool>(sql, new { Id = userId }, _dbConnector.dbTransaction);

            return users.FirstOrDefault();
        }

        public async Task<bool> ExistsByLoginAsync(string email)
        {
            string sql = $"SELECT 1 FROM [Users] WHERE Email = @Email ";
                
            var users = await _dbConnector.dbConnection.QueryAsync<bool>(sql, new { Email = email }, _dbConnector.dbTransaction);

            return users.FirstOrDefault();
        }

        public async Task<UserModel> GetByIdAsync(string userId)
        {
            string sql = $"{baseSql} AND Id = @Id";

            var users = await _dbConnector.dbConnection.QueryAsync<UserModel>(sql, new { Id = userId }, _dbConnector.dbTransaction);

            return users.FirstOrDefault();
        }

        public async Task<List<UserModel>> ListByFilterAsync( string email = null)
        {
            string sql = $"{baseSql} ";

            if (!string.IsNullOrWhiteSpace(email))
            {
                sql += "AND Email = @Email";
            }

            var users = await _dbConnector.dbConnection.QueryAsync<UserModel>(sql, new { Email = email }, _dbConnector.dbTransaction);

            return users.ToList();
        }

        public async Task<UserModel> GetByLoginAsync(string email)
        {
            string sql = $"{baseSql} AND Email = @Email";

            var users = await _dbConnector.dbConnection.QueryAsync<UserModel>(sql, new { Email = email }, _dbConnector.dbTransaction);

            return users.FirstOrDefault();
        }
    }
}
