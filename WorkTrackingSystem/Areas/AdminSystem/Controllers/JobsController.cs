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

        // GET: AdminSystem/Jobs
        public async Task<IActionResult> Index(
                 int? page,
                 string searchText = null,
                 int? status = null,
                 int? categoryId = null,
                 bool dueToday = false,
                 string sortOrder = null,
                 string month = null,
                 bool showCompletedZeroReview = false)
                    {
            int limit = 10;
            int pageIndex = page ?? 1;
            var assignedEmployees = await _context.Jobmapemployees
                                     .Where(j => j.EmployeeId.HasValue)
                                     .Select(j => new
                                     {
                                         Value = j.EmployeeId,
                                         Text = $"{j.Employee.Code} - {j.Employee.FirstName} {j.Employee.LastName}", 
                                         Avatar = j.Employee.Avatar ?? "/images/default-avatar.png"
                                     })
                                     .Distinct()
                                     .ToListAsync();

            ViewBag.EmployeeList = assignedEmployees;
            var jobs = _context.Jobmapemployees
                .Include(j => j.Job)
                .ThenInclude(js => js.Category)
                .Include(j => j.Scores)
                .Include(j => j.Employee)
                .Where(j => j.IsActive == true &&
                ((j.EmployeeId.HasValue && assignedEmployees.Select(e => e.Value).Contains(j.EmployeeId.Value))
                || (j.EmployeeId == null)));

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(searchText))
            {
                jobs = jobs.Where(j =>
                    j.Employee != null &&
                    (j.Employee.Code.Contains(searchText) ||
                     j.Employee.FirstName.Contains(searchText) ||
                     j.Employee.LastName.Contains(searchText) ||
                     j.Job.Name.Contains(searchText)));
            }

            // Lọc theo trạng thái công việc
            if (status.HasValue)
            {
                jobs = jobs.Where(j => j.Scores.Any(s => s.Status == status.Value));
            }

            // Lọc theo danh mục công việc
            if (categoryId.HasValue)
            {
                jobs = jobs.Where(j => j.Job.CategoryId == categoryId.Value);
            }

            // Lọc công việc có hạn chót là hôm nay
            if (dueToday)
            {
                jobs = jobs.Where(j => j.Scores.Any(s => s.Time.HasValue && s.Time.Value.Date == DateTime.Today));
            }

            // Lọc công việc đã hoàn thành nhưng chưa có đánh giá
            if (showCompletedZeroReview)
            {
                jobs = jobs.Where(j => j.Scores.Any(s => (s.Status == 1 || s.Status == 3) && s.SummaryOfReviews == 0));
            }

            // Lọc theo tháng
            if (!string.IsNullOrEmpty(month))
            {
                if (DateTime.TryParse($"{month}-01", out DateTime selectedMonth))
                {
                    jobs = jobs.Where(j => j.Scores.Any(s =>
                        s.Time.HasValue &&
                        s.Time.Value.Year == selectedMonth.Year &&
                        s.Time.Value.Month == selectedMonth.Month));
                }
                else
                {
                    TempData["Error"] = "Định dạng tháng không hợp lệ.";
                }
            }

            // Sắp xếp danh sách công việc
            switch (sortOrder)
            {
                case "due_asc":
                    jobs = jobs.OrderBy(j => j.Scores.OrderBy(s => s.Time).FirstOrDefault().Time);
                    break;
                case "due_desc":
                    jobs = jobs.OrderByDescending(j => j.Scores.OrderByDescending(s => s.Time).FirstOrDefault().Time);
                    break;
                case "review_asc":
                    jobs = jobs.OrderBy(j => j.Scores.OrderBy(s => s.SummaryOfReviews).FirstOrDefault().SummaryOfReviews);
                    break;
                case "review_desc":
                    jobs = jobs.OrderByDescending(j => j.Scores.OrderByDescending(s => s.SummaryOfReviews).FirstOrDefault().SummaryOfReviews);
                    break;
                default:
                    jobs = jobs.OrderBy(j => j.EmployeeId == null ? 0 : 1).ThenByDescending(j => j.Id);
                    break;
            }

            // Đưa dữ liệu vào ViewBag
            ViewBag.SearchText = searchText;
            ViewBag.Status = status;
            ViewBag.CategoryId = categoryId;
            ViewBag.DueToday = dueToday;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Month = month;
            ViewBag.ShowCompletedZeroReview = showCompletedZeroReview;

            // Tính tổng số lượng công việc theo trạng thái
            ViewData["TotalCompleted"] = await _context.Scores.CountAsync(s => s.Status == 1);
            ViewData["TotalNotCompleted"] = await _context.Scores.CountAsync(s => s.Status == 2);
            ViewData["TotalLate"] = await _context.Scores.CountAsync(s => s.Status == 3);
            ViewData["TotalProcessing"] = await _context.Scores.CountAsync(s => s.Status == 4);

            ViewData["Categories"] = await _context.Categories.ToListAsync();

            return View(jobs.ToPagedList(pageIndex, limit));
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
            search = search != null ? Uri.UnescapeDataString(search) : "";

            var jobs = _context.Jobmapemployees
                
                .Include(jm => jm.Job)
                    .ThenInclude(j => j.Category)
                    
                .Include(jm => jm.Scores) // Lấy danh sách điểm đánh giá của công việc
                .Where(jm => jm.EmployeeId == id && jm.IsActive == true && jm.IsDelete == false);

            // Tìm kiếm theo tên công việc
            if (!string.IsNullOrEmpty(search))
            {
				var searchLower = search.ToLower();
				jobs = jobs.Where(jm => jm.Job.Name.ToLower().Contains(search) );
				//employees = employees.Where(e => (e.Employee.FirstName + " " + e.Employee.LastName).ToLower().Contains(searchLower)).ToList();
			}

            // Lọc theo trạng thái công việc dựa trên Score.Status
            if (!string.IsNullOrEmpty(filterStatus) && int.TryParse(filterStatus, out int statusValue))
            {
                jobs = jobs.Where(jm => jm.Scores.Any(s => s.Status == statusValue));
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
				VolumeAssessment= jm.Scores.OrderByDescending(s => s.Id).Select(s => s.VolumeAssessment).FirstOrDefault(),
				ProgressAssessment= jm.Scores.OrderByDescending(s => s.Id).Select(s => s.ProgressAssessment).FirstOrDefault(),
				QualityAssessment = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.QualityAssessment).FirstOrDefault(),
				SummaryOfReviews = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.SummaryOfReviews).FirstOrDefault()
			}).ToPagedList(pageNumber, pageSize);

            ViewBag.Search = search;
            ViewBag.FilterStatus = filterStatus;

            return View(jobList);
        }
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> AssignEmployee(long jobId, long employeeId)
		//{
		//    try
		//    {
		//        var job = await _context.Jobs.FindAsync(jobId);
		//        if (job == null)
		//        {
		//            return Json(new { success = false, message = "Không tìm thấy công việc với ID: " + jobId });
		//        }

		//        var employee = await _context.Employees.FindAsync(employeeId);
		//        if (employee == null)
		//        {
		//            return Json(new { success = false, message = "Không tìm thấy nhân viên với ID: " + employeeId });
		//        }
		//        job.Status = 0;
		//        job.EmployeeId = employeeId;
		//        await _context.SaveChangesAsync();

		//        Trả về thông tin nhân viên để cập nhật giao diện(nếu cần)
		//        return Json(new
		//        {
		//            success = true,
		//            message = "Đã gán công việc thành công!",
		//            employee = new
		//            {
		//                Id = employee.Id,
		//                Name = $"{employee.Code} {employee.FirstName} {employee.LastName}",
		//                Avatar = employee.Avatar ?? "/images/default-avatar.png"
		//            }
		//        });
		//    }
		//    catch (Exception ex)
		//    {
		//        return Json(new { success = false, message = "Lỗi khi gán công việc: " + ex.Message });
		//    }
		//}
		//public async Task<IActionResult> GetJobsByEmployee(long employeeId)
		//{
		//    try
		//    {
		//        var managerUsername = HttpContext.Session.GetString("AdminLogin");
		//        if (string.IsNullOrEmpty(managerUsername))
		//        {
		//            return Json(new { success = false, message = "Vui lòng đăng nhập." });
		//        }

		//        var manager = await _context.Users
		//            .Where(u => u.UserName == managerUsername)
		//            .Select(u => u.Employee)
		//            .FirstOrDefaultAsync();

		//        if (manager == null)
		//        {
		//            return Json(new { success = false, message = "Không tìm thấy thông tin quản lý." });
		//        }

		//        var managedDepartments = await _context.Departments
		//            .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
		//            .Select(d => d.Id)
		//            .ToListAsync();

		//        Lấy danh sách công việc chưa được giao(EmployeeId == null)
		//        var unassignedJobs = await _context.Jobs
		//            .Include(j => j.Category)
		//            .Where(j => j.EmployeeId == null && j.CreateBy == managerUsername)
		//            .Select(j => new
		//            {
		//                j.Id,
		//                j.Name,
		//                CategoryName = j.Category.Name,
		//                j.Time,
		//                j.Deadline1,
		//                j.Status,
		//                j.CompletionDate
		//            })
		//            .ToListAsync();

		//        Lấy danh sách công việc của nhân viên đã chọn
		//        var assignedJobs = await _context.Jobs
		//            .Include(j => j.Category)
		//            .Where(j => j.EmployeeId == employeeId)
		//            .Select(j => new
		//            {
		//                j.Id,
		//                j.Name,
		//                CategoryName = j.Category.Name,
		//                j.Time,
		//                j.Deadline1,
		//                j.Status,
		//                j.CompletionDate
		//            })
		//            .ToListAsync();

		//        return Json(new
		//        {
		//            success = true,
		//            unassignedJobs = unassignedJobs,
		//            assignedJobs = assignedJobs
		//        });
		//    }
		//    catch (Exception ex)
		//    {
		//        return Json(new { success = false, message = "Lỗi: " + ex.Message });
		//    }
		//}
		//[HttpPost]

		//public IActionResult UpdateJobDate(long jobId, string field, string date)
		//{
		//    try
		//    {
		//        var job = _context.Jobs.FirstOrDefault(j => j.Id == jobId);
		//        if (job == null)
		//        {
		//            return Json(new { success = false, message = "Công việc không tồn tại" });
		//        }

		//        DateTime parsedDate;
		//        if (!DateTime.TryParse(date, out parsedDate))
		//        {
		//            return Json(new { success = false, message = "Định dạng ngày không hợp lệ" });
		//        }

		//        if (field == "time")
		//        {
		//            job.Time = parsedDate;
		//        }
		//        else if (field == "deadline1")
		//        {
		//            job.Deadline1 = DateOnly.FromDateTime(parsedDate);
		//        }
		//        else
		//        {
		//            return Json(new { success = false, message = "Trường không hợp lệ" });
		//        }

		//        _context.SaveChanges();
		//        return Json(new { success = true });
		//    }
		//    catch (Exception ex)
		//    {
		//        return Json(new { success = false, message = ex.Message });
		//    }
		//}
		//public async Task<IActionResult> ExportToExcel(string searchText, int? status, int? categoryId, bool dueToday, string sortOrder, string month)
		//{
		//	var managerUsername = HttpContext.Session.GetString("AdminLogin");
		//	if (string.IsNullOrEmpty(managerUsername))
		//	{
		//		return RedirectToAction("Index", "Login");
		//	}

		//	var manager = await _context.Users
		//		.Where(u => u.UserName == managerUsername)
		//		.Select(u => u.Employee)
		//		.FirstOrDefaultAsync();

		//	if (manager == null)
		//	{
		//		return RedirectToAction("Index", "Login");
		//	}

		//	var managedDepartments = await _context.Departments
		//		.Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
		//		.Select(d => d.Id)
		//		.ToListAsync();

		//	var employeesInManagedDepartments = await _context.Employees
		//		.Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value))
		//		.Select(e => e.Id)
		//		.ToListAsync();

		//	var jobs = _context.Jobmapemployees
		//		.Include(jm => jm.Employee)
		//		.Include(jm => jm.Job)
		//		.ThenInclude(j => j.Category)
		//		.Include(jm => jm.Job)
		//		.Include(jm=>jm.Scores) // Lấy dữ liệu từ bảng SCORE
		//		.Where(jm => employeesInManagedDepartments.Contains(jm.EmployeeId));

		//	// Áp dụng bộ lọc tìm kiếm
		//	if (!string.IsNullOrEmpty(searchText))
		//	{
		//		jobs = jobs.Where(jm =>
		//			jm.Employee.Code.Contains(searchText) ||
		//			jm.Employee.FirstName.Contains(searchText) ||
		//			jm.Employee.LastName.Contains(searchText) ||
		//			jm.Job.Name.Contains(searchText));
		//	}

		//	if (status.HasValue)
		//	{
		//		jobs = jobs.Where(jm => jm.Job.Scores.Any(s => s.Status == status.Value));
		//	}

		//	if (categoryId.HasValue)
		//	{
		//		jobs = jobs.Where(jm => jm.Job.CategoryId == categoryId.Value);
		//	}

		//	if (dueToday)
		//	{
		//		jobs = jobs.Where(jm => jm.Job.Scores.Any(s => s.CompletionDate.HasValue && s.CompletionDate.Value.Date == DateTime.Today));
		//	}

		//	if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out DateTime selectedMonth))
		//	{
		//		jobs = jobs.Where(jm => jm.Job.Scores.Any(s => s.CompletionDate.HasValue &&
		//													s.CompletionDate.Value.Year == selectedMonth.Year &&
		//													s.CompletionDate.Value.Month == selectedMonth.Month));
		//	}

		//	switch (sortOrder)
		//	{
		//		case "due_asc":
		//			jobs = jobs.OrderBy(jm => jm.Job.Deadline1);
		//			break;
		//		case "due_desc":
		//			jobs = jobs.OrderByDescending(jm => jm.Job.Deadline1);
		//			break;
		//		case "review_asc":
		//			jobs = jobs.OrderBy(jm => jm.Job.Scores.FirstOrDefault().SummaryOfReviews);
		//			break;
		//		case "review_desc":
		//			jobs = jobs.OrderByDescending(jm => jm.Job.Scores.FirstOrDefault().SummaryOfReviews);
		//			break;
		//	}

		//	var jobList = await jobs.ToListAsync();

		//	// Tạo file Excel bằng EPPlus
		//	using (var package = new ExcelPackage())
		//	{
		//		var worksheet = package.Workbook.Worksheets.Add("Jobs");
		//		worksheet.Cells[1, 1].Value = "STT";
		//		worksheet.Cells[1, 2].Value = "Nhân viên";
		//		worksheet.Cells[1, 3].Value = "Hạng mục";
		//		worksheet.Cells[1, 4].Value = "Công việc";
		//		worksheet.Cells[1, 5].Value = "Deadline";
		//		worksheet.Cells[1, 6].Value = "Ngày hoàn thành";
		//		worksheet.Cells[1, 7].Value = "Trạng thái";
		//		worksheet.Cells[1, 8].Value = "Đánh giá khối lượng";
		//		worksheet.Cells[1, 9].Value = "Đánh giá tiến độ";
		//		worksheet.Cells[1, 10].Value = "Đánh giá chất lượng";
		//		worksheet.Cells[1, 11].Value = "Đánh giá tổng hợp";

		//		// Điền dữ liệu
		//		for (int i = 0; i < jobList.Count; i++)
		//		{
		//			var jobMap = jobList[i];
		//			var job = jobMap.Job;
		//			var score = job.Scores.FirstOrDefault(); // Lấy bản ghi đánh giá đầu tiên nếu có

		//			worksheet.Cells[i + 2, 1].Value = i + 1; // STT
		//			worksheet.Cells[i + 2, 2].Value = $"{jobMap.Employee.Code} {jobMap.Employee.FirstName} {jobMap.Employee.LastName}";
		//			worksheet.Cells[i + 2, 3].Value = job.Category.Name;
		//			worksheet.Cells[i + 2, 4].Value = job.Name;
		//			worksheet.Cells[i + 2, 5].Value = job.Deadline1?.ToString("dd/MM/yyyy");
		//			worksheet.Cells[i + 2, 6].Value = score?.CompletionDate?.ToString("dd/MM/yyyy");
		//			worksheet.Cells[i + 2, 7].Value = score?.Status switch
		//			{
		//				1 => "Hoàn thành",
		//				2 => "Chưa hoàn thành",
		//				3 => "Hoàn thành muộn",
		//				4 => "Đang xử lý",
		//				_ => "Chưa bắt đầu"
		//			};
		//			worksheet.Cells[i + 2, 8].Value = score?.VolumeAssessment;
		//			worksheet.Cells[i + 2, 9].Value = score?.ProgressAssessment;
		//			worksheet.Cells[i + 2, 10].Value = score?.QualityAssessment;
		//			worksheet.Cells[i + 2, 11].Value = score?.SummaryOfReviews;
		//		}

		//		// Định dạng cột
		//		worksheet.Cells[1, 1, 1, 11].Style.Font.Bold = true;
		//		worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

		//		// Xuất file Excel
		//		var stream = new MemoryStream(package.GetAsByteArray());
		//		return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Jobs.xlsx");
		//	}
		//}

		//// GET: ProjectManager/Jobs/Details/5
		//public async Task<IActionResult> Details(long? id)
		//{
		//    if (id == null)
		//    {
		//        return NotFound();
		//    }

		//    var job = await _context.Jobs
		//        .Include(j => j.Category)
		//        .Include(j => j.Employee) // Include Employee nếu có
		//        .FirstOrDefaultAsync(m => m.Id == id);

		//    if (job == null)
		//    {
		//        return NotFound();
		//    }

		//    // Kiểm tra nếu EmployeeId là null
		//    if (job.EmployeeId == null)
		//    {
		//        ViewBag.JobStatus = "Công việc chưa được giao";
		//    }

		//    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
		//    {
		//        return PartialView("_Details", job);
		//    }

		//    return View(job);
		//}



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

		//public async Task<IActionResult> Edit(long? id)
		//{
		//    if (id == null)
		//    {
		//        return NotFound();
		//    }

		//    var job = await _context.Jobs
		//        .Include(j => j.Jobmapemployees) // Load bảng trung gian
		//            .ThenInclude(jme => jme.Scores) // Load danh sách Score từ JobMapEmployee
		//        .FirstOrDefaultAsync(j => j.Id == id);

		//    if (job == null)
		//    {
		//        return NotFound();
		//    }

		//    // Lấy JobMapEmployee đầu tiên (vì có thể có nhiều)
		//    var jobMapEmployee = job.Jobmapemployees.FirstOrDefault();

		//    // Lấy Score mới nhất nếu có
		//    var latestScore = jobMapEmployee?.Scores?.OrderByDescending(s => s.Time).FirstOrDefault();

		//    // Gán dữ liệu vào ViewBag để hiển thị trong View
		//    ViewBag.LatestStatus = latestScore?.Status ?? 0;
		//    ViewBag.CompletionDate = latestScore?.CompletionDate?.ToString("yyyy-MM-dd");
		//    ViewBag.LastUpdated = latestScore?.Time?.ToString("yyyy-MM-dd HH:mm:ss");

		//    ViewData["CategoryId"] = new SelectList(
		//        _context.Categories.Select(c => new { c.Id, Display = c.Code + " - " + c.Name }),
		//        "Id", "Display", job.CategoryId
		//    );

		//    ViewData["EmployeeId"] = new SelectList(
		//        _context.Employees.Select(e => new { e.Id, Display = e.Code + " - " + e.FirstName + " " + e.LastName }),
		//        "Id", "Display", jobMapEmployee.EmployeeId
		//    );
		//    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
		//    {
		//        return PartialView("_Edit", job);
		//    }
		//    return View(job);
		//}
		// POST: ProjectManager/Jobs/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(long id, [Bind("Id,EmployeeId,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,CompletionDate,Status,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Job job)
        //{
        //	var managerUsername = HttpContext.Session.GetString("AdminLogin");
        //	if (id != job.Id)
        //	{
        //		return NotFound();
        //	}

        //	if (ModelState.IsValid)
        //	{
        //		try
        //		{
        //			_context.Update(job);
        //			job.Status = 5;
        //			job.CreateBy = managerUsername;
        //			await _context.SaveChangesAsync();
        //			await UpdateBaselineAssessment(job.EmployeeId);
        //			await UpdateAnalysis(job.EmployeeId);
        //		}
        //		catch (DbUpdateConcurrencyException)
        //		{
        //			if (!JobExists(job.Id))
        //			{
        //				return NotFound();
        //			}
        //			else
        //			{
        //				throw;
        //			}
        //		}
        //		return RedirectToAction(nameof(Index));
        //	}

        //	ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", job.CategoryId);
        //	ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", job.EmployeeId);
        //	if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        //	{
        //		await UpdateAnalysis(job.EmployeeId);
        //		return PartialView("_Edit", job);
        //	}
        //	return View(job);
        //}
        //private async Task UpdateBaselineAssessment(long? employeeId)
        //{
        //    if (employeeId == null)
        //        return;

        //    var currentMonth = DateTime.Now.Month;
        //    var currentYear = DateTime.Now.Year;

        //    // Lấy danh sách công việc của nhân viên trong tháng hiện tại có đánh giá
        //    var jobs = await _context.Jobs
        //        .Where(j => j.EmployeeId == employeeId
        //                 && j.Time.HasValue
        //                 && j.Time.Value.Month == currentMonth
        //                 && j.Time.Value.Year == currentYear
        //                 && (j.Status == 1 || j.Status == 3)) // Chỉ tính các công việc "Hoàn thành" hoặc "Hoàn thành muộn"
        //        .ToListAsync();

        //    if (!jobs.Any())
        //        return;

        //    // Tính tổng các đánh giá
        //    double sumVolume = jobs.Sum(j => j.VolumeAssessment ?? 0);
        //    double sumProgress = jobs.Sum(j => j.ProgressAssessment ?? 0);
        //    double sumQuality = jobs.Sum(j => j.QualityAssessment ?? 0);
        //    double sumSummary = jobs.Sum(j => j.SummaryOfReviews ?? 0);

        //    // Xác định trạng thái Evaluate (giả sử tổng Summary >= 6 là đạt, bạn có thể điều chỉnh ngưỡng)
        //    bool evaluate = sumSummary >= 45;

        //    // Tìm bản ghi BaselineAssessment của nhân viên trong tháng hiện tại
        //    var baseline = await _context.Baselineassessments
        //        .FirstOrDefaultAsync(b => b.EmployeeId == employeeId
        //                               && b.Time.HasValue
        //                               && b.Time.Value.Month == currentMonth
        //                               && b.Time.Value.Year == currentYear);

        //    if (baseline == null)
        //    {
        //        // Nếu chưa có bản ghi trong tháng, tạo mới
        //        baseline = new Baselineassessment
        //        {
        //            EmployeeId = employeeId,
        //            VolumeAssessment = sumVolume,
        //            ProgressAssessment = sumProgress,
        //            QualityAssessment = sumQuality,
        //            SummaryOfReviews = sumSummary,
        //            Time = new DateTime(currentYear, currentMonth, 1),
        //            Evaluate = evaluate,
        //            CreateDate = DateTime.Now,
        //            UpdateDate = DateTime.Now
        //        };
        //        _context.Baselineassessments.Add(baseline);
        //    }
        //    else
        //    {
        //        // Nếu đã có bản ghi trong tháng, cập nhật dữ liệu
        //        baseline.VolumeAssessment = sumVolume;
        //        baseline.ProgressAssessment = sumProgress;
        //        baseline.QualityAssessment = sumQuality;
        //        baseline.SummaryOfReviews = sumSummary;
        //        baseline.Evaluate = evaluate;
        //        baseline.UpdateDate = DateTime.Now;
        //    }

        //    await _context.SaveChangesAsync();
        //}

        //private async Task UpdateAnalysis(long? employeeId)
        //{
        //    if (employeeId == null)
        //        return;

        //    var currentMonth = DateTime.Now.Month;
        //    var currentYear = DateTime.Now.Year;

        //    // Lấy danh sách công việc của nhân viên trong tháng hiện tại
        //    var jobs = await _context.Jobs
        //        .Where(j => j.EmployeeId == employeeId && j.Time.HasValue &&
        //                    j.Time.Value.Month == currentMonth && j.Time.Value.Year == currentYear)
        //        .ToListAsync();


        //    int ontime = jobs.Count(j => j.Status == 1);
        //    int late = jobs.Count(j => j.Status == 2);
        //    int overdue = jobs.Count(j => j.Status == 3);
        //    int processing = jobs.Count(j => j.Status == 4);
        //    int total = ontime + late + overdue + processing;
        //    // Tính trung bình đánh giá của nhân viên
        //    var averageReview = jobs.Any()
        //        ? jobs.Average(j => j.SummaryOfReviews ?? 0)
        //        : 0;

        //    // Tìm bản ghi Analysis của nhân viên trong tháng hiện tại
        //    var analysis = await _context.Analyses
        //        .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Time.HasValue &&
        //                                  a.Time.Value.Month == currentMonth && a.Time.Value.Year == currentYear);

        //    if (analysis == null)
        //    {
        //        // Nếu chưa có bản ghi trong tháng, tạo mới
        //        analysis = new Analysis
        //        {
        //            EmployeeId = employeeId,
        //            Total = total,
        //            Ontime = ontime,
        //            Late = late,
        //            Overdue = overdue,
        //            Processing = processing,
        //            Time = new DateTime(currentYear, currentMonth, 1),
        //            CreateDate = DateTime.Now,
        //            UpdateDate = DateTime.Now
        //        };
        //        _context.Analyses.Add(analysis);
        //    }
        //    else
        //    {
        //        // Nếu đã có bản ghi trong tháng, cập nhật dữ liệu
        //        analysis.Total = total;
        //        analysis.Ontime = ontime;
        //        analysis.Late = late;
        //        analysis.Overdue = overdue;
        //        analysis.Processing = processing;
        //        analysis.UpdateDate = DateTime.Now;
        //    }

        //    await _context.SaveChangesAsync();
        //}

        //// GET: ProjectManager/Jobs/Delete/5
        //public async Task<IActionResult> Delete(long? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var job = await _context.Jobs
        //        .Include(j => j.Category)
        //        .Include(j => j.Employee)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (job == null)
        //    {
        //        return NotFound();
        //    }
        //    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        //    {
        //        return PartialView("_Delete", job);
        //    }
        //    return View(job);
        //}

        //// POST: ProjectManager/Jobs/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(long id)
        //{
        //    var job = await _context.Jobs.FindAsync(id);
        //    if (job != null)
        //    {
        //        job.IsActive = false;
        //        job.IsDelete = true;
        //        _context.Jobs.Update(job);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

     
        
        private bool JobExists(long id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }

    }
    //public class JobViewModel
    //   {
    //       public List<int> EmployeeIds { get; set; }
    //       public long? CategoryId { get; set; }
    //       public string Name { get; set; }
    //       public string Description { get; set; }
    //       public string Deadline1 { get; set; }
    //       public string Deadline2 { get; set; }
    //       public string Deadline3 { get; set; }
    //       public string CompletionDate { get; set; }
    //       public byte? Status { get; set; }
    //       public bool? IsActive { get; set; }
    //       public bool? IsDelete { get; set; }
    //       public string CreateBy { get; set; }
    //   }

}


