using Models;

namespace DataAccess.Repository.IRepository
{
    public interface ITestResultRepository : IRepository<TestResults>
    {
        void Update(TestResults testResults);
    }
}