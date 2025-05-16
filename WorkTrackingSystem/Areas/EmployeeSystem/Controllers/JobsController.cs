using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Areas.EmployeeSystem.Models;
using WorkTrackingSystem.Models;

using X.PagedList;
using X.PagedList.Extensions;

namespace WorkTrackingSystem.Areas.EmployeeSystem.Controllers
{
    [Area("EmployeeSystem")]
    public class JobsController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public JobsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: EmployeeSystem/Jobs
        public async Task<IActionResult> Index(int? page, string? selectedMonth = null,
    DateTime? startDate = null, DateTime? endDate = null,
    string? search = null, string? filterStatus = null,
    DateTime? deadlineStartDate = null, DateTime? deadlineEndDate = null, bool showReview = false)
        {
            var employeeIdString = HttpContext.Session.GetString("EmployeeId");
            long employeeId = 0;

            if (!string.IsNullOrEmpty(employeeIdString))
            {
                long.TryParse(employeeIdString, out employeeId);
            }
            int limit = 10;
            int pageIndex = page ?? 1;

            var sessionUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(sessionUserId) || !long.TryParse(sessionUserId, out long userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.EmployeeId == null)
            {
                return NotFound("Không tìm thấy nhân viên tương ứng.");
            }

            //var jobs = _context.Jobmapemployees
            //	.Include(jm => jm.Job)
            //		.ThenInclude(j => j.Category)
            //	.Include(jm => jm.Employee)
            //	.Include(jm => jm.Scores)
            //	.Where(jm => jm.EmployeeId == user.EmployeeId && jm.IsActive == true && jm.IsDelete == false);
            var jobs = _context.Scoreemployees
                                .Include(s => s.JobMapEmployee)
                                    .ThenInclude(jm => jm.Job)
                                        .ThenInclude(j => j.Category)
                                .Include(s => s.JobMapEmployee.Employee)
                                .Where(s => s.JobMapEmployee.EmployeeId == user.EmployeeId && s.IsDelete == false);
            if (!string.IsNullOrEmpty(search))
            {
                jobs = jobs.Where(jm => jm.JobMapEmployee.Job.Name.Contains(search));
            }

            if (filterStatus != null && int.TryParse(filterStatus, out int statusValue))
            {
                jobs = jobs.Where(s => s.Status == statusValue);
            }
            if (showReview)
            {
                jobs = jobs.Where(s => s.IsActive == false
                    );
            }

            if (!string.IsNullOrEmpty(selectedMonth) &&
                DateTime.TryParseExact(selectedMonth + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var startOfMonth))
            {
                var startOfMonthDateOnly = DateOnly.FromDateTime(startOfMonth);
                var endOfMonthDateOnly = startOfMonthDateOnly.AddMonths(1).AddDays(-1);
                jobs = jobs.Where(s => s.CompletionDate.HasValue &&
                                                           s.CompletionDate.Value >= startOfMonthDateOnly &&
                                                           s.CompletionDate.Value <= endOfMonthDateOnly);
            }
            if (startDate.HasValue || endDate.HasValue)
            {
                jobs = jobs.Where(j => j.CompletionDate.HasValue &&
                                      (!startDate.HasValue || j.CompletionDate.Value >= DateOnly.FromDateTime(startDate.Value)) &&
                                      (!endDate.HasValue || j.CompletionDate.Value <= DateOnly.FromDateTime(endDate.Value)));
            }
            if (deadlineStartDate.HasValue || deadlineEndDate.HasValue)
            {
                jobs = jobs.Where(j => j.JobMapEmployee.Job.Deadline1.HasValue &&
                                      (!deadlineStartDate.HasValue || j.JobMapEmployee.Job.Deadline1.Value >= DateOnly.FromDateTime(deadlineStartDate.Value)) &&
                                      (!deadlineEndDate.HasValue || j.JobMapEmployee.Job.Deadline1.Value <= DateOnly.FromDateTime(deadlineEndDate.Value)));
            }
            var jobList = await jobs
    .OrderBy(s =>
        s.JobMapEmployee.JobRepeat != null && s.JobMapEmployee.JobRepeat.Deadline1 != null
            ? s.JobMapEmployee.JobRepeat.Deadline1
            : s.JobMapEmployee.Job.Deadline1)
    .Select(s => new JobViewModel
    {
        JobId = s.JobMapEmployee.Job.Id,
        JobName = s.JobMapEmployee.Job.Name,
        CategoryName = s.JobMapEmployee.Job.Category.Name,
        EmployeeName = s.JobMapEmployee.Employee.FirstName + " " + s.JobMapEmployee.Employee.LastName,
        ScoreStatus = s.Status ?? 0,
        CompletionDate = s.CompletionDate,
        JobRepeatId = s.JobMapEmployee.JobRepeatId,
        // Cập nhật logic Deadline:
        Deadline = s.JobMapEmployee.JobRepeat != null && s.JobMapEmployee.JobRepeat.Deadline1 != null
            ? s.JobMapEmployee.JobRepeat.Deadline1
            : s.JobMapEmployee.Job.Deadline1,

        Progress = s.Progress ?? 0,
        VolumeAssessment = s.VolumeAssessment ?? 0,
        ProgressAssessment = s.ProgressAssessment ?? 0,
        QualityAssessment = s.QualityAssessment ?? 0,
        SummaryOfReviews = s.SummaryOfReviews ?? 0,
        IsActive = s.IsActive ?? true,
    })
    .ToListAsync();
            //var jobList = await jobs.OrderBy(s => s.JobMapEmployee.Job.Deadline1)
            //                        .Select(s => new JobViewModel
            //                        {
            //                            JobId = s.JobMapEmployee.Job.Id,
            //                            JobName = s.JobMapEmployee.Job.Name,
            //                            CategoryName = s.JobMapEmployee.Job.Category.Name,
            //                            EmployeeName = s.JobMapEmployee.Employee.FirstName + " " + s.JobMapEmployee.Employee.LastName,
            //                            ScoreStatus = s.Status ?? 0,
            //                            CompletionDate = s.CompletionDate,
            //                            Deadline = s.JobMapEmployee.Job.Deadline1 ,
            //                            Progress = s.Progress ?? 0,
            //                            VolumeAssessment = s.VolumeAssessment ?? 0,
            //                            ProgressAssessment = s.ProgressAssessment ?? 0,
            //                            QualityAssessment = s.QualityAssessment ?? 0,
            //                            SummaryOfReviews = s.SummaryOfReviews ?? 0,
            //                            IsActive = s.IsActive ?? true,
            //                        })
            //                        .ToListAsync();

            var pagedJobs = jobList.Any() ? jobList.ToPagedList(pageIndex, limit) : new PagedList<JobViewModel>(new List<JobViewModel>(), pageIndex, limit);
            ViewBag.employee = employeeId;
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.Search = search;
            ViewBag.FilterStatus = filterStatus;
            ViewBag.deadlineStartDate = deadlineStartDate?.ToString("yyyy-MM-dd");
            ViewBag.deadlineEndDate = deadlineEndDate?.ToString("yyyy-MM-dd");
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.ShowReview = showReview;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_JobListPartial", pagedJobs);
            }
            return View(pagedJobs);
        }
        public IActionResult Createjob()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Createjob");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Createjob([Bind("Id,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Job job)
        {
            //var userId = HttpContext.Session.GetString("UserId");
            var employeeIdString = HttpContext.Session.GetString("EmployeeId");
            long employeeId = 0;

            if (!string.IsNullOrEmpty(employeeIdString))
            {
                long.TryParse(employeeIdString, out employeeId);
            }
            var emUsername = _context.Employees.Where(x => x.Id == employeeId).Select(x => x.FirstName + "" + x.LastName).FirstOrDefault();
            if (ModelState.IsValid)
            {
                // Tạo Job
                job.CreateBy = emUsername;
                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                // Tạo JobMapEmployee (Gán JobId)
                var jobMapEmployee = new Jobmapemployee
                {
                    JobId = job.Id,
                    EmployeeId = employeeId,
                    IsDelete = false,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreateBy = job.CreateBy,
                    UpdateBy = job.UpdateBy
                };

                _context.Jobmapemployees.Add(jobMapEmployee);
                await _context.SaveChangesAsync(); // Lưu để lấy Id của JobMapEmployee

                // Tạo Score (Gán JobMapEmployeeId)
                var score = new Scoreemployee
                {
                    JobMapEmployeeId = jobMapEmployee.Id,
                    Status = 0, // Mặc định trạng thái chưa bat dau
                    IsDelete = false,
                    IsActive = true,
                    CreateDate = job.Deadline1?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreateBy = job.CreateBy,
                    UpdateBy = job.UpdateBy
                };

                _context.Scoreemployees.Add(score);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", job.CategoryId);
            return View(job);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]

        public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressRequest request)
        {
            if (request == null || request.Id <= 0)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            // Kiểm tra giá trị Progress hợp lệ (0-100)
            if (request.Progress < 0 || request.Progress > 100)
            {
                return BadRequest(new { success = false, message = "Tiến độ phải nằm trong khoảng 0 đến 100" });
            }

            // Tìm JobMapEmployee liên quan đến công việc
            var jobMapEmployee = await _context.Jobmapemployees
     .Include(jm => jm.Job)
     .Include(jm => jm.JobRepeat)
     .FirstOrDefaultAsync(jm =>
         jm.IsActive == true &&
         jm.IsDelete == false &&
         jm.JobId == request.Id &&
         jm.JobRepeatId == request.JobRepeatId); // CÓ THỂ NULL nếu không lặp
            if (jobMapEmployee == null || jobMapEmployee.Job == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy công việc" });
            }

            var job = jobMapEmployee.Job;

            // Tìm bản ghi Score liên quan đến công việc
            var score = await _context.Scoreemployees
      .Include(s => s.JobMapEmployee)
      .ThenInclude(jm => jm.JobRepeat)
      .FirstOrDefaultAsync(s =>
          s.JobMapEmployeeId == jobMapEmployee.Id &&
          s.IsActive == true &&
          s.IsDelete == false);
            //var score = await _context.Scoreemployees
            // .Include(s => s.JobMapEmployee)
            //     .ThenInclude(jm => jm.JobRepeat) // Nếu cần truy cập thêm thông tin Job
            // .FirstOrDefaultAsync(s =>
            //     s.JobMapEmployeeId == jobMapEmployee.Id &&
            //     s.JobMapEmployee.JobRepeatId == s.JobMapEmployee.JobRepeat.Id &&  // Điều kiện so sánh thêm
            //     s.IsActive == true &&
            //     s.IsDelete == false);
            if (score == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy dữ liệu đánh giá công việc" });
            }

            var today = DateOnly.FromDateTime(DateTime.Now);
            bool isPastDeadline = job.Deadline1.HasValue && job.Deadline1.Value < today;

            // Cập nhật tiến độ công việc
            score.Progress = request.Progress;

            // Tính toán VolumeAssessment
            if (score.Progress == null || score.Progress == 0)
            {
                score.VolumeAssessment = 0;
                score.ProgressAssessment = 0;
                score.QualityAssessment = 0;
            }
            else if (score.Progress <= 12.5)
            {
                score.VolumeAssessment = 0.5;
                score.ProgressAssessment = 0.5;
                score.QualityAssessment = 0.5;
            }
            else if (score.Progress <= 25)
            {
                score.VolumeAssessment = 1;
                score.ProgressAssessment = 1;
                score.QualityAssessment = 1;
            }
            else if (score.Progress <= 50)
            {
                score.VolumeAssessment = 1.5;
                score.ProgressAssessment = 1.5;
                score.QualityAssessment = 1.5;
            }
            else if (score.Progress <= 70)
            {
                score.VolumeAssessment = 2;
                score.ProgressAssessment = 2;
                score.QualityAssessment = 2;
            }
            else if (score.Progress >= 100)
            {
                score.VolumeAssessment = 3;
                score.ProgressAssessment = 3;
                score.QualityAssessment = 3;

            }
            else
            {
                score.VolumeAssessment = 2.5;
                score.ProgressAssessment = 2.5;
                score.QualityAssessment = 2.5;
            }

            // Tính SummaryOfReviews
            score.SummaryOfReviews = ((score.VolumeAssessment ?? 0) * 0.6) + ((score.ProgressAssessment ?? 0) * 0.15) + ((score.QualityAssessment ?? 0) * 0.25);

            // Cập nhật trạng thái dựa trên tiến độ và deadline
            if (score.Progress == 100 && !isPastDeadline)
            {
                score.Status = 1; // Hoàn thành đúng hạn
                score.CompletionDate = today;
            }
            else if (isPastDeadline && score.Progress < 100)
            {
                score.Status = 2; // Chưa hoàn thành (quá hạn mà chưa đạt 100%)
                score.CompletionDate = null;
            }
            else if (isPastDeadline && score.Progress == 100)
            {
                score.Status = 3; // Hoàn thành muộn (quá hạn nhưng đã đạt 100%)
                score.CompletionDate = today;
            }
            else if (!isPastDeadline && score.Progress > 0 && score.Progress < 100)
            {
                score.Status = 4; // Đang xử lý (chưa đến hạn nhưng tiến độ > 0)
                score.CompletionDate = null;
            }
            else if (score.Progress == 0 || score.Progress == null)
            {
                score.Status = 0; // Chưa bắt đầu
                score.CompletionDate = null;
            }
            //update progressAssessment
            if (score.Status == 1)
            {
                score.ProgressAssessment = 3;
            }
            else if ((score.Status == 2 && score.Progress >= 50) || score.Status == 4)
            {
                score.ProgressAssessment = 1.5;
            }
            else if (score.Status == 3)
            {
                score.ProgressAssessment = 2;
            }
            else
            {
                score.ProgressAssessment = 0;
            }
            // Lưu thay đổi vào database
            _context.Entry(score).State = EntityState.Modified;
            await UpdateBaselineAssessment(jobMapEmployee.EmployeeId);
            await UpdateAnalysis(jobMapEmployee.EmployeeId);
            await _context.SaveChangesAsync();

            // Trả về phản hồi với các giá trị mới
            return Ok(new
            {
                success = true,
                newStatus = score.Status,
                completionDate = score.CompletionDate?.ToString("dd/MM/yyyy"),
                volumeAssessment = Math.Round(score.VolumeAssessment ?? 0, 1),
                progressAssessment = Math.Round(score.ProgressAssessment ?? 0, 1),
                qualityAssessment = Math.Round(score.QualityAssessment ?? 0, 1),
                summaryOfReviews = Math.Round(score.SummaryOfReviews ?? 0, 2)
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAssessment(long id, string field, float value)
        {
            var score = await _context.Scoreemployees
                .Include(s => s.JobMapEmployee)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (score == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bản ghi." });
            }

            try
            {
                // Update the specified field
                switch (field)
                {
                    case "VolumeAssessment":
                        score.VolumeAssessment = value;
                        break;
                    case "ProgressAssessment":
                        score.ProgressAssessment = value;
                        break;
                    case "QualityAssessment":
                        score.QualityAssessment = value;
                        break;
                    default:
                        return Json(new { success = false, message = "Trường không hợp lệ." });
                }


                // Update SummaryOfReviews automatically
                score.SummaryOfReviews = Math.Round((score.VolumeAssessment ?? 0) * 0.6f +
                                         (score.ProgressAssessment ?? 0) * 0.15f +
                                         (score.QualityAssessment ?? 0) * 0.25f, 2);

                score.UpdateDate = DateTime.Now;
                score.UpdateBy = HttpContext.Session.GetString("EmployeeSystem");

                // Update related assessments if EmployeeId exists
                if (score.JobMapEmployee != null)
                {
                    await UpdateBaselineAssessment(score.JobMapEmployee.EmployeeId);
                    await UpdateAnalysis(score.JobMapEmployee.EmployeeId);
                }

                _context.Update(score);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private async Task UpdateBaselineAssessment(long? employeeId)
        {
            if (employeeId == null)
                return;

            // Lấy tất cả bản ghi Score của nhân viên
            var jobs = await _context.Scoreemployees
                .Where(j => j.JobMapEmployee.EmployeeId == employeeId && j.CreateDate.HasValue)
                .ToListAsync();

            if (!jobs.Any())
                return;

            // Nhóm các bản ghi Score theo tháng và năm của CreateDate
            var groupedJobs = jobs
                .GroupBy(j => new { j.CreateDate.Value.Year, j.CreateDate.Value.Month });

            foreach (var group in groupedJobs)
            {
                var year = group.Key.Year;
                var month = group.Key.Month;

                // Tính tổng các đánh giá cho tháng/năm này
                double sumVolume = group.Sum(j => j.VolumeAssessment ?? 0);
                double sumProgress = group.Sum(j => j.ProgressAssessment ?? 0);
                double sumQuality = group.Sum(j => j.QualityAssessment ?? 0);
                double sumSummary = group.Sum(j => j.SummaryOfReviews ?? 0);
                bool evaluate = sumSummary >= 45;

                // Tìm bản ghi BaselineAssessment cho tháng/năm của CreateDate
                var baseline = await _context.Baselineassessments
                    .FirstOrDefaultAsync(b => b.EmployeeId == employeeId
                                           && b.Time.HasValue
                                           && b.Time.Value.Month == month
                                           && b.Time.Value.Year == year);

                if (baseline == null)
                {
                    // Nếu chưa có bản ghi, tạo mới
                    baseline = new Baselineassessment
                    {
                        EmployeeId = employeeId,
                        VolumeAssessment = sumVolume,
                        ProgressAssessment = sumProgress,
                        QualityAssessment = sumQuality,
                        SummaryOfReviews = sumSummary,
                        Time = new DateTime(year, month, 1), // Dựa trên CreateDate
                        Evaluate = evaluate,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        IsActive = true,
                        IsDelete = false
                    };
                    _context.Baselineassessments.Add(baseline);
                }
                else
                {
                    // Nếu đã có bản ghi, cập nhật dữ liệu
                    baseline.VolumeAssessment = sumVolume;
                    baseline.ProgressAssessment = sumProgress;
                    baseline.QualityAssessment = sumQuality;
                    baseline.SummaryOfReviews = sumSummary;
                    baseline.Evaluate = evaluate;
                    baseline.UpdateDate = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpdateAnalysis(long? employeeId)
        {
            if (employeeId == null)
                return;

            // Lấy tất cả bản ghi Score của nhân viên
            var jobs = await _context.Scoreemployees
                .Where(j => j.JobMapEmployee.EmployeeId == employeeId && j.CreateDate.HasValue)
                .ToListAsync();

            if (!jobs.Any())
                return;

            // Nhóm các bản ghi Score theo tháng và năm của CreateDate
            var groupedJobs = jobs
                .GroupBy(j => new { j.CreateDate.Value.Year, j.CreateDate.Value.Month });

            foreach (var group in groupedJobs)
            {
                var year = group.Key.Year;
                var month = group.Key.Month;

                // Tính số lượng công việc theo trạng thái cho tháng/năm này
                int ontime = group.Count(j => j.Status == 1);
                int late = group.Count(j => j.Status == 2);
                int overdue = group.Count(j => j.Status == 3);
                int processing = group.Count(j => j.Status == 4 || j.Status == 0 || j.Status == 5);
                int total = ontime + late + overdue + processing;

                // Tìm bản ghi Analysis cho tháng/năm của CreateDate
                var analysis = await _context.Analyses
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                                           && a.Time.HasValue
                                           && a.Time.Value.Month == month
                                           && a.Time.Value.Year == year);

                if (analysis == null)
                {
                    // Nếu chưa có bản ghi, tạo mới
                    analysis = new Analysis
                    {
                        EmployeeId = employeeId,
                        Total = total,
                        Ontime = ontime,
                        Late = late,
                        Overdue = overdue,
                        Processing = processing,
                        Time = new DateTime(year, month, 1), // Dựa trên CreateDate
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    _context.Analyses.Add(analysis);
                }
                else
                {
                    // Nếu đã có bản ghi, cập nhật dữ liệu
                    analysis.Total = total;
                    analysis.Ontime = ontime;
                    analysis.Late = late;
                    analysis.Overdue = overdue;
                    analysis.Processing = processing;
                    analysis.UpdateDate = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
        }


        private bool JobExists(long id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }

    }
}
