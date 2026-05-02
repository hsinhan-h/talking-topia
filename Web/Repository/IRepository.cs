

using System.Linq.Expressions;

namespace Web.Repository
{
    public interface IRepository
    {
        void Create<T>(T value) where T : class;
        void Delete<T>(T value) where T : class;
        void Dispose();
        //T Get<T>(int id) where T : class;
        IQueryable<T> GetAll<T>() where T : class;
        void SaveChanges();
        void Update<T>(T value) where T : class;
        T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class;
        Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        Task SaveChangesAsync();
        Task BeginTransActionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task<Member> GetMemberByIdAsync(int memberId);

        Task<Member> GetMemberByLineIdAsync(string lineId);
        Task<Course> GetCourseByIdAsync(int courseId);
        
    }
}