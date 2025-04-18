using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Areas.ProjectManager.Models;
using WorkTrackingSystem.Models;
using X.PagedList.Extensions;

namespace WorkTrackingSystem.Areas.ProjectManager.Controllers
{
    [Area("ProjectManager")]
    public class EmployeesController : Controller
    {
        private readonly WorkTrackingSystemContext _context;

        public EmployeesController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: ProjectManager/Employees
        public async Task<IActionResult> Index(string search, int? positionId, int page = 1)
        {
            var limit = 10;

            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var manager = await _context.Users
                .Where(u => u.UserName == managerUsername)
                .Select(u => u.Employee)
                .FirstOrDefaultAsync();

            if (manager == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Tải dữ liệu từ database trước khi gọi ToPagedList
            var managedDepartments = await _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
                .Select(d => d.Id)
                .ToListAsync(); // Chuyển thành danh sách thực tế

            var employeesQuery = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value)
                            && (e.IsDelete == false || e.IsDelete == null));

            if (positionId.HasValue && positionId > 0)
            {
                employeesQuery = employeesQuery.Where(e => e.PositionId == positionId.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                employeesQuery = employeesQuery.Where(e =>
                    e.Code.ToLower().Contains(search) ||
                    (e.FirstName != null && e.FirstName.ToLower().Contains(search)) ||
                    (e.LastName != null && e.LastName.ToLower().Contains(search)));
            }

            // Lấy danh sách nhân viên từ database trước khi phân trang
            var employees = await employeesQuery.ToListAsync();
            var pagedEmployees = employees.ToPagedList(page, limit); // Phân trang trên danh sách đã tải

            //       ViewBag.Positions = _context.Positions
            //.Select(p => new SelectListItem
            //{
            //    Value = p.Id.ToString(),
            //    Text = p.Name
            //})
            //.ToList();
            ViewBag.Search = search;
            ViewBag.positionId = positionId;
            ViewBag.Positions = new SelectList(_context.Positions, "Id", "Name");
            return View(pagedEmployees); // Trả về IPagedList<Employee>
        }
        public async Task<IActionResult> AnalysesEmployees(string search, int? positionId, string timeType, DateTime? fromDate, DateTime? toDate, string time, int? quarter, int? quarterYear, int? year, string sortOrder, int page = 1)
        {
            var limit = 10;
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var manager = await _context.Users
                .Where(u => u.UserName == managerUsername)
                .Select(u => u.Employee)
                .FirstOrDefaultAsync();

            if (manager == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy danh sách phòng ban mà manager quản lý
            var managedDepartments = await _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
                .Select(d => d.Id)
                .ToListAsync();

            // Xử lý tham số thời gian
            int? month = null;
            int? monthYear = null;

            if (timeType == "month" && !string.IsNullOrEmpty(time) && DateTime.TryParse(time + "-01", out var parsedTime))
            {
                month = parsedTime.Month;
                monthYear = parsedTime.Year;
                fromDate = new DateTime(parsedTime.Year, parsedTime.Month, 1);
                toDate = fromDate.Value.AddMonths(1).AddDays(-1);
            }
            else if (timeType == "quarter" && quarter.HasValue && quarterYear.HasValue)
            {
                fromDate = new DateTime(quarterYear.Value, (quarter.Value - 1) * 3 + 1, 1);
                toDate = fromDate.Value.AddMonths(3).AddDays(-1);
            }
            else if (timeType == "year" && year.HasValue)
            {
                fromDate = new DateTime(year.Value, 1, 1);
                toDate = new DateTime(year.Value, 12, 31);
            }
            else if (timeType != "dateRange")
            {
                // For "total", clear all time parameters
                fromDate = null;
                toDate = null;
                quarter = null;
                quarterYear = null;
                month = null;
                monthYear = null;
                year = null;
            }

            // Gọi stored procedure với các tham số
            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@DepartmentId", managedDepartments.FirstOrDefault()),
        new SqlParameter("@FromDate", (object)fromDate ?? DBNull.Value),
        new SqlParameter("@ToDate", (object)toDate ?? DBNull.Value),
        new SqlParameter("@Quarter", (object)quarter ?? DBNull.Value),
        new SqlParameter("@QuarterYear", (object)quarterYear ?? DBNull.Value),
        new SqlParameter("@Month", (object)month ?? DBNull.Value),
        new SqlParameter("@MonthYear", (object)monthYear ?? DBNull.Value),
        new SqlParameter("@Year", (object)year ?? DBNull.Value)
    };

            var employeeScores = await _context.Set<EmployeeScoreSummary>()
                .FromSqlRaw("EXEC sp_GetEmployeeScoreSummary @DepartmentId, @FromDate, @ToDate, @Quarter, @QuarterYear, @Month, @MonthYear, @Year", parameters.ToArray())
                .ToListAsync();

            // Lọc thêm nếu có search
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                employeeScores = employeeScores
                    .Where(e => e.EmployeeName.ToLower().Contains(search))
                    .ToList();
            }

            // Áp dụng sắp xếp
            switch (sortOrder)
            {
                case "total_asc":
                    employeeScores = employeeScores.OrderBy(e => e.TotalJobs).ToList();
                    break;
                case "total_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.TotalJobs).ToList();
                    break;
                case "ontime_asc":
                    employeeScores = employeeScores.OrderBy(e => e.OnTimeCount).ToList();
                    break;
                case "ontime_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.OnTimeCount).ToList();
                    break;
                case "late_asc":
                    employeeScores = employeeScores.OrderBy(e => e.LateCount).ToList();
                    break;
                case "late_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.LateCount).ToList();
                    break;
                case "overdue_asc":
                    employeeScores = employeeScores.OrderBy(e => e.OverdueCount).ToList();
                    break;
                case "overdue_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.OverdueCount).ToList();
                    break;
                case "processing_asc":
                    employeeScores = employeeScores.OrderBy(e => e.ProcessingCount).ToList();
                    break;
                case "processing_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.ProcessingCount).ToList();
                    break;
                default:
                    employeeScores = employeeScores.OrderBy(e => e.EmployeeName).ToList();
                    break;
            }

            // Phân trang
            var pagedEmployeeScores = employeeScores.ToPagedList(page, limit);

            // Chuẩn bị dữ liệu cho view
            ViewBag.Search = search;
            ViewBag.PositionId = positionId;
            ViewBag.TimeType = timeType ?? "total";
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Time = time;
            ViewBag.Quarter = quarter;
            ViewBag.QuarterYear = quarterYear;
            ViewBag.Year = year;
            ViewBag.SortOrder = sortOrder;
            ViewBag.DepartmentName = _context.Departments
                .Where(d => d.Id == managedDepartments.FirstOrDefault())
                .Select(d => d.Name)
                .FirstOrDefault() ?? "All Departments";
            ViewBag.Departments = new SelectList(_context.Departments.Where(d => managedDepartments.Contains(d.Id)), "Id", "Name");
            ViewBag.Positions = new SelectList(_context.Positions, "Id", "Name");

            return View(pagedEmployeeScores);
        }
        public async Task<IActionResult> Details(int? page, long? id)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Lấy danh sách công việc liên quan đến nhân viên và lưu vào ViewBag
            var jobMaps = await _context.Jobmapemployees
                         .Where(jm => jm.EmployeeId == id && jm.IsDelete != true)
                         .Include(jm => jm.Job)
                             .ThenInclude(j => j.Category)
                         .Include(jm => jm.Scores)
                         .ToListAsync(); // Tải danh sách trước

            var pagedJobMaps = jobMaps.ToPagedList(pageNumber, pageSize); // Sau đó mới phân trang

            ViewBag.JobMaps = pagedJobMaps;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details", employee);
            }

            return View(employee);
        }

        // GET: ProjectManager/Employees/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id");
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Id");
            return View();
        }

        // POST: ProjectManager/Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DepartmentId,PositionId,Code,FirstName,LastName,Gender,Birthday,Phone,Email,HireDate,Address,Avatar,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Id", employee.PositionId);
            return View(employee);
        }

        // GET: ProjectManager/Employees/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Id", employee.PositionId);
            return View(employee);
        }

        // POST: ProjectManager/Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,DepartmentId,PositionId,Code,FirstName,LastName,Gender,Birthday,Phone,Email,HireDate,Address,Avatar,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Id", employee.PositionId);
            return View(employee);
        }

        // GET: ProjectManager/Employees/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: ProjectManager/Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(long id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
