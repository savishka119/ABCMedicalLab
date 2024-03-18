using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class QueryRepository : Repository<Query>, IQueryRepository
    {
        private readonly ApplicationDbContext _db;

        public QueryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
       
    }
}