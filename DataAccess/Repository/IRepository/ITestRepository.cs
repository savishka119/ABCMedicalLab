using Models;

namespace DataAccess.Repository.IRepository
{
    public interface ITestRepository : IRepository<Test>
    {
        void Update(Test test);
    }
}