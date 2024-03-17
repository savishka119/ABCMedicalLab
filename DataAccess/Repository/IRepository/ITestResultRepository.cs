using Models;

namespace DataAccess.Repository.IRepository
{
    public interface IOrderRepository : IRepository<Orders>
    {
        void Update(Orders orders);
    }
}