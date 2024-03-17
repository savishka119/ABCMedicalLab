using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IApplicationRoleRepository ApplicationRole { get; }
        IApplicationUserRepository ApplicationUser { get; }
        ITestRepository Tests { get; }
        ITestResultRepository TestResult { get; }
     
        IOrderRepository Order { get; }
     
       
      
        ISP_Call SP_Call { get; }
        void SaveAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        void CommitAsync(IDbContextTransaction transaction);
        void RollbackAsync(IDbContextTransaction transaction);
    }
}
