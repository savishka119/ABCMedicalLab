using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class ApplicationRoleRepository : Repository<ApplicationRole>, IApplicationRoleRepository
    {
        private readonly ApplicationDbContext _db;

        public ApplicationRoleRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}