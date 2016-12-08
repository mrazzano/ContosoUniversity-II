using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.Generic;
using ContosoUniversity.Core.Models;
using ContosoUniversity.Core.Repository;
using ContosoUniversity.Infrastructure.Database;

namespace ContosoUniversity.Infrastructure.Repository
{
    public class InstructorRepository : IInstructorRepository
    {
        private readonly SchoolContext _dbContext;

        public InstructorRepository(SchoolContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Instructor>> GetAsync()
        {
            return await _dbContext.Instructors
                .Include(i=>i.OfficeAssignment)
                .ToListAsync();
        }

        public async Task<Instructor> GetAsync(int id)
        {
            return await _dbContext.Instructors
                .Where(i => i.Id == id)
                .Include(i => i.Courses)
                .Include(i => i.Courses.Select(c => c.Department))
                .Include(i=>i.OfficeAssignment)
                .SingleOrDefaultAsync();
        }

        public void Add(Instructor entity)
        {
            _dbContext.Instructors.Add(entity);
        }

        public void Update(Instructor entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }
        
        public void Delete(int id)
        {
            var entity = _dbContext.Instructors.Find(id);
            if (entity.OfficeAssignment != null)
            {
                entity.OfficeAssignment = null;
            }
            _dbContext.Instructors.Remove(entity);
        }
    }
}
