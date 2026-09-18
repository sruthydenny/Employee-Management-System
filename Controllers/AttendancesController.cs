using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AttendancesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Attendances
        public async Task<IActionResult> Index(DateTime? date)
        {
            var selectedDate = date?.Date ?? DateTime.Today;

            var employees = await _context.Employees
                .OrderBy(e => e.FullName)
                .ToListAsync();

            var attendanceRecords = await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.Date.Date == selectedDate)
                .ToListAsync();

            var attendanceList = employees.Select(employee =>
            {
                var existingRecord = attendanceRecords
                    .FirstOrDefault(a => a.EmployeeId == employee.Id);

                return new Attendance
                {
                    Id = existingRecord?.Id ?? 0,
                    EmployeeId = employee.Id,
                    Employee = employee,
                    Date = selectedDate,
                    IsPresent = existingRecord?.IsPresent ?? true,
                    IsLate = existingRecord?.IsLate ?? false
                };
            }).ToList();

            ViewBag.SelectedDate = selectedDate;

            return View(attendanceList);
        }

        // POST: Attendances/SaveDaily
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDaily(
            DateTime date,
            int[] employeeIds,
            string[] statuses)
        {
            if (employeeIds == null || employeeIds.Length == 0)
            {
                TempData["ErrorMessage"] = "No employees were found.";
                return RedirectToAction(nameof(Index), new { date });
            }

            if (employeeIds.Length != statuses.Length)
            {
                TempData["ErrorMessage"] =
                    "There was a problem saving attendance. Please try again.";

                return RedirectToAction(nameof(Index), new { date });
            }

            var selectedDate = date.Date;

            var existingRecords = await _context.Attendances
                .Where(a => a.Date.Date == selectedDate)
                .ToListAsync();

            for (int i = 0; i < employeeIds.Length; i++)
            {
                var employeeId = employeeIds[i];
                var status = statuses[i];

                var record = existingRecords
                    .FirstOrDefault(a => a.EmployeeId == employeeId);

                if (record == null)
                {
                    record = new Attendance
                    {
                        EmployeeId = employeeId,
                        Date = selectedDate
                    };

                    _context.Attendances.Add(record);
                }

                switch (status)
                {
                    case "Present":
                        record.IsPresent = true;
                        record.IsLate = false;
                        break;

                    case "Late":
                        record.IsPresent = true;
                        record.IsLate = true;
                        break;

                    case "Absent":
                        record.IsPresent = false;
                        record.IsLate = false;
                        break;

                    default:
                        record.IsPresent = true;
                        record.IsLate = false;
                        break;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Attendance saved successfully for {selectedDate:dd MMM yyyy}.";

            return RedirectToAction(nameof(Index), new { date = selectedDate });
        }

        // GET: Attendances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                return NotFound();
            }

            return View(attendance);
        }

        // POST: Attendances/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Attendance attendance)
        {
            if (id != attendance.Id)
            {
                return NotFound();
            }

            var employeeExists = await _context.Employees
                .AnyAsync(e => e.Id == attendance.EmployeeId);

            if (!employeeExists)
            {
                ModelState.AddModelError(
                    "EmployeeId",
                    "Please select a valid employee.");
            }

            if (!ModelState.IsValid)
            {
                return View(attendance);
            }

            try
            {
                _context.Update(attendance);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Attendance record updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AttendanceExists(attendance.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index), new { date = attendance.Date });
        }

        // GET: Attendances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                return NotFound();
            }

            return View(attendance);
        }

        // POST: Attendances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendance = await _context.Attendances
                .FindAsync(id);

            if (attendance != null)
            {
                var date = attendance.Date;

                _context.Attendances.Remove(attendance);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Attendance record deleted successfully.";

                return RedirectToAction(nameof(Index), new { date });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AttendanceExists(int id)
        {
            return _context.Attendances
                .Any(a => a.Id == id);
        }
    }
}