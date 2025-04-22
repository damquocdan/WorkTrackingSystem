using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using WorkTrackingSystem.Models;
using X.PagedList.Extensions;
using WorkTrackingSystem.Areas.ProjectManager.Models;

namespace WorkTrackingSystem.Areas.AdminSystem.Controllers
{
    [Area("AdminSystem")]
    public class BaselineassessmentsController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public BaselineassessmentsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }
        // GET: AdminSystem/Baselineassessments
        public async Task<IActionResult> Index(
             string employeeName,
             string evaluate,
             string time,
             int page = 1)
        {
            var limit = 10;
         
            var managedEmployeeIds = await _context.Employees
             
                .Select(e => e.Id)
                .ToListAsync();

            var assessments = _context.Baselineassessments
                .Include(b => b.Employee)
                .Where(b => b.EmployeeId.HasValue && managedEmployeeIds.Contains(b.EmployeeId.Value));

            // Set default time to current month if not provided
            if (string.IsNullOrEmpty(time))
            {
                time = DateTime.Now.ToString("yyyy-MM");
            }

            // Format display month for the view
            string displayMonth = "Toàn bộ";
            if (DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedTime))
            {
                displayMonth = $"Tháng {selectedTime:MM/yyyy}";
            }
            ViewBag.DisplayMonth = displayMonth;


            if (!string.IsNullOrEmpty(employeeName))
            {
                assessments = assessments.Where(b => b.Employee.FirstName.Contains(employeeName) || b.Employee.LastName.Contains(employeeName));
            }

            if (!string.IsNullOrEmpty(evaluate) && bool.TryParse(evaluate, out bool evaluateVal))
            {
                assessments = assessments.Where(b => b.Evaluate == evaluateVal);
            }

            if (!string.IsNullOrEmpty(time) && DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedTimeForFilter))
            {
                assessments = assessments.Where(b => b.Time.HasValue &&
                                                    b.Time.Value.Year == selectedTimeForFilter.Year &&
                                                    b.Time.Value.Month == selectedTimeForFilter.Month);
            }

            var assessmentsList = await assessments.ToListAsync();
            if (!assessmentsList.Any())
            {
                TempData["NoDataMessage"] = "Không có dữ liệu để hiển thị hoặc xuất Excel.";
            }

            ViewBag.EmployeeName = employeeName;
            ViewBag.Evaluate = evaluate;
            ViewBag.Time = time;

            return View(assessments.OrderBy(x => x.EmployeeId).ToPagedList(page, limit));
        }

        public async Task<IActionResult> JobEvaluation(
string searchText = "",
string month = "",
string day = "",
string status = "",
string categoryId = "",
string sortOrder = "",
bool showCompletedZeroReview = false,
bool dueToday = false,
long? jobId = null, int page = 1)
        {
            var limit = 8;
           

            var employeesInManagedDepartments = await _context.Employees
                //.Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value))
                .Select(e => new { Value = e.Id.ToString(), Text = $"{e.Code} {e.FirstName} {e.LastName}", Avatar = e.Avatar ?? "/images/default-avatar.png" })
                .ToListAsync();
            ViewBag.EmployeeList = employeesInManagedDepartments;

            ViewData["Categories"] = await _context.Categories.ToListAsync();

            // Lấy danh sách JobId đã được gán cho bất kỳ nhân viên nào (EmployeeId không null)
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
                .Where(s => s.JobMapEmployee != null && s.JobMapEmployee.Job != null)
                .Where(s =>
                    (s.JobMapEmployee.EmployeeId.HasValue && employeesInManagedDepartments.Select(e => long.Parse(e.Value)).Contains(s.JobMapEmployee.EmployeeId.Value))
                    || (s.JobMapEmployee.EmployeeId == null  && !assignedJobIds.Contains(s.JobMapEmployee.JobId)));

            // Apply the filter only if jobId is provided (keeping the original jobId-specific logic)
            if (jobId.HasValue)
            {
                bool hasAssignedJob = await _context.Jobmapemployees
                    .Where(jme => jme.JobId == jobId.Value && jme.EmployeeId.HasValue)
                    .AnyAsync();

                if (hasAssignedJob)
                {
                    scoresQuery = scoresQuery.Where(s =>
                        !(s.JobMapEmployee.JobId == jobId.Value && s.JobMapEmployee.EmployeeId == null));
                }
            }

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
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Year == selectedMonth.Year && s.CreateDate.Value.Month == selectedMonth.Month);
            }
            if (!string.IsNullOrEmpty(day) && DateTime.TryParse(day, out DateTime selectedDay))
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Date == selectedDay.Date);
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
                scoresQuery = scoresQuery.Where(s => s.SummaryOfReviews == 0);
            }
            if (dueToday)
            {
                scoresQuery = scoresQuery.Where(s => s.Time == s.CreateDate);
            }
            var scores = scoresQuery.ToPagedList(page, limit);
            ViewBag.SearchText = searchText;
            ViewBag.Month = month;
            ViewBag.Day = day;
            ViewBag.Status = status?.ToString();
            ViewBag.CategoryId = categoryId?.ToString();
            ViewBag.SortOrder = sortOrder;
            ViewBag.ShowCompletedZeroReview = showCompletedZeroReview;
            ViewBag.DueToday = dueToday;
            return View(scores);
        }

        public async Task<IActionResult> ExportToExcel(string employeeCode, string employeeName, bool? evaluate, string time)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

           
            var assessments = _context.Baselineassessments
                .Include(b => b.Employee)
                .Where(b => b.Employee != null &&
                            b.Employee.DepartmentId.HasValue 
                            );

            if (!string.IsNullOrEmpty(employeeCode))
            {
                assessments = assessments.Where(b => b.Employee.Code.Contains(employeeCode));
            }

            if (!string.IsNullOrEmpty(employeeName))
            {
                assessments = assessments.Where(b => b.Employee.FirstName.Contains(employeeName) || b.Employee.LastName.Contains(employeeName));
            }

            if (evaluate.HasValue)
            {
                assessments = assessments.Where(b => b.Evaluate == evaluate.Value);
            }
            string selectedMonth = "Toàn bộ";
            if (!string.IsNullOrEmpty(time))
            {
                if (DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedTime))
                {
                    selectedMonth = selectedTime.ToString("MM/yyyy");
                    assessments = assessments.Where(b => b.Time.HasValue &&
                                                         b.Time.Value.Year == selectedTime.Year &&
                                                         b.Time.Value.Month == selectedTime.Month);
                }
            }

            var assessmentList = await assessments.OrderByDescending(x => x.Time).ToListAsync();
            if (!assessmentList.Any())
            {
                TempData["NoDataMessage"] = "Không có dữ liệu để xuất Excel.";
                return RedirectToAction("Index");
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh sách đánh giá");

                worksheet.Cells[1, 1, 1, 7].Merge = true;
                worksheet.Cells[1, 1].Value = $"Bảng tổng hợp đánh giá tháng {selectedMonth}";
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[2, 1].Value = "Mã nhân viên";
                worksheet.Cells[2, 2].Value = "Tên nhân viên";
                worksheet.Cells[2, 3].Value = "Tổng đánh giá khối lượng";
                worksheet.Cells[2, 4].Value = "Tổng đánh giá tiến độ";
                worksheet.Cells[2, 5].Value = "Tổng đánh giá chất lượng";
                worksheet.Cells[2, 6].Value = "Tổng đánh giá tổng hợp";
                worksheet.Cells[2, 7].Value = "Đánh giá theo baseline";

                using (var range = worksheet.Cells[2, 1, 2, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                int row = 3;
                foreach (var item in assessmentList)
                {
                    worksheet.Cells[row, 1].Value = item.Employee.Code;
                    worksheet.Cells[row, 2].Value = $"{item.Employee.FirstName} {item.Employee.LastName}";
                    worksheet.Cells[row, 3].Value = item.VolumeAssessment;
                    worksheet.Cells[row, 4].Value = item.ProgressAssessment;
                    worksheet.Cells[row, 5].Value = item.QualityAssessment;
                    worksheet.Cells[row, 6].Value = item.SummaryOfReviews;
                    worksheet.Cells[row, 7].Value = item.Evaluate.GetValueOrDefault() ? "Đạt baseline" : "Chưa đạt baseline";

                    worksheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 3, row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                if (assessmentList.Any())
                {
                    var chart = worksheet.Drawings.AddChart("PieChart", eChartType.Pie3D) as ExcelPieChart;
                    chart.SetPosition(1, 0, 9, 0);
                    chart.SetSize(500, 350);

                    int totalColumnIndex = 3;
                    int nameColumnIndex = 2;

                    var dataRange = worksheet.Cells[3, totalColumnIndex, assessmentList.Count + 2, totalColumnIndex];
                    var labelRange = worksheet.Cells[3, nameColumnIndex, assessmentList.Count + 2, nameColumnIndex];

                    var series = chart.Series.Add(dataRange, labelRange) as ExcelPieChartSerie;
                    chart.Title.Text = "Đánh giá khối lượng";
                    chart.Legend.Position = eLegendPosition.Left;

                    if (series != null)
                    {
                        series.DataLabel.ShowPercent = true;
                        series.DataLabel.Position = eLabelPosition.Center;
                    }
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachDanhGia.xlsx");
            }
        }

        public async Task<IActionResult> EmployeeScoreSummary(string search, int? departmentId, string timeType, DateTime? fromDate, DateTime? toDate, string time, int? quarter, int? year, string sortOrder, string evaluate, int page = 1)
        {
            var limit = 10;
            // Handle time parameters
            int? month = null;
            if (timeType == "month" && !string.IsNullOrEmpty(time) && DateTime.TryParse(time + "-01", out var parsedTime))
            {
                month = parsedTime.Month;
                year = parsedTime.Year;
                fromDate = new DateTime(parsedTime.Year, parsedTime.Month, 1);
                toDate = fromDate.Value.AddMonths(1).AddDays(-1);
            }
            else if (timeType == "quarter" && quarter.HasValue && year.HasValue)
            {
                fromDate = new DateTime(year.Value, (quarter.Value - 1) * 3 + 1, 1);
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
                month = null;
                year = null;
            }

            // Query to aggregate scores by employee, restricted to managed departments
            var employeeScoresQuery = from s in _context.Scores
                                      join jme in _context.Jobmapemployees on s.JobMapEmployeeId equals jme.Id
                                      join e in _context.Employees on jme.EmployeeId equals e.Id
                                      join d in _context.Departments on e.DepartmentId equals d.Id
                                      where  // Restrict to managed departments
                                             departmentId == null || d.Id == departmentId // Filter by selected department
                                            && (fromDate == null || s.CreateDate >= fromDate)
                                            && (toDate == null || s.CreateDate <= toDate)
                                      group new { s, e } by new { e.Id, e.FirstName, e.LastName } into g
                                      select new ScoreSummary
                                      {
                                          EmployeeId = (int)g.Key.Id,
                                          EmployeeName = (g.Key.FirstName + " " + g.Key.LastName).Trim(),
                                          TotalVolume = (int)(g.Sum(x => x.s.VolumeAssessment ?? 0)),
                                          TotalProgress = (int)(g.Sum(x => x.s.ProgressAssessment ?? 0)),
                                          TotalQuality = (int)(g.Sum(x => x.s.QualityAssessment ?? 0)),
                                          SummaryScore = g.Sum(x =>
                                              (decimal)(x.s.VolumeAssessment ?? 0) * 0.6m +
                                              (decimal)(x.s.ProgressAssessment ?? 0) * 0.15m +
                                              (decimal)(x.s.QualityAssessment ?? 0) * 0.25m),
                                          EvaluationResult = g.Sum(x =>
                                              (decimal)(x.s.VolumeAssessment ?? 0) * 0.6m +
                                              (decimal)(x.s.ProgressAssessment ?? 0) * 0.15m +
                                              (decimal)(x.s.QualityAssessment ?? 0) * 0.25m) >= 4.5m ? "Đạt" : "Chưa đạt"
                                      };

            // Execute query and apply additional filters
            var employeeScores = await employeeScoresQuery.ToListAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                employeeScores = employeeScores
                    .Where(e => e.EmployeeName.ToLower().Contains(search))
                    .ToList();
            }

            // Apply evaluation filter
            if (!string.IsNullOrEmpty(evaluate))
            {
                employeeScores = employeeScores
                    .Where(e => e.EvaluationResult == evaluate)
                    .ToList();
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "name_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.EmployeeName).ToList();
                    break;
                case "volume_asc":
                    employeeScores = employeeScores.OrderBy(e => e.TotalVolume).ToList();
                    break;
                case "volume_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.TotalVolume).ToList();
                    break;
                case "progress_asc":
                    employeeScores = employeeScores.OrderBy(e => e.TotalProgress).ToList();
                    break;
                case "progress_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.TotalProgress).ToList();
                    break;
                case "quality_asc":
                    employeeScores = employeeScores.OrderBy(e => e.TotalQuality).ToList();
                    break;
                case "quality_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.TotalQuality).ToList();
                    break;
                case "summary_asc":
                    employeeScores = employeeScores.OrderBy(e => e.SummaryScore).ToList();
                    break;
                case "summary_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.SummaryScore).ToList();
                    break;
                default:
                    employeeScores = employeeScores.OrderBy(e => e.EmployeeName).ToList();
                    break;
            }

            // Pagination
            var pagedEmployeeScores = employeeScores.ToPagedList(page, limit);

            // Prepare view data
            ViewBag.Search = search;
            ViewBag.DepartmentId = departmentId;
            ViewBag.TimeType = timeType ?? "total";
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Time = time;
            ViewBag.Quarter = quarter;
            ViewBag.Year = year;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Evaluate = evaluate;
            ViewBag.DepartmentName = departmentId.HasValue
                ? _context.Departments.Where(d => d.Id == departmentId).Select(d => d.Name).FirstOrDefault() ?? "All Departments"
                : "All Departments";
            //ViewBag.Departments = new SelectList(_context.Departments.Where(d => managedDepartments.Contains(d.Id)), "Id", "Name");

            return View(pagedEmployeeScores);
        }
        public async Task<IActionResult> ExportToExcelScore(string search, string evaluate, string timeType, string time, int? quarter, int? year, string fromDate, string toDate)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

      

            // Handle time parameters
            DateTime? parsedFromDate = null;
            DateTime? parsedToDate = null;
            string selectedPeriod = "Toàn bộ";

            if (timeType == "month" && !string.IsNullOrEmpty(time) && DateTime.TryParse(time + "-01", out var parsedTime))
            {
                parsedFromDate = new DateTime(parsedTime.Year, parsedTime.Month, 1);
                parsedToDate = parsedFromDate.Value.AddMonths(1).AddDays(-1);
                selectedPeriod = parsedTime.ToString("MM/yyyy");
            }
            else if (timeType == "quarter" && quarter.HasValue && year.HasValue)
            {
                parsedFromDate = new DateTime(year.Value, (quarter.Value - 1) * 3 + 1, 1);
                parsedToDate = parsedFromDate.Value.AddMonths(3).AddDays(-1);
                selectedPeriod = $"Quý {quarter}/{year}";
            }
            else if (timeType == "year" && year.HasValue)
            {
                parsedFromDate = new DateTime(year.Value, 1, 1);
                parsedToDate = new DateTime(year.Value, 12, 31);
                selectedPeriod = $"Năm {year}";
            }
            else if (timeType == "dateRange" && !string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate) &&
                     DateTime.TryParse(fromDate, out var parsedFrom) && DateTime.TryParse(toDate, out var parsedTo))
            {
                parsedFromDate = parsedFrom;
                parsedToDate = parsedTo;
                selectedPeriod = $"Từ {parsedFrom.ToString("dd/MM/yyyy")} Đến {parsedTo.ToString("dd/MM/yyyy")}";
            }
            else
            {
                // Default to "Toàn bộ" if no valid time filter is provided
                timeType = "total";
            }

            // Query to aggregate scores by employee
            var employeeScoresQuery = from s in _context.Scores
                                      join jme in _context.Jobmapemployees on s.JobMapEmployeeId equals jme.Id
                                      join e in _context.Employees on jme.EmployeeId equals e.Id
                                      join d in _context.Departments on e.DepartmentId equals d.Id
                                      where( (parsedFromDate == null || s.CreateDate >= parsedFromDate) &&
                                            (parsedToDate == null || s.CreateDate <= parsedToDate))
                                      group new { s, e } by new { e.Id, e.FirstName, e.LastName } into g
                                      select new ScoreSummary
                                      {
                                          EmployeeId = (int)g.Key.Id,
                                          EmployeeName = (g.Key.FirstName + " " + g.Key.LastName).Trim(),
                                          TotalVolume = (int)(g.Sum(x => x.s.VolumeAssessment ?? 0)),
                                          TotalProgress = (int)(g.Sum(x => x.s.ProgressAssessment ?? 0)),
                                          TotalQuality = (int)(g.Sum(x => x.s.QualityAssessment ?? 0)),
                                          SummaryScore = g.Sum(x =>
                                              (decimal)(x.s.VolumeAssessment ?? 0) * 0.6m +
                                              (decimal)(x.s.ProgressAssessment ?? 0) * 0.15m +
                                              (decimal)(x.s.QualityAssessment ?? 0) * 0.25m),
                                          EvaluationResult = g.Sum(x =>
                                              (decimal)(x.s.VolumeAssessment ?? 0) * 0.6m +
                                              (decimal)(x.s.ProgressAssessment ?? 0) * 0.15m +
                                              (decimal)(x.s.QualityAssessment ?? 0) * 0.25m) >= 4.5m ? "Đạt" : "Chưa đạt"
                                      };

            // Apply search and evaluation filters
            var employeeScores = await employeeScoresQuery.ToListAsync();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                employeeScores = employeeScores
                    .Where(e => e.EmployeeName.ToLower().Contains(search))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(evaluate))
            {
                employeeScores = employeeScores
                    .Where(e => e.EvaluationResult == evaluate)
                    .ToList();
            }

            // Order by EmployeeId for consistency
            employeeScores = employeeScores.OrderBy(x => x.EmployeeId).ToList();

            if (!employeeScores.Any())
            {
                TempData["NoDataMessage"] = "Không có dữ liệu để xuất Excel.";
                return RedirectToAction("EmployeeScoreSummary");
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh sách đánh giá");

                // Title
                worksheet.Cells[1, 1, 1, 7].Merge = true;
                worksheet.Cells[1, 1].Value = $"Bảng tổng hợp đánh giá {selectedPeriod}";
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Headers
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Cells[2, 2].Value = "Nhân viên";
                worksheet.Cells[2, 3].Value = "Tổng đánh giá khối lượng";
                worksheet.Cells[2, 4].Value = "Tổng đánh giá tiến độ";
                worksheet.Cells[2, 5].Value = "Tổng đánh giá chất lượng";
                worksheet.Cells[2, 6].Value = "Tổng đánh giá tổng hợp";
                worksheet.Cells[2, 7].Value = "Đánh giá";

                using (var range = worksheet.Cells[2, 1, 2, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Data
                int row = 3;
                int index = 0;
                foreach (var item in employeeScores)
                {
                    index++;
                    worksheet.Cells[row, 1].Value = index;
                    worksheet.Cells[row, 2].Value = item.EmployeeName;
                    worksheet.Cells[row, 3].Value = item.TotalVolume;
                    worksheet.Cells[row, 4].Value = item.TotalProgress;
                    worksheet.Cells[row, 5].Value = item.TotalQuality;
                    worksheet.Cells[row, 6].Value = Math.Round(item.SummaryScore, 2);
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "0.00";
                    worksheet.Cells[row, 7].Value = item.EvaluationResult;
                    worksheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 3, row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Add pie chart
                if (employeeScores.Any())
                {
                    var chart = worksheet.Drawings.AddChart("PieChart", eChartType.Pie3D) as ExcelPieChart;
                    chart.SetPosition(1, 0, 9, 0);
                    chart.SetSize(500, 350);

                    var dataRange = worksheet.Cells[3, 3, employeeScores.Count + 2, 3];
                    var labelRange = worksheet.Cells[3, 2, employeeScores.Count + 2, 2];

                    var series = chart.Series.Add(dataRange, labelRange) as ExcelPieChartSerie;
                    chart.Title.Text = "Đánh giá khối lượng";
                    chart.Legend.Position = eLegendPosition.Left;

                    if (series != null)
                    {
                        series.DataLabel.ShowPercent = true;
                        series.DataLabel.Position = eLabelPosition.Center;
                    }
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DanhSachDanhGia_{selectedPeriod.Replace("/", "_").Replace(" ", "_")}.xlsx");
            }
        }

        // GET: AdminSystem/Baselineassessments/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baselineassessment = await _context.Baselineassessments
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (baselineassessment == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details", baselineassessment);
            }
            return View(baselineassessment);
        }

        // GET: AdminSystem/Baselineassessments/Create
        public IActionResult Create()
        {
            // Lấy thông tin quản lý đang đăng nhập từ session
            var managerUsername = HttpContext.Session.GetString("AdminLogin");

            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            // Tìm nhân viên (quản lý) đang đăng nhập
            var manager = _context.Users
                .Where(u => u.UserName == managerUsername)
                .Select(u => u.Employee)
                .FirstOrDefault();

            if (manager == null || manager.DepartmentId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy danh sách nhân viên thuộc phòng ban của quản lý
            // Lấy tháng và năm hiện tại
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Lấy danh sách nhân viên chưa có đánh giá trong tháng hiện tại
            var employees = _context.Employees
                .Where(e => e.DepartmentId == manager.DepartmentId &&
                            !_context.Baselineassessments.Any(b => b.EmployeeId == e.Id &&
                                                                   b.Time.HasValue &&
                                                                   b.Time.Value.Month == currentMonth &&
                                                                   b.Time.Value.Year == currentYear))
                .Select(e => new
                {
                    Id = e.Id,
                    FullName = e.Code + " - " + e.FirstName + " " + e.LastName // Hiển thị họ và tên đầy đủ
                })
                .ToList();


            // Truyền danh sách nhân viên vào ViewData để hiển thị trong dropdown
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName");

            return View();
        }


        // POST: AdminSystem/Baselineassessments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,Evaluate,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Baselineassessment baselineassessment)
        {
            if (ModelState.IsValid)
            {
                baselineassessment.SummaryOfReviews = (baselineassessment.VolumeAssessment * 0.6) +
                                         (baselineassessment.ProgressAssessment * 0.15) +
                                         (baselineassessment.QualityAssessment * 0.25);

                // Nếu tổng điểm đánh giá > 50, Evaluate = true
                baselineassessment.Evaluate = baselineassessment.SummaryOfReviews > 45;
                baselineassessment.Time = DateTime.Now;
                baselineassessment.CreateDate = DateTime.Now;
                baselineassessment.IsActive = true;
                baselineassessment.IsDelete = false;
                _context.Add(baselineassessment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", baselineassessment.EmployeeId);
            return View(baselineassessment);
        }

        // GET: AdminSystem/Baselineassessments/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baselineassessment = await _context.Baselineassessments
                .Include(b => b.Employee) // Load Employee để lấy tên
                .FirstOrDefaultAsync(b => b.Id == id);

            if (baselineassessment == null)
            {
                return NotFound();
            }

            // Lấy tên nhân viên để hiển thị
            ViewBag.EmployeeName = baselineassessment.Employee.Code + " - " + baselineassessment.Employee.FirstName + " " + baselineassessment.Employee.LastName;

            return View(baselineassessment);
        }


        // POST: AdminSystem/Baselineassessments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,EmployeeId,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,Evaluate,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Baselineassessment baselineassessment)
        {
            if (id != baselineassessment.Id)
            {
                return NotFound();
            }

            var existingRecord = await _context.Baselineassessments.FindAsync(id);
            if (existingRecord == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Chỉ cập nhật các trường cần thiết, giữ nguyên giá trị ban đầu của một số trường
                    existingRecord.VolumeAssessment = baselineassessment.VolumeAssessment;
                    existingRecord.ProgressAssessment = baselineassessment.ProgressAssessment;
                    existingRecord.QualityAssessment = baselineassessment.QualityAssessment;
                    existingRecord.SummaryOfReviews = (baselineassessment.VolumeAssessment * 0.6) +
                                                      (baselineassessment.ProgressAssessment * 0.15) +
                                                      (baselineassessment.QualityAssessment * 0.25);

                    // Nếu tổng điểm đánh giá > 50, Evaluate = true
                    existingRecord.Evaluate = existingRecord.SummaryOfReviews > 45;
                    existingRecord.UpdateDate = DateTime.Now;

                    string userIdStr = HttpContext.Session.GetString("AdminLogin");
                    if (long.TryParse(userIdStr, out long userId))
                    {
                        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                        if (user != null && user.EmployeeId.HasValue)
                        {
                            existingRecord.UpdateBy = HttpContext.Session.GetString("AdminLogin");
                        }
                    }

                    // Giữ nguyên các giá trị không thay đổi
                    // existingRecord.Time = existingRecord.Time; // Không cần vì không cập nhật lại
                    // existingRecord.IsDelete = existingRecord.IsDelete;
                    // existingRecord.IsActive = existingRecord.IsActive;
                    // existingRecord.CreateDate = existingRecord.CreateDate;
                    // existingRecord.CreateBy = existingRecord.CreateBy;

                    _context.Update(existingRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BaselineassessmentExists(baselineassessment.Id))
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

            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", baselineassessment.EmployeeId);
            return View(baselineassessment);
        }

        // GET: AdminSystem/Baselineassessments/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baselineassessment = await _context.Baselineassessments
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (baselineassessment == null)
            {
                return NotFound();
            }

            return View(baselineassessment);
        }

        // POST: AdminSystem/Baselineassessments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var baselineassessment = await _context.Baselineassessments.FindAsync(id);
            if (baselineassessment != null)
            {
                _context.Baselineassessments.Remove(baselineassessment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignEmployee(long jobId, long employeeId, string time, string deadline)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null)
            {
                return Json(new { success = false, message = "Job not found" });
            }

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            // Tạo JobMapEmployee
            var jobMapEmployee = new Jobmapemployee
            {
                JobId = jobId,
                EmployeeId = employeeId,
                CreateBy = HttpContext.Session.GetString("AdminLogin"),
                CreateDate = DateTime.Now,
                IsActive = true,
                IsDelete = false
            };

            _context.Jobmapemployees.Add(jobMapEmployee);
            await _context.SaveChangesAsync();

            // Xử lý ngày bắt đầu và ngày kết thúc
            DateTime? parsedTime = null;
            DateOnly? parsedDeadline = null;

            if (!string.IsNullOrEmpty(time) && DateTime.TryParseExact(time, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime startDate))
            {
                parsedTime = startDate;
            }

            if (!string.IsNullOrEmpty(deadline) && DateOnly.TryParseExact(deadline, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateOnly endDate))
            {
                parsedDeadline = endDate;
                job.Deadline1 = parsedDeadline;
                _context.Jobs.Update(job);
            }

            // Tạo Score
            var score = new Score
            {
                JobMapEmployeeId = jobMapEmployee.Id,
                Time = parsedTime ?? DateTime.Now,
                Status = 0,
                CreateDate = job.Deadline1?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                CreateBy = HttpContext.Session.GetString("AdminLogin")
            };
            _context.Scores.Add(score);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAssessment(long id, string field, float value)
        {
            var score = await _context.Scores
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
                score.SummaryOfReviews = (score.VolumeAssessment ?? 0) * 0.6f +
                                         (score.ProgressAssessment ?? 0) * 0.15f +
                                         (score.QualityAssessment ?? 0) * 0.25f;

                score.UpdateDate = DateTime.Now;
                score.UpdateBy = HttpContext.Session.GetString("AdminLogin");

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
            var jobs = await _context.Scores
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
            var jobs = await _context.Scores
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

        private bool BaselineassessmentExists(long id)
        {
            return _context.Baselineassessments.Any(e => e.Id == id);
        }
    }
}