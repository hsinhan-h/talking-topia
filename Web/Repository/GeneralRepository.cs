using System.Linq.Expressions;
using Web.Data;

namespace Web.Repository
{
    public class GeneralRepository : IRepository
    {
        private readonly TalkingTopiaDbContext _context;
        public GeneralRepository(TalkingTopiaDbContext context)
        {
            _context = context;
        }

        public void Create<T>(T value) where T : class
        {
            _context.Set<T>().Add(value);  // 正確新增實體
        }

        public void Update<T>(T value) where T : class
        {
            _context.Entry(value).State = EntityState.Modified;
        }

        public void Delete<T>(T value) where T : class
        {
            _context.Entry(value).State = EntityState.Deleted;
        }

        //public T Get<T>(int id) where T : class
        //{
        //    return _context.Set<T>().Find(id);
        //}

        public IQueryable<T> GetAll<T>() where T : class
        {
            return _context.Set<T>();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public T FirstOrDefault<T>(Func<T, bool> predicate) where T : class
        {
            return _context.Set<T>().FirstOrDefault(predicate);

        }
        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task BeginTransActionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }
        public async Task RollbackAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return _context.Set<T>().FirstOrDefault(predicate);

        }

        public async Task<Member> GetMemberByIdAsync(int memberId)
        {
            return await _context.Members
            .FirstOrDefaultAsync(m => m.MemberId == memberId);
        }

 

        public async Task<Member> GetMemberByLineIdAsync(string lineId)
        {
            return await _context.Members.FirstOrDefaultAsync(u => u.LineUserId == lineId);

        }

        public async Task<Course> GetCourseByIdAsync(int courseId)
        {
            return await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
        }
    }
}
