using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db;

        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ApplicationUser applicationUser)
        {
            var obj = _db.ApplicationUsers.FirstOrDefault(x => x.Id == applicationUser.Id);
            if (obj != null)
            {
                obj.FirstName = applicationUser.FirstName;
                obj.LastName = applicationUser.LastName;
            }
        }
    }
}