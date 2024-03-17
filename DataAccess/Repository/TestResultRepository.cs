using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class TestResultRepository : Repository<TestResults>, ITestResultRepository
    {
        private readonly ApplicationDbContext _db;

        public TestResultRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(TestResults test)
        {
            var obj = _db.TestResults.FirstOrDefault(x => x.Id ==test.Id);
            if (obj != null)
            {
                obj = test;
            }
        }
    }
}