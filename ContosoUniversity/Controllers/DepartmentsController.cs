using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly SchoolContext _context;
        public DepartmentsController
          (
            SchoolContext context
          )
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _context.Departments
                .Include(d => d.Administrator)
                .ToListAsync();

            return View(result);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();

            }
            var department = await _context.Departments
                .Include(i => i.Administrator)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "FullName",
                department.InstructorId);
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, byte[] rowVersion)
        {
            if (id == null)
            {
                return NotFound();
            }
            var departmentToUpdate = await _context.Departments
                .Include(i => i.Administrator)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (departmentToUpdate == null)
            {
                Department deleteDepartment = new Department();
                await TryUpdateModelAsync(deleteDepartment);
                ModelState.AddModelError(string.Empty,
                    "Unable to save changes. The department was deleted by another user.");
                ViewData["InstructorId"] = new SelectList(_context.Instructors,
                    "Id", "FullName", deleteDepartment.InstructorId);
                return View(deleteDepartment);
            }
            _context.Entry(departmentToUpdate)
                .Property("RowVersion")
                .OriginalValue = rowVersion;
            if (await TryUpdateModelAsync<Department>(departmentToUpdate, "",
                s => s.Name, s => s.StartDate, s => s.Budget, s => s.InstructorId))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exeptionEntry = ex.Entries.Single();
                    var clientValues = (Department)exeptionEntry.Entity;
                    var databaseEntry = exeptionEntry.GetDatabaseValues();
                    if (databaseEntry != null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to save changes. The department was deleted by another user.");
                    }
                    else
                    {

                        var databaseValues = (Department)databaseEntry.ToObject();

                        if (databaseValues.Name != clientValues.Name)
                        {
                            ModelState.AddModelError("Name", $"Current value: {databaseValues.Name}");
                        }
                        if (databaseValues.Budget != clientValues.Budget)
                        {
                            ModelState.AddModelError("Budget", $"Current value: {databaseValues.Budget:c}");
                        }
                        if (databaseValues.StartDate != clientValues.StartDate)
                        {
                            ModelState.AddModelError("StartDate", $"Current value: {databaseValues.StartDate:d}");
                        }
                        if (databaseValues.InstructorId != clientValues.InstructorId)
                        {
                            Instructor databaseInstructor = await _context.Instructors.FirstOrDefaultAsync(i => i.Id == databaseValues.InstructorId);
                            ModelState.AddModelError("InstructorID", $"Current value: {databaseInstructor?.FullName}");
                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                + "was modified by another user after you got the original value. The "
                                + "edit operation was canceled and the current values in the database "
                                + "have been displayed. If you still want to edit this record, click "
                                + "the Save button again. Otherwise click the Back to List hyperlink.");
                        departmentToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");

                    }
                }
            }

            ViewData["InstructorID"] = new SelectList(_context.Instructors, "ID", "FullName", departmentToUpdate.InstructorId);
            return View(departmentToUpdate);

        }
        public async Task<IActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(i => i.Administrator)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                if (concurrencyError.GetValueOrDefault())
                {
                    return RedirectToAction(nameof(Index));
                }
                return NotFound();
            }
            if (concurrencyError.GetValueOrDefault())
            {
                ViewData["ConcurrencyErrorMessage"] = "The record you attempted to delete "
                    + "was modified by another user after you got the original values. "
                    + "The delete operation was canceled and the current values in the "
                    + "database have been displayed. If you still want to delete this "
                    + "record, click the Delete button again. Otherwise "
                    + "click the Back to List hyperlink.";
            }

            return View(department);
        }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Delete(Department department)
            {
                try
                {
                    if (await _context.Departments.AnyAsync(m => m.Id == department.Id))
                    {
                        _context.Departments.Remove(department);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    return RedirectToAction(nameof(Delete), new { concurrencyError = true, id = department.Id });
                }
            }

        public async Task<IActionResult>Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string query = "SELECT * FROM Department WHERE Id = {0}";
            var department = await _context.Departments
                .FromSqlRaw(query, id)
                .Include(d => d.Administrator)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        public IActionResult Create()
        {
            ViewData["InstructorID"] = new SelectList(_context.Instructors, "Id", "FullName");
            return View();
        }

        public async Task<IActionResult> Create([Bind("DepartmentId,Name,Budget,StartDate,InstructorID,RowVersion")] Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "FullName", department.InstructorId);
            return View(department);
        }

        private bool DepartmentExists(int id)
            {
                return _context.Departments.Any(e => e.Id == id);
            }
        }
    }

