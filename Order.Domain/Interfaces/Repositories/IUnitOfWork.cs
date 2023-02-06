using Microsoft.Extensions.Configuration;
using Order.Domain.Interfaces.Repositories.DataConnector;

namespace Order.Domain.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IClientRepository ClientRepository { get; }
        IOrderRepository OrderRepository { get; }
        IProductRepository ProductRepository { get; }
        IUserRepository UserRepository { get; }

        IDbConnector dbConnector { get;}
        IConfiguration configuration { get; }

        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}
