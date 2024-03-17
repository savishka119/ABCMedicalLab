using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class TestRepository : Repository<Test>, ITestRepository
    {
        private readonly ApplicationDbContext _db;

        public TestRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Test test)
        {
            var obj = _db.Tests.FirstOrDefault(x => x.Id ==test.Id);
            if (obj != null)
            {
                obj = test;
            }
        }
    }
}