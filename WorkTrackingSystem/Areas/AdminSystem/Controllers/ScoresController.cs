using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.AdminSystem.Controllers
{
    [Area("AdminSystem")]
    public class ScoresController : Controller
    {
        private readonly WorkTrackingSystemContext _context;

        public ScoresController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: AdminSystem/Scores
        public async Task<IActionResult> ExportToExcel(
   string searchText = "",
   string month = "",
   string day = "",
   int? status = null,
   int? categoryId = null,
   bool dueToday = false,
   string sortOrder = "",
   bool showCompletedZeroReview = false)
        {
            //var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            //if (string.IsNullOrEmpty(managerUsername))
            //{
            //    return RedirectToAction("Index", "Login");
            //}

            //var manager = await _context.Users
            //    .Where(u => u.UserName == managerUsername)
            //    .Include(u => u.Employee)
            //    .Select(u => u.Employee)
            //    .FirstOrDefaultAsync();

            //if (manager == null)
            //{
            //    return RedirectToAction("Index", "Login");
            //}

            //var managedDepartments = await _context.Departments
            //    .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
            //    .Select(d => d.Id)
            //    .ToListAsync();

            var employeesInManagedDepartments = await _context.Employees
                .Where(e => e.DepartmentId.HasValue)
                .Select(e => e.Id)
                .ToListAsync();

            var scoresQuery = _context.Scores
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Employee)
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Job)
                        .ThenInclude(j => j.Category)
                .Where(s => s.JobMapEmployee.EmployeeId.HasValue && employeesInManagedDepartments.Contains(s.JobMapEmployee.EmployeeId.Value));

            // Áp dụng các bộ lọc giống như trong JobEaluation

            // 1. Tìm kiếm theo mã / tên nhân viên / công việc
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                scoresQuery = scoresQuery.Where(s =>
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.Code.ToLower().Contains(searchText)) ||
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.FirstName.ToLower().Contains(searchText)) ||
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.LastName.ToLower().Contains(searchText)) ||
                    s.JobMapEmployee.Job.Name.ToLower().Contains(searchText));
            }

            // 2. Lọc theo tháng
            if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out DateTime selectedMonth))
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue &&
                                                  s.CreateDate.Value.Year == selectedMonth.Year &&
                                                  s.CreateDate.Value.Month == selectedMonth.Month);
            }

            // 3. Lọc theo ngày
            if (!string.IsNullOrEmpty(day) && DateTime.TryParse(day, out DateTime selectedDay))
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Date == selectedDay.Date);
            }

            // 4. Lọc theo trạng thái
            if (status.HasValue)
            {
                scoresQuery = scoresQuery.Where(s => s.Status == status.Value);
            }

            // 5. Lọc theo danh mục
            if (categoryId.HasValue)
            {
                scoresQuery = scoresQuery.Where(s => s.JobMapEmployee.Job.CategoryId == categoryId.Value);
            }

            // 6. Hiển thị công việc hoàn thành nhưng chưa đánh giá
            if (showCompletedZeroReview)
            {
                scoresQuery = scoresQuery.Where(s => s.SummaryOfReviews == 0);
            }

            // 7. Lọc công việc theo ngày (dueToday)
            if (dueToday)
            {
                scoresQuery = scoresQuery.Where(s => s.Time == s.CreateDate);
            }

            // 8. Sắp xếp
            switch (sortOrder)
            {
                case "due_asc":
                    scoresQuery = scoresQuery.OrderBy(s => s.CreateDate);
                    break;
                case "due_desc":
                    scoresQuery = scoresQuery.OrderByDescending(s => s.CreateDate);
                    break;
                case "review_asc":
                    scoresQuery = scoresQuery.OrderBy(s => s.SummaryOfReviews);
                    break;
                case "review_desc":
                    scoresQuery = scoresQuery.OrderByDescending(s => s.SummaryOfReviews);
                    break;
                default:
                    scoresQuery = scoresQuery.OrderBy(s => s.CreateDate);
                    break;
            }

            var scoreList = await scoresQuery.ToListAsync();

            // Tạo file Excel bằng EPPlus
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Scores");
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Nhân viên";
                worksheet.Cells[1, 3].Value = "Hạng mục";
                worksheet.Cells[1, 4].Value = "Công việc";
                worksheet.Cells[1, 5].Value = "Ngày tạo";
                worksheet.Cells[1, 6].Value = "Ngày hoàn thành";
                worksheet.Cells[1, 7].Value = "Trạng thái";
                worksheet.Cells[1, 8].Value = "Đánh giá khối lượng";
                worksheet.Cells[1, 9].Value = "Đánh giá tiến độ";
                worksheet.Cells[1, 10].Value = "Đánh giá chất lượng";
                worksheet.Cells[1, 11].Value = "Đánh giá tổng hợp";

                // Điền dữ liệu
                for (int i = 0; i < scoreList.Count; i++)
                {
                    var score = scoreList[i];
                    worksheet.Cells[i + 2, 1].Value = i + 1; // STT
                    worksheet.Cells[i + 2, 2].Value = score.JobMapEmployee.Employee != null
                        ? $"{score.JobMapEmployee.Employee.Code} {score.JobMapEmployee.Employee.FirstName} {score.JobMapEmployee.Employee.LastName}"
                        : "N/A";
                    worksheet.Cells[i + 2, 3].Value = score.JobMapEmployee.Job.Category?.Name ?? "N/A";
                    worksheet.Cells[i + 2, 4].Value = score.JobMapEmployee.Job?.Name ?? "N/A";
                    worksheet.Cells[i + 2, 5].Value = score.CreateDate?.ToString("dd/MM/yyyy");
                    worksheet.Cells[i + 2, 6].Value = score.CompletionDate?.ToString("dd/MM/yyyy");
                    worksheet.Cells[i + 2, 7].Value = score.Status switch
                    {
                        1 => "Hoàn thành",
                        2 => "Chưa hoàn thành",
                        3 => "Hoàn thành muộn",
                        4 => "Đang xử lý",
                        _ => "Chưa bắt đầu"
                    };
                    worksheet.Cells[i + 2, 8].Value = score.VolumeAssessment;
                    worksheet.Cells[i + 2, 9].Value = score.ProgressAssessment;
                    worksheet.Cells[i + 2, 10].Value = score.QualityAssessment;
                    worksheet.Cells[i + 2, 11].Value = score.SummaryOfReviews;
                }

                // Định dạng cột
                worksheet.Cells[1, 1, 1, 11].Style.Font.Bold = true;
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Xuất file Excel
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Scores.xlsx");
            }
        }
        public async Task<IActionResult> Index()
        {
            var workTrackingSystemContext = _context.Scores.Include(s => s.JobMapEmployee);
            return View(await workTrackingSystemContext.ToListAsync());
        }

        // GET: AdminSystem/Scores/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var score = await _context.Scores
                .Include(s => s.JobMapEmployee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (score == null)
            {
                return NotFound();
            }

            return View(score);
        }

        // GET: AdminSystem/Scores/Create
        public IActionResult Create()
        {
            ViewData["JobMapEmployeeId"] = new SelectList(_context.Jobmapemployees, "Id", "Id");
            return View();
        }

        // POST: AdminSystem/Scores/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,JobMapEmployeeId,CompletionDate,Status,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Progress,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Score score)
        {
            if (ModelState.IsValid)
            {
                _context.Add(score);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["JobMapEmployeeId"] = new SelectList(_context.Jobmapemployees, "Id", "Id", score.JobMapEmployeeId);
            return View(score);
        }

        // GET: AdminSystem/Scores/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var score = await _context.Scores.FindAsync(id);
            if (score == null)
            {
                return NotFound();
            }
            ViewData["JobMapEmployeeId"] = new SelectList(_context.Jobmapemployees, "Id", "Id", score.JobMapEmployeeId);
            return View(score);
        }

        // POST: AdminSystem/Scores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,JobMapEmployeeId,CompletionDate,Status,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Progress,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Score score)
        {
            if (id != score.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(score);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScoreExists(score.Id))
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
            ViewData["JobMapEmployeeId"] = new SelectList(_context.Jobmapemployees, "Id", "Id", score.JobMapEmployeeId);
            return View(score);
        }

        // GET: AdminSystem/Scores/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var score = await _context.Scores
                .Include(s => s.JobMapEmployee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (score == null)
            {
                return NotFound();
            }

            return View(score);
        }

        // POST: AdminSystem/Scores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var score = await _context.Scores.FindAsync(id);
            if (score != null)
            {
                _context.Scores.Remove(score);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScoreExists(long id)
        {
            return _context.Scores.Any(e => e.Id == id);
        }
    }
}
