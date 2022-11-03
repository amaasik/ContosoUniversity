using ContosoUniversity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ContosoUniversity.Controllers
{
    public class CoursesController : Controller
    {
        private readonly SchoolContext _context;
        public CoursesController
            (
            SchoolContext context
            )
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Include(c => c.Department)
                .AsNoTracking()
                .ToListAsync();

            return View(courses);
        }
    }
}
