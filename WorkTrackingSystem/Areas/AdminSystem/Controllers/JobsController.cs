using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using WorkTrackingSystem.Models;
using WorkTrackingSystem.Areas.AdminSystem.Models;
using OfficeOpenXml.Drawing.Chart;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using X.PagedList.Extensions;
using X.PagedList;

namespace WorkTrackingSystem.Areas.AdminSystem.Controllers
{
    [Area("AdminSystem")]
    public class JobsController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public JobsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(
            int? page,
            string searchText = "",
            string month = "",
            string status = "",
            string categoryId = "",
            string sortOrder = "",
            bool showCompletedZeroReview = false,
            bool dueToday = false,
            long? jobId = null)
        {
            int pageSize = 5; // Số lượng công việc mỗi trang
            int pageIndex = page ?? 1; // Trang hiện tại

            // Lấy toàn bộ nhân viên
            var employees = await _context.Employees
                .Select(e => new {
                    Value = e.Id.ToString(),
                    Text = $"{e.Code} {e.FirstName} {e.LastName}",
                    Avatar = e.Avatar ?? "/images/default-avatar.png"
                })
                .ToListAsync();

            ViewBag.EmployeeList = employees;
            ViewData["Categories"] = await _context.Categories.ToListAsync();

            // Lưu các tham số lọc vào ViewBag
            ViewBag.SearchText = searchText;
            ViewBag.Month = month;
            ViewBag.Status = status;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortOrder = sortOrder;
            ViewBag.ShowCompletedZeroReview = showCompletedZeroReview;
            ViewBag.DueToday = dueToday;

            // Lấy danh sách JobId đã được gán cho nhân viên
            var assignedJobIds = await _context.Jobmapemployees
                .Where(jme => jme.EmployeeId != null)
                .Select(jme => jme.JobId)
                .Distinct()
                .ToListAsync();

            var scoresQuery = _context.Scores
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Employee)
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Job)
                        .ThenInclude(j => j.Category)
                .Where(s => s.JobMapEmployee != null && s.JobMapEmployee.Job != null);

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
                scoresQuery = scoresQuery.Where(s => s.Time.HasValue &&
                                                     s.Time.Value.Year == selectedMonth.Year &&
                                                     s.Time.Value.Month == selectedMonth.Month);
            }

            // 3. Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && byte.TryParse(status, out byte statusValue))
            {
                scoresQuery = scoresQuery.Where(s => s.Status == statusValue);
            }

            // 4. Lọc theo danh mục
            if (!string.IsNullOrEmpty(categoryId) && long.TryParse(categoryId, out long catId))
            {
                scoresQuery = scoresQuery.Where(s => s.JobMapEmployee.Job.CategoryId == catId);
            }

            // 5. Hiển thị công việc hoàn thành nhưng chưa đánh giá
            if (showCompletedZeroReview)
            {
                scoresQuery = scoresQuery.Where(s => s.Status == 1 && (!s.SummaryOfReviews.HasValue || s.SummaryOfReviews == 0));
            }

            // 6. Sắp xếp theo sortOrder (nếu cần)
            switch (sortOrder)
            {
                case "due_asc":
                    scoresQuery = scoresQuery.OrderBy(s => s.JobMapEmployee.Job.Deadline1);
                    break;
                case "due_desc":
                    scoresQuery = scoresQuery.OrderByDescending(s => s.JobMapEmployee.Job.Deadline1);
                    break;
                case "review_asc":
                    scoresQuery = scoresQuery.OrderBy(s => s.SummaryOfReviews ?? 0);
                    break;
                case "review_desc":
                    scoresQuery = scoresQuery.OrderByDescending(s => s.SummaryOfReviews ?? 0);
                    break;
                default:
                    scoresQuery = scoresQuery.OrderByDescending(s => s.Time);
                    break;
            }
            // Lọc hạn hôm nay
            if (dueToday)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                scoresQuery = scoresQuery.Where(s => s.JobMapEmployee.Job.Deadline1.HasValue && s.JobMapEmployee.Job.Deadline1.Value == today);
            }
            // 7. Phân trang dữ liệu
            var pagedScores = scoresQuery.ToPagedList(pageIndex, pageSize);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ScoreTablePartial", pagedScores);
            }
            return View(pagedScores);
        }
        public IActionResult JobOfEmployee(string search, int? DepartmentId,int page = 1)
        {
            var limit = 10;
           
            var employees = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Jobmapemployees)
                .Select(e => new
                {
                    Employee = e,
                    JobCount = _context.Jobmapemployees.Count(j => j.EmployeeId == e.Id) // Đếm công việc của từng nhân viên
                })
                .ToList();
			
			if (!string.IsNullOrEmpty(search))
            {
				var searchLower = search.ToLower();
				employees = employees.Where(e => (e.Employee.FirstName + " " + e.Employee.LastName).ToLower().Contains(searchLower)|| e.Employee.Code.ToLower().Contains(searchLower)).ToList();
            }
			if (DepartmentId > 0)
			{
				employees = employees.Where(e => e.Employee.DepartmentId == DepartmentId).ToList();
			}
            if(!string.IsNullOrEmpty(search) && DepartmentId > 0)
            {
				var searchLower = search.ToLower();
				employees = employees.Where(e => (e.Employee.FirstName + " " + e.Employee.LastName).ToLower().Contains(searchLower)&& e.Employee.DepartmentId == DepartmentId).ToList();
			}
			// Chuyển danh sách nhân viên về dạng List<Employee>
			var filteredEmployees = employees.Select(e => e.Employee).ToList();

            // Chuyển đổi danh sách sang kiểu IPagedList
            var pagedEmployees = filteredEmployees.ToPagedList(page, limit);

            // Gán danh sách số lượng công việc vào ViewBag
            ViewBag.JobCounts = employees.ToDictionary(e => e.Employee.Id, e => e.JobCount);
			ViewBag.Department = new SelectList(_context.Departments, "Id", "Name");
			return View(pagedEmployees);
        }

        public IActionResult EmployeeWork(long id, int? page, string search, string filterStatus)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // Xử lý search
            search = string.IsNullOrEmpty(search) ? "" : Uri.UnescapeDataString(search).Trim();

            // Lấy danh sách công việc
            var jobs = _context.Jobmapemployees
                .Include(jm => jm.Job)
                    .ThenInclude(j => j.Category)
                .Include(jm => jm.Scores)
                .Where(jm => jm.EmployeeId == id && jm.IsActive == true && jm.IsDelete == false);

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                jobs = jobs.Where(jm => jm.Job.Name.ToLower().Contains(searchLower) ||
                                       jm.Job.Category.Name.ToLower().Contains(searchLower));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(filterStatus) && int.TryParse(filterStatus, out int statusValue))
            {
                jobs = jobs.Where(jm => jm.Scores.OrderByDescending(s => s.Id)
                                                .Select(s => s.Status)
                                                .FirstOrDefault() == statusValue);
            }

            // Map dữ liệu sang JobViewModel
            var jobList = jobs.Select(jm => new JobViewModel
            {
                JobId = jm.Job.Id,
                JobName = jm.Job.Name,
                CategoryName = jm.Job.Category.Name,
                Deadline = jm.Job.Deadline1,
                CompletionDate = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.CompletionDate).FirstOrDefault(),
                ScoreStatus = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.Status).FirstOrDefault(),
                Progress = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.Progress).FirstOrDefault(),
                VolumeAssessment = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.VolumeAssessment).FirstOrDefault(),
                ProgressAssessment = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.ProgressAssessment).FirstOrDefault(),
                QualityAssessment = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.QualityAssessment).FirstOrDefault(),
                SummaryOfReviews = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.SummaryOfReviews).FirstOrDefault()
            }).ToPagedList(pageNumber, pageSize);

            // Lưu giá trị cho ViewBag
            ViewBag.EmployeeId = id;
            ViewBag.Search = search;
            ViewBag.FilterStatus = filterStatus;

            // Nếu là yêu cầu AJAX, trả về PartialView thay vì toàn bộ View
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_JobTablePartial", jobList);
            }

            return View(jobList);
        }
     
        public async Task<IActionResult> ExportToExcel(string searchText, int? status, int? categoryId, bool dueToday, string sortOrder, string month)
		{
			//var managerUsername = HttpContext.Session.GetString("AdminLogin");
			//if (string.IsNullOrEmpty(managerUsername))
			//{
			//	return RedirectToAction("Index", "Login");
			//}

			//var manager = await _context.Users
			//	.Where(u => u.UserName == managerUsername)
			//	.Select(u => u.Employee)
			//	.FirstOrDefaultAsync();

			//if (manager == null)
			//{
			//	return RedirectToAction("Index", "Login");
			//}

			//var managedDepartments = await _context.Departments
			//	.Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
			//	.Select(d => d.Id)
			//	.ToListAsync();

			var employeesInManagedDepartments = await _context.Employees
				.Where(e => e.DepartmentId.HasValue )
				.Select(e => e.Id)
				.ToListAsync();

			var jobs = _context.Jobmapemployees
				.Include(jm => jm.Employee)
				.Include(jm => jm.Job)
				.ThenInclude(j => j.Category)
				.Include(jm => jm.Job)
				.Include(jm => jm.Scores) // Lấy dữ liệu từ bảng SCORE
				.Where(jm => employeesInManagedDepartments.Contains((long)jm.EmployeeId));

			// Áp dụng bộ lọc tìm kiếm
			if (!string.IsNullOrEmpty(searchText))
			{
				jobs = jobs.Where(jm =>
					jm.Employee.Code.Contains(searchText) ||
					jm.Employee.FirstName.Contains(searchText) ||
					jm.Employee.LastName.Contains(searchText) ||
					jm.Job.Name.Contains(searchText));
			}

			if (status.HasValue)
			{
				jobs = jobs.Where(jm => jm.Scores.Any(s => s.Status == status.Value));
			}

			if (categoryId.HasValue)
			{
				jobs = jobs.Where(jm => jm.Job.CategoryId == categoryId.Value);
			}

			if (dueToday)
			{
				jobs = jobs.Where(jm => jm.Scores.Any(s => s.CompletionDate.HasValue && s.CompletionDate.Value == DateOnly.FromDateTime(DateTime.Today)));
			}

			if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out DateTime selectedMonth))
			{
				jobs = jobs.Where(jm => jm.Scores.Any(s => s.CompletionDate.HasValue &&
															s.CompletionDate.Value.Year == selectedMonth.Year &&
															s.CompletionDate.Value.Month == selectedMonth.Month));
			}

			switch (sortOrder)
			{
				case "due_asc":
					jobs = jobs.OrderBy(jm => jm.Job.Deadline1);
					break;
				case "due_desc":
					jobs = jobs.OrderByDescending(jm => jm.Job.Deadline1);
					break;
				case "review_asc":
					jobs = jobs.OrderBy(jm => jm.Scores.FirstOrDefault().SummaryOfReviews);
					break;
				case "review_desc":
					jobs = jobs.OrderByDescending(jm => jm.Scores.FirstOrDefault().SummaryOfReviews);
					break;
			}

			var jobList = await jobs.ToListAsync();

			// Tạo file Excel bằng EPPlus
			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add("Jobs");
				worksheet.Cells[1, 1].Value = "STT";
				worksheet.Cells[1, 2].Value = "Nhân viên";
				worksheet.Cells[1, 3].Value = "Hạng mục";
				worksheet.Cells[1, 4].Value = "Công việc";
				worksheet.Cells[1, 5].Value = "Deadline";
				worksheet.Cells[1, 6].Value = "Ngày hoàn thành";
				worksheet.Cells[1, 7].Value = "Trạng thái";
				worksheet.Cells[1, 8].Value = "Đánh giá khối lượng";
				worksheet.Cells[1, 9].Value = "Đánh giá tiến độ";
				worksheet.Cells[1, 10].Value = "Đánh giá chất lượng";
				worksheet.Cells[1, 11].Value = "Đánh giá tổng hợp";

				// Điền dữ liệu
				for (int i = 0; i < jobList.Count; i++)
				{
					var jobMap = jobList[i];
					var job = jobMap.Job;
					var score = jobMap.Scores.FirstOrDefault();

					worksheet.Cells[i + 2, 1].Value = i + 1; // STT
					worksheet.Cells[i + 2, 2].Value = $"{jobMap.Employee.Code} {jobMap.Employee.FirstName} {jobMap.Employee.LastName}";
					worksheet.Cells[i + 2, 3].Value = job.Category.Name;
					worksheet.Cells[i + 2, 4].Value = job.Name;
					worksheet.Cells[i + 2, 5].Value = job.Deadline1?.ToString("dd/MM/yyyy");
					worksheet.Cells[i + 2, 6].Value = score?.CompletionDate?.ToString("dd/MM/yyyy");
					worksheet.Cells[i + 2, 7].Value = score?.Status switch
					{
						1 => "Hoàn thành",
						2 => "Chưa hoàn thành",
						3 => "Hoàn thành muộn",
						4 => "Đang xử lý",
						_ => "Chưa bắt đầu"
					};
					worksheet.Cells[i + 2, 8].Value = score?.VolumeAssessment;
					worksheet.Cells[i + 2, 9].Value = score?.ProgressAssessment;
					worksheet.Cells[i + 2, 10].Value = score?.QualityAssessment;
					worksheet.Cells[i + 2, 11].Value = score?.SummaryOfReviews;
				}

				// Định dạng cột
				worksheet.Cells[1, 1, 1, 11].Style.Font.Bold = true;
				worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

				// Xuất file Excel
				var stream = new MemoryStream(package.GetAsByteArray());
				return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Jobs.xlsx");
			}
		}

        // GET: ProjectManager/Jobs/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var score = await _context.Scores
                .Include(s => s.JobMapEmployee).Include(s => s.JobMapEmployee.Job).Include(s => s.JobMapEmployee.Employee).Include(s => s.JobMapEmployee.Job.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (score == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details", score);
            }
            return View(score);
        }



        public async Task<IActionResult> Create()
		{
			var managerUsername = HttpContext.Session.GetString("AdminLogin");
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
			ViewBag.CreateBy = managerUsername;
			// Lấy danh sách phòng ban mà người quản lý này quản lý (dựa trên PositionId == 3)
			var managedDepartments = await _context.Departments
				.Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
				.Select(d => d.Id)
				.ToListAsync();

			// Lấy danh sách nhân viên thuộc các phòng ban mà người quản lý đang quản lý
			var employeesInManagedDepartments = await _context.Employees
				.Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value))
				.Select(e => new
				{
					Id = e.Id,
					FullName = e.Code + " " + e.FirstName + " " + e.LastName
				})
				.ToListAsync();

			// Gán danh sách nhân viên vào ViewBag để hiển thị trong dropdown
			//ViewData["EmployeeId"] = new SelectList(employeesInManagedDepartments, "Id", "FullName");
			ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
			ViewBag.JobNames = await _context.Jobs
		.Where(j => !string.IsNullOrEmpty(j.Name)) // Lọc bỏ các job không có tên
		.Select(j => j.Name)
		.Distinct()
		.ToListAsync();
			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return PartialView("_Create");
			}
			return View();
		}


		//// POST: AdminSystem/Jobs/Create
		//// To protect from overposting attacks, enable the specific properties you want to bind to.
		//// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Job job)
		{
			var managerUsername = HttpContext.Session.GetString("AdminLogin");
			if (string.IsNullOrEmpty(managerUsername))
			{
				return RedirectToAction("Index", "Login");
			}

			if (ModelState.IsValid)
			{
				job.CreateBy = managerUsername; // Gán giá trị CreateBy

				_context.Add(job);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", job.CategoryId);
			//ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", job.EmployeeId);
			return View(job);
		}

		
		[HttpPost]
        [ValidateAntiForgeryToken]
        
        private async Task UpdateBaselineAssessment(long? employeeId)
        {
            if (employeeId == null)
                return;

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Lấy danh sách công việc của nhân viên trong tháng hiện tại có đánh giá
            var jobMaps = await _context.Jobmapemployees
                .Include(jm => jm.Scores) // Bao gồm bảng Score để lấy dữ liệu đánh giá
                .Where(jm => jm.EmployeeId == employeeId
                          && jm.Scores.Any(s => s.CompletionDate.HasValue
                                                && s.CompletionDate.Value.Month == currentMonth
                                                && s.CompletionDate.Value.Year == currentYear
                                                && (s.Status == 1 || s.Status == 3))) // Hoàn thành hoặc Hoàn thành muộn
                .ToListAsync();

            if (!jobMaps.Any())
                return;

            // Tính tổng các đánh giá từ bảng Score
            double sumVolume = jobMaps.Sum(jm => jm.Scores.Sum(s => s.VolumeAssessment ?? 0));
            double sumProgress = jobMaps.Sum(jm => jm.Scores.Sum(s => s.ProgressAssessment ?? 0));
            double sumQuality = jobMaps.Sum(jm => jm.Scores.Sum(s => s.QualityAssessment ?? 0));
            double sumSummary = jobMaps.Sum(jm => jm.Scores.Sum(s => s.SummaryOfReviews ?? 0));

            // Xác định trạng thái Evaluate (giả sử tổng Summary >= 45 là đạt)
            bool evaluate = sumSummary >= 45;

            // Tìm bản ghi BaselineAssessment của nhân viên trong tháng hiện tại
            var baseline = await _context.Baselineassessments
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId
                                       && b.Time.HasValue
                                       && b.Time.Value.Month == currentMonth
                                       && b.Time.Value.Year == currentYear);

            if (baseline == null)
            {
                // Nếu chưa có bản ghi trong tháng, tạo mới
                baseline = new Baselineassessment
                {
                    EmployeeId = employeeId,
                    VolumeAssessment = sumVolume,
                    ProgressAssessment = sumProgress,
                    QualityAssessment = sumQuality,
                    SummaryOfReviews = sumSummary,
                    Time = new DateTime(currentYear, currentMonth, 1),
                    Evaluate = evaluate,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };
                _context.Baselineassessments.Add(baseline);
            }
            else
            {
                // Nếu đã có bản ghi trong tháng, cập nhật dữ liệu
                baseline.VolumeAssessment = sumVolume;
                baseline.ProgressAssessment = sumProgress;
                baseline.QualityAssessment = sumQuality;
                baseline.SummaryOfReviews = sumSummary;
                baseline.Evaluate = evaluate;
                baseline.UpdateDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpdateAnalysis(long? employeeId)
        {
            if (employeeId == null)
                return;

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Lấy danh sách công việc của nhân viên trong tháng hiện tại
            var jobMaps = await _context.Jobmapemployees
                .Include(jm => jm.Scores) // Lấy dữ liệu từ bảng Score
                .Where(jm => jm.EmployeeId == employeeId
                          && jm.Scores.Any(s => s.CompletionDate.HasValue
                                                && s.CompletionDate.Value.Month == currentMonth
                                                && s.CompletionDate.Value.Year == currentYear))
                .ToListAsync();

            // Tính số lượng công việc theo trạng thái từ bảng Score
            int ontime = jobMaps.Count(jm => jm.Scores.Any(s => s.Status == 1));    // Hoàn thành đúng hạn
            int late = jobMaps.Count(jm => jm.Scores.Any(s => s.Status == 2));      // Hoàn thành muộn
            int overdue = jobMaps.Count(jm => jm.Scores.Any(s => s.Status == 3));   // Quá hạn chưa hoàn thành
            int processing = jobMaps.Count(jm => jm.Scores.Any(s => s.Status == 4));// Đang xử lý
            int total = ontime + late + overdue + processing;

            // Tính trung bình đánh giá của nhân viên
            var averageReview = jobMaps.Any()
                ? jobMaps.Average(jm => jm.Scores.Average(s => s.SummaryOfReviews ?? 0))
                : 0;

            // Tìm bản ghi Analysis của nhân viên trong tháng hiện tại
            var analysis = await _context.Analyses
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                                          && a.Time.HasValue
                                          && a.Time.Value.Month == currentMonth
                                          && a.Time.Value.Year == currentYear);

            if (analysis == null)
            {
                // Nếu chưa có bản ghi trong tháng, tạo mới
                analysis = new Analysis
                {
                    EmployeeId = employeeId,
                    Total = total,
                    Ontime = ontime,
                    Late = late,
                    Overdue = overdue,
                    Processing = processing,
                    //AverageReview = averageReview,
                    Time = new DateTime(currentYear, currentMonth, 1),
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };
                _context.Analyses.Add(analysis);
            }
            else
            {
                // Nếu đã có bản ghi trong tháng, cập nhật dữ liệu
                analysis.Total = total;
                analysis.Ontime = ontime;
                analysis.Late = late;
                analysis.Overdue = overdue;
                analysis.Processing = processing;
                //analysis.AverageReview = averageReview;
                analysis.UpdateDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }


      

        private bool JobExists(long id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }

    }

}


