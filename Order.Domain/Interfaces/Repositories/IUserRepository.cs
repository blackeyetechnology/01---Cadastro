using Order.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task CreateAsync(UserModel user);
        Task UpdateAsync(UserModel user);
        Task DeleteAsync(string userId);
        Task<UserModel> GetByIdAsync(string userId);
        Task<UserModel> GetByLoginAsync(string Email);
        Task<List<UserModel>> ListByFilterAsync(string email = null);
        Task<bool> ExistsByIdAsync(string userId);
        Task<bool> ExistsByLoginAsync(string email);
        Task<bool> CodeVerification(string email, string codeVerification);
        Task<bool> CheckRegisteredEmail(string email);
        Task SendEmail(string email, string codeVerification);
    }
}
