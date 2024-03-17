using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class OrderRepository : Repository<Orders>, IOrderRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Orders orders)
        {
            var obj = _db.Orders.FirstOrDefault(x => x.Id ==orders.Id);
            if (obj != null)
            {
                obj =orders;
            }
        }
    }
}