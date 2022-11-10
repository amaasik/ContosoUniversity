using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;

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

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id== null)
            {
                return NotFound();
            }
            var courses = await _context.Courses
                .Include(_c => _c.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if(courses==null)
            {
                return NotFound();
            }
            return View(courses);
        }
        [HttpGet]
        public IActionResult Create()
        {
            PopulateDepartmentsDropDownList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseID, Title, Gredits, DepartmentId")] Course course)
        {
            if(ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateDepartmentsDropDownList(course.DepartmentId);
            return View(course);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if(id== null)
            {
                return NotFound();
            }
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if(course==null)
            {
                return NotFound();
            }
            PopulateDepartmentsDropDownList(course.DepartmentId);
            return View(course);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseToUpdate = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (await TryUpdateModelAsync<Course>(courseToUpdate,
                "",
                c => c.Credits, c => c.DepartmentId, c => c.Title))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    
                }
                catch (DbUpdateException)
                {
                        ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");

                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDepartmentsDropDownList(courseToUpdate.DepartmentId);
            return View(courseToUpdate);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private void PopulateDepartmentsDropDownList(object selectedDepartment = null)
        {
            var depQuery = from d in _context.Departments
                           orderby d.Name
                           select d;
            ViewBag.Id = new SelectList(depQuery.AsNoTracking(), "Id", "Name", selectedDepartment);
        }

    }
}
