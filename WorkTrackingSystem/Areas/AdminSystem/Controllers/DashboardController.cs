using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using WorkTrackingSystem.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;

namespace WorkTrackingSystem.Areas.AdminSystem.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public DashboardController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string day = "",
            string fromDate = "",
            string toDate = "",
            string month = "",
            string quarter = "",
            string year = "",
            string departmentId = "")
        {
            // Store filter state in ViewBag
            ViewBag.Day = day;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Month = month;
            ViewBag.Quarter = quarter;
            ViewBag.Year = year;
            ViewBag.DepartmentId = departmentId;

            // Check if filtering by date range
            bool isDateRange = !string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate);
            ViewBag.Date1 = fromDate;
            ViewBag.Date2 = toDate;

            // Determine time period and grouping based on CreateDate
            DateTime? filterFromDate = null;
            DateTime? filterToDate = null;
            bool isDailyGrouping = false;

            if (isDateRange && DateTime.TryParse(fromDate, out DateTime startDate) && DateTime.TryParse(toDate, out DateTime endDate))
            {
                filterFromDate = startDate;
                filterToDate = endDate;
                isDailyGrouping = true;
            }
            else if (!string.IsNullOrEmpty(day) && DateTime.TryParse(day, out DateTime selectedDay))
            {
                filterFromDate = selectedDay;
                filterToDate = selectedDay;
                isDailyGrouping = true;
            }
            else if (!string.IsNullOrEmpty(quarter) && int.TryParse(quarter, out int selectedQuarter) && !string.IsNullOrEmpty(year) && int.TryParse(year, out int selectedYear))
            {
                filterFromDate = new DateTime(selectedYear, (selectedQuarter - 1) * 3 + 1, 1);
                filterToDate = filterFromDate.Value.AddMonths(3).AddDays(-1);
                isDailyGrouping = false;
            }
            else if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out DateTime selectedMonth))
            {
                int filterYear = !string.IsNullOrEmpty(year) && int.TryParse(year, out int y) ? y : DateTime.Now.Year;
                filterFromDate = new DateTime(filterYear, selectedMonth.Month, 1);
                filterToDate = filterFromDate.Value.AddMonths(1).AddDays(-1);
                isDailyGrouping = true;
            }
            else if (!string.IsNullOrEmpty(year) && int.TryParse(year, out int selectedYearForYear))
            {
                filterFromDate = new DateTime(selectedYearForYear, 1, 1);
                filterToDate = new DateTime(selectedYearForYear, 12, 31);
                isDailyGrouping = false;
            }

            // Default to monthly/yearly grouping if no filters are applied
            if (string.IsNullOrEmpty(day) && string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate) &&
                string.IsNullOrEmpty(month) && string.IsNullOrEmpty(quarter) && string.IsNullOrEmpty(year))
            {
                isDailyGrouping = false;
            }

            // Get all departments for filter dropdown
            var departments = await _context.Departments
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();
            ViewBag.Departments = departments;

            // Query scores, restricted to selected department(s) or all if none selected
            var scoresQuery = _context.Scores
                .Include(s => s.JobMapEmployee)
                .ThenInclude(jme => jme.Job)
                .Include(s => s.JobMapEmployee)
                .ThenInclude(jme => jme.Employee)
                .ThenInclude(e => e.Department)
                .Where(s => s.CreateDate.HasValue &&
                            s.JobMapEmployee.Job.IsActive == true &&
                            s.JobMapEmployee.Job.IsDelete == false &&
                            s.JobMapEmployee.Employee != null &&
                            s.JobMapEmployee.Employee.DepartmentId.HasValue);

            // Apply department filter if specified
            if (!string.IsNullOrEmpty(departmentId) && int.TryParse(departmentId, out int deptId))
            {
                scoresQuery = scoresQuery.Where(s => s.JobMapEmployee.Employee.DepartmentId == deptId);
            }

            // Apply time filters based on CreateDate
            if (filterFromDate.HasValue && filterToDate.HasValue)
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.Value.Date >= filterFromDate.Value.Date && s.CreateDate.Value.Date <= filterToDate.Value.Date);
            }

            // Overview statistics
            var totalJobs = await _context.Jobs.CountAsync(j => j.IsActive == true && j.IsDelete == false);
            var completedJobs = await scoresQuery.CountAsync(s => s.Status == 1);
            var overdueJobs = await scoresQuery.CountAsync(s => s.Status == 2);
            var totalCategories = await _context.Categories.CountAsync(c => c.IsActive == true && c.IsDelete == false);

            // Job status for pie chart
            var jobStatusOntime = await scoresQuery.CountAsync(s => s.Status == 1);
            var jobStatusOverdue = await scoresQuery.CountAsync(s => s.Status == 2);
            var jobStatusLate = await scoresQuery.CountAsync(s => s.Status == 3);
            var jobStatusProcessing = await scoresQuery.CountAsync(s => s.Status == 4);

            // Jobs by period (monthly/yearly or daily)
            var jobsByPeriod = await scoresQuery
                .Select(s => new { s.CreateDate })
                .ToListAsync();

            var jobsByPeriodGrouped = isDailyGrouping
                ? jobsByPeriod
                    .GroupBy(s => new { s.CreateDate.Value.Year, s.CreateDate.Value.Month, s.CreateDate.Value.Day })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Day:D2}/{g.Key.Month:D2}/{g.Key.Year}",
                        TotalJobs = g.Count()
                    })
                    .OrderBy(g => DateTime.ParseExact(g.Period, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                : jobsByPeriod
                    .GroupBy(s => new { s.CreateDate.Value.Year, s.CreateDate.Value.Month })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Month:D2}/{g.Key.Year}",
                        TotalJobs = g.Count()
                    })
                    .OrderBy(g => DateTime.ParseExact(g.Period, "MM/yyyy", CultureInfo.InvariantCulture));

            var jobMonths = jobsByPeriodGrouped.Select(j => j.Period).ToList();
            var jobCounts = jobsByPeriodGrouped.Select(j => j.TotalJobs).ToList();

            // Calendar data with status
            var calendarJobs = await scoresQuery
                .GroupBy(s => new { s.CreateDate.Value.Date, s.Status })
                .Select(g => new
                {
                    title = $"{g.Count()} CV đến hạn",
                    start = g.Key.Date.ToString("yyyy-MM-dd"),
                    status = g.Key.Status
                })
                .ToListAsync();

            // Score summary for charts
            var scoreSummaryRaw = await scoresQuery
                .Select(s => new
                {
                    CreateDate = s.CreateDate.Value,
                    s.VolumeAssessment,
                    s.ProgressAssessment,
                    s.QualityAssessment
                })
                .ToListAsync();

            var scoreSummary = isDailyGrouping
                ? scoreSummaryRaw
                    .GroupBy(s => new { s.CreateDate.Year, s.CreateDate.Month, s.CreateDate.Day })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Day:D2}/{g.Key.Month:D2}/{g.Key.Year}",
                        TotalVolume = g.Sum(s => s.VolumeAssessment ?? 0),
                        TotalProgress = g.Sum(s => s.ProgressAssessment ?? 0),
                        TotalQuality = g.Sum(s => s.QualityAssessment ?? 0),
                        SummaryScore = g.Sum(s =>
                            (decimal)(s.VolumeAssessment ?? 0) * 0.6m +
                            (decimal)(s.ProgressAssessment ?? 0) * 0.15m +
                            (decimal)(s.QualityAssessment ?? 0) * 0.25m)
                    })
                    .OrderBy(g => DateTime.ParseExact(g.Period, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                : scoreSummaryRaw
                    .GroupBy(s => new { s.CreateDate.Year, s.CreateDate.Month })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Month:D2}/{g.Key.Year}",
                        TotalVolume = g.Sum(s => s.VolumeAssessment ?? 0),
                        TotalProgress = g.Sum(s => s.ProgressAssessment ?? 0),
                        TotalQuality = g.Sum(s => s.QualityAssessment ?? 0),
                        SummaryScore = g.Sum(s =>
                            (decimal)(s.VolumeAssessment ?? 0) * 0.6m +
                            (decimal)(s.ProgressAssessment ?? 0) * 0.15m +
                            (decimal)(s.QualityAssessment ?? 0) * 0.25m)
                    })
                    .OrderBy(g => DateTime.ParseExact(g.Period, "MM/yyyy", CultureInfo.InvariantCulture));

            var labels = scoreSummary.Select(s => s.Period).ToList();
            var sumVolume = scoreSummary.Select(s => s.TotalVolume).ToList();
            var sumProgress = scoreSummary.Select(s => s.TotalProgress).ToList();
            var sumQuality = scoreSummary.Select(s => s.TotalQuality).ToList();
            var sumSummary = scoreSummary.Select(s => s.SummaryScore).ToList();

            // Handle AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    day,
                    fromDate,
                    toDate,
                    month,
                    quarter,
                    year,
                    departmentId,
                    isDateRange,
                    labels,
                    sumVolume,
                    sumProgress,
                    sumQuality,
                    sumSummary,
                    jobMonths,
                    jobCounts,
                    jobStatusOntime,
                    jobStatusOverdue,
                    jobStatusLate,
                    jobStatusProcessing,
                    calendarJobs,
                    totalJobs,
                    completedJobs,
                    overdueJobs,
                    totalCategories
                });
            }

            // For non-AJAX requests, populate ViewBag
            ViewBag.Labels = labels;
            ViewBag.SumVolume = sumVolume;
            ViewBag.SumProgress = sumProgress;
            ViewBag.SumQuality = sumQuality;
            ViewBag.SumSummary = sumSummary;
            ViewBag.JobMonths = jobMonths;
            ViewBag.JobCounts = jobCounts;
            ViewBag.JobStatusOntime = jobStatusOntime;
            ViewBag.JobStatusOverdue = jobStatusOverdue;
            ViewBag.JobStatusLate = jobStatusLate;
            ViewBag.JobStatusProcessing = jobStatusProcessing;
            ViewBag.CalendarJobs = JsonConvert.SerializeObject(calendarJobs);
            ViewBag.TotalJobs = totalJobs;
            ViewBag.CompletedJobs = completedJobs;
            ViewBag.OverdueJobs = overdueJobs;
            ViewBag.TotalCategories = totalCategories;

            return View();
        }

        public async Task<IActionResult> ExportToExcel(
            string day = "",
            string fromDate = "",
            string toDate = "",
            string month = "",
            string quarter = "",
            string year = "",
            string departmentId = "")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var manager = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.UserName == managerUsername);

            if (manager?.Employee == null)
            {
                return RedirectToAction("Index", "Login");
            }

            DateTime? filterFromDate = null;
            DateTime? filterToDate = null;
            string selectedPeriod = "Toàn bộ";
            bool isDateRange = !string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate);
            bool isMonthFilter = !string.IsNullOrEmpty(month);

            if (isDateRange && DateTime.TryParse(fromDate, out DateTime startDate) && DateTime.TryParse(toDate, out DateTime endDate))
            {
                filterFromDate = startDate;
                filterToDate = endDate;
                selectedPeriod = $"Từ {startDate:dd/MM/yyyy} Đến {endDate:dd/MM/yyyy}";
            }
            else if (!string.IsNullOrEmpty(day) && DateTime.TryParse(day, out DateTime selectedDay))
            {
                filterFromDate = selectedDay;
                filterToDate = selectedDay;
                selectedPeriod = $"Ngày {selectedDay:dd/MM/yyyy}";
            }
            else if (!string.IsNullOrEmpty(quarter) && int.TryParse(quarter, out int selectedQuarter) && !string.IsNullOrEmpty(year) && int.TryParse(year, out int selectedYear))
            {
                filterFromDate = new DateTime(selectedYear, (selectedQuarter - 1) * 3 + 1, 1);
                filterToDate = filterFromDate.Value.AddMonths(3).AddDays(-1);
                selectedPeriod = $"Quý {selectedQuarter}/{selectedYear}";
            }
            else if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out DateTime selectedMonth))
            {
                int filterYear = !string.IsNullOrEmpty(year) && int.TryParse(year, out int y) ? y : DateTime.Now.Year;
                filterFromDate = new DateTime(filterYear, selectedMonth.Month, 1);
                filterToDate = filterFromDate.Value.AddMonths(1).AddDays(-1);
                selectedPeriod = $"Tháng {selectedMonth:MM/yyyy}";
            }
            else if (!string.IsNullOrEmpty(year) && int.TryParse(year, out int selectedYearForYear))
            {
                filterFromDate = new DateTime(selectedYearForYear, 1, 1);
                filterToDate = new DateTime(selectedYearForYear, 12, 31);
                selectedPeriod = $"Năm {selectedYearForYear}";
            }

            var scoresQuery = _context.Scores
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Employee)
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Job)
                        .ThenInclude(j => j.Category)
                .Where(s => s.CreateDate.HasValue &&
                            s.JobMapEmployee.EmployeeId.HasValue &&
                            s.JobMapEmployee.Employee != null &&
                            s.JobMapEmployee.Employee.DepartmentId.HasValue);

            // Apply department filter if specified
            if (!string.IsNullOrEmpty(departmentId) && int.TryParse(departmentId, out int deptId))
            {
                scoresQuery = scoresQuery.Where(s => s.JobMapEmployee.Employee.DepartmentId == deptId);
                selectedPeriod += $" - Phòng ban: {(_context.Departments.FirstOrDefault(d => d.Id == deptId)?.Name ?? "Unknown")}";
            }
            else
            {
                selectedPeriod += " - Tất cả phòng ban";
            }

            if (filterFromDate.HasValue && filterToDate.HasValue)
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.Value.Date >= filterFromDate.Value.Date && s.CreateDate.Value.Date <= filterToDate.Value.Date);
            }

            var scoreList = await scoresQuery.ToListAsync();

            var employeeScoresQuery = from s in scoresQuery
                                      join jme in _context.Jobmapemployees on s.JobMapEmployeeId equals jme.Id
                                      join e in _context.Employees on jme.EmployeeId equals e.Id
                                      group new { s, e } by new { e.Id, e.FirstName, e.LastName } into g
                                      select new
                                      {
                                          EmployeeId = g.Key.Id,
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

            var employeeScores = await employeeScoresQuery.OrderBy(e => e.EmployeeId).ToListAsync();

            var jobStatusQuery = from s in scoresQuery
                                 join jme in _context.Jobmapemployees on s.JobMapEmployeeId equals jme.Id
                                 join e in _context.Employees on jme.EmployeeId equals e.Id
                                 group s by new { e.Id, EmployeeName = e.FirstName + " " + e.LastName } into g
                                 select new
                                 {
                                     EmployeeId = g.Key.Id,
                                     EmployeeName = g.Key.EmployeeName.Trim(),
                                     TotalJobs = g.Count(),
                                     OnTimeCount = g.Count(s => s.Status == 1),
                                     LateCount = g.Count(s => s.Status == 3),
                                     OverdueCount = g.Count(s => s.Status == 2),
                                     ProcessingCount = g.Count(s => s.Status == 4)
                                 };

            var jobStatusSummary = await jobStatusQuery.OrderBy(e => e.EmployeeName).ToListAsync();

            if (!scoreList.Any() && !employeeScores.Any() && !jobStatusSummary.Any())
            {
                TempData["NoDataMessage"] = "Không có dữ liệu để xuất Excel.";
                return RedirectToAction("Index");
            }

            using (var package = new ExcelPackage())
            {
                var ws1 = package.Workbook.Worksheets.Add("Scores");
                ws1.Cells[1, 1].Value = $"Danh sách điểm số - {selectedPeriod}";
                ws1.Cells[1, 1, 1, 11].Merge = true;
                ws1.Cells[1, 1].Style.Font.Bold = true;
                ws1.Cells[1, 1].Style.Font.Size = 14;
                ws1.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws1.Cells[2, 1].Value = "STT";
                ws1.Cells[2, 2].Value = "Người triển khai";
                ws1.Cells[2, 3].Value = "Hạng mục";
                ws1.Cells[2, 4].Value = "Công việc";
                ws1.Cells[2, 5].Value = "Hạn chót";
                ws1.Cells[2, 6].Value = "Ngày hoàn thành";
                ws1.Cells[2, 7].Value = "Trạng thái";
                ws1.Cells[2, 8].Value = "Đánh giá khối lượng";
                ws1.Cells[2, 9].Value = "Đánh giá tiến độ";
                ws1.Cells[2, 10].Value = "Đánh giá chất lượng";
                ws1.Cells[2, 11].Value = "Đánh giá tổng hợp";

                ws1.Cells[2, 1, 2, 11].Style.Font.Bold = true;
                ws1.Cells[2, 1, 2, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws1.Cells[2, 1, 2, 11].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                for (int i = 0; i < scoreList.Count; i++)
                {
                    var score = scoreList[i];
                    ws1.Cells[i + 3, 1].Value = i + 1;
                    ws1.Cells[i + 3, 2].Value = score.JobMapEmployee.Employee != null
                        ? $"{score.JobMapEmployee.Employee.FirstName} {score.JobMapEmployee.Employee.LastName}"
                        : "N/A";
                    ws1.Cells[i + 3, 3].Value = score.JobMapEmployee.Job.Category?.Name ?? "N/A";
                    ws1.Cells[i + 3, 4].Value = score.JobMapEmployee.Job?.Name ?? "N/A";
                    ws1.Cells[i + 3, 5].Value = score.CreateDate?.ToString("dd/MM/yyyy");
                    ws1.Cells[i + 3, 6].Value = score.CompletionDate?.ToString("dd/MM/yyyy");
                    ws1.Cells[i + 3, 7].Value = score.Status switch
                    {
                        1 => "Hoàn thành",
                        2 => "Chưa hoàn thành",
                        3 => "Hoàn thành muộn",
                        4 => "Đang xử lý",
                        _ => "Chưa bắt đầu"
                    };
                    ws1.Cells[i + 3, 8].Value = score.VolumeAssessment;
                    ws1.Cells[i + 3, 9].Value = score.ProgressAssessment;
                    ws1.Cells[i + 3, 10].Value = score.QualityAssessment;
                    ws1.Cells[i + 3, 11].Value = score.SummaryOfReviews ?? 0;
                    ws1.Cells[i + 3, 11].Style.Numberformat.Format = "0.00";
                }

                ws1.Cells.AutoFitColumns();

                var ws2 = package.Workbook.Worksheets.Add("Danh sách đánh giá");
                ws2.Cells[1, 1].Value = $"Bảng tổng hợp đánh giá - {selectedPeriod}";
                ws2.Cells[1, 1, 1, isMonthFilter ? 7 : 6].Merge = true;
                ws2.Cells[1, 1].Style.Font.Bold = true;
                ws2.Cells[1, 1].Style.Font.Size = 14;
                ws2.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws2.Cells[2, 1].Value = "STT";
                ws2.Cells[2, 2].Value = "Người triển khai";
                ws2.Cells[2, 3].Value = "Tổng đánh giá khối lượng";
                ws2.Cells[2, 4].Value = "Tổng đánh giá tiến độ";
                ws2.Cells[2, 5].Value = "Tổng đánh giá chất lượng";
                ws2.Cells[2, 6].Value = "Tổng đánh giá tổng hợp";
                if (isMonthFilter)
                {
                    ws2.Cells[2, 7].Value = "Đánh giá";
                }

                ws2.Cells[2, 1, 2, isMonthFilter ? 7 : 6].Style.Font.Bold = true;
                ws2.Cells[2, 1, 2, isMonthFilter ? 7 : 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws2.Cells[2, 1, 2, isMonthFilter ? 7 : 6].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                ws2.Cells[2, 1, 2, isMonthFilter ? 7 : 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                int row = 3;
                int index = 0;
                foreach (var item in employeeScores)
                {
                    index++;
                    ws2.Cells[row, 1].Value = index;
                    ws2.Cells[row, 2].Value = item.EmployeeName;
                    ws2.Cells[row, 3].Value = item.TotalVolume;
                    ws2.Cells[row, 4].Value = item.TotalProgress;
                    ws2.Cells[row, 5].Value = item.TotalQuality;
                    ws2.Cells[row, 6].Value = Math.Round(item.SummaryScore, 2);
                    ws2.Cells[row, 6].Style.Numberformat.Format = "0.00";
                    if (isMonthFilter)
                    {
                        ws2.Cells[row, 7].Value = item.EvaluationResult;
                        ws2.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    ws2.Cells[row, 3, row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    row++;
                }

                ws2.Cells.AutoFitColumns();

                if (employeeScores.Any())
                {
                    var chart = ws2.Drawings.AddChart("PieChart", eChartType.Pie3D) as ExcelPieChart;
                    chart.SetPosition(1, 0, 9, 0);
                    chart.SetSize(500, 350);

                    var dataRange = ws2.Cells[3, 3, employeeScores.Count + 2, 3];
                    var labelRange = ws2.Cells[3, 2, employeeScores.Count + 2, 2];

                    var series = chart.Series.Add(dataRange, labelRange) as ExcelPieChartSerie;
                    chart.Title.Text = "Đánh giá khối lượng";
                    chart.Legend.Position = eLegendPosition.Left;

                    if (series != null)
                    {
                        series.DataLabel.ShowPercent = true;
                        series.DataLabel.Position = eLabelPosition.Center;
                    }
                }

                var ws3 = package.Workbook.Worksheets.Add("Analysis Summary");
                var departmentName = !string.IsNullOrEmpty(departmentId) && int.TryParse(departmentId, out int parsedDeptId)
                    ? _context.Departments.FirstOrDefault(d => d.Id == parsedDeptId)?.Name ?? "Unknown"
                    : "All Departments";

                ws3.Cells[1, 1].Value = $"Bảng tổng hợp phân tích - {selectedPeriod}";
                ws3.Cells[1, 1, 1, 7].Merge = true;
                ws3.Cells[1, 1].Style.Font.Size = 14;
                ws3.Cells[1, 1].Style.Font.Bold = true;
                ws3.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws3.Cells[2, 1].Value = "STT";
                ws3.Cells[2, 2].Value = "Người triển khai";
                ws3.Cells[2, 3].Value = "Tổng số";
                ws3.Cells[2, 4].Value = "Đúng hạn";
                ws3.Cells[2, 5].Value = "Trễ hạn";
                ws3.Cells[2, 6].Value = "Quá hạn";
                ws3.Cells[2, 7].Value = "Đang xử lý";

                ws3.Cells[2, 1, 2, 7].Style.Font.Bold = true;
                ws3.Cells[2, 1, 2, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws3.Cells[2, 1, 2, 7].Style.Fill.BackgroundColor.SetColor(Color.Green);
                ws3.Cells[2, 1, 2, 7].Style.Font.Color.SetColor(Color.White);
                ws3.Cells[2, 1, 2, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws3.Cells[3, 1].Value = "Tổng";
                ws3.Cells[3, 2].Value = departmentName;
                ws3.Cells[3, 3].Value = jobStatusSummary.Sum(e => e.TotalJobs);
                ws3.Cells[3, 4].Value = jobStatusSummary.Sum(e => e.OnTimeCount);
                ws3.Cells[3, 5].Value = jobStatusSummary.Sum(e => e.LateCount);
                ws3.Cells[3, 6].Value = jobStatusSummary.Sum(e => e.OverdueCount);
                ws3.Cells[3, 7].Value = jobStatusSummary.Sum(e => e.ProcessingCount);
                ws3.Cells[3, 1, 3, 7].Style.Font.Bold = true;

                for (int i = 0; i < jobStatusSummary.Count; i++)
                {
                    var score = jobStatusSummary[i];
                    ws3.Cells[i + 4, 1].Value = i + 1;
                    ws3.Cells[i + 4, 2].Value = score.EmployeeName;
                    ws3.Cells[i + 4, 3].Value = score.TotalJobs;
                    ws3.Cells[i + 4, 4].Value = score.OnTimeCount;
                    ws3.Cells[i + 4, 5].Value = score.LateCount;
                    ws3.Cells[i + 4, 6].Value = score.OverdueCount;
                    ws3.Cells[i + 4, 7].Value = score.ProcessingCount;
                }

                ws3.Cells[3, 1, jobStatusSummary.Count + 3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws3.Cells.AutoFitColumns();

                if (jobStatusSummary.Any())
                {
                    var columnChart = ws3.Drawings.AddChart("ColumnChart", eChartType.ColumnClustered) as ExcelBarChart;
                    columnChart.SetPosition(1, 0, 9, 0);
                    columnChart.SetSize(800, 400);

                    int nameColumnIndex = 2;
                    int totalColumnIndex = 3;
                    int ontimeColumnIndex = 4;
                    int lateColumnIndex = 5;
                    int overdueColumnIndex = 6;

                    var labelRange = ws3.Cells[4, nameColumnIndex, jobStatusSummary.Count + 3, nameColumnIndex];
                    var totalRange = ws3.Cells[4, totalColumnIndex, jobStatusSummary.Count + 3, totalColumnIndex];
                    var ontimeRange = ws3.Cells[4, ontimeColumnIndex, jobStatusSummary.Count + 3, ontimeColumnIndex];
                    var lateRange = ws3.Cells[4, lateColumnIndex, jobStatusSummary.Count + 3, lateColumnIndex];
                    var overdueRange = ws3.Cells[4, overdueColumnIndex, jobStatusSummary.Count + 3, overdueColumnIndex];

                    var seriesTotal = columnChart.Series.Add(totalRange, labelRange);
                    seriesTotal.Header = "Tổng số";
                    var seriesOntime = columnChart.Series.Add(ontimeRange, labelRange);
                    seriesOntime.Header = "Đúng hạn";
                    var seriesLate = columnChart.Series.Add(lateRange, labelRange);
                    seriesLate.Header = "Trễ hạn";
                    var seriesOverdue = columnChart.Series.Add(overdueRange, labelRange);
                    seriesOverdue.Header = "Quá hạn";

                    seriesTotal.Fill.Color = Color.Blue;
                    seriesOntime.Fill.Color = Color.Green;
                    seriesLate.Fill.Color = Color.Red;
                    seriesOverdue.Fill.Color = Color.Yellow;

                    columnChart.Title.Text = $"Tổng hợp {selectedPeriod}";
                    columnChart.Legend.Position = eLegendPosition.Bottom;
                    columnChart.DataLabel.ShowValue = true;
                    columnChart.DataLabel.Position = eLabelPosition.OutEnd;

                    var pieChart = ws3.Drawings.AddChart("PieChart", eChartType.Pie3D) as ExcelPieChart;
                    pieChart.SetPosition(jobStatusSummary.Count + 5, 0, 1, 0);
                    pieChart.SetSize(500, 350);

                    var pieDataRange = ws3.Cells[4, totalColumnIndex, jobStatusSummary.Count + 3, totalColumnIndex];
                    var pieLabelRange = ws3.Cells[4, nameColumnIndex, jobStatusSummary.Count + 3, nameColumnIndex];

                    var pieSeries = pieChart.Series.Add(pieDataRange, pieLabelRange) as ExcelPieChartSerie;
                    pieChart.Title.Text = "Theo số lượng công việc";
                    pieChart.Legend.Position = eLegendPosition.Left;

                    if (pieSeries != null)
                    {
                        pieSeries.DataLabel.ShowPercent = true;
                        pieSeries.DataLabel.Position = eLabelPosition.Center;
                    }
                }

                var employees = scoreList
                    .Select(s => new { s.JobMapEmployee.EmployeeId, s.JobMapEmployee.Employee })
                    .Where(e => e.EmployeeId.HasValue && e.Employee != null)
                    .DistinctBy(e => e.EmployeeId)
                    .OrderBy(e => e.Employee.FirstName + " " + e.Employee.LastName);

                foreach (var emp in employees)
                {
                    var employeeScoresList = scoreList
                        .Where(s => s.JobMapEmployee.EmployeeId == emp.EmployeeId)
                        .ToList();

                    if (!employeeScoresList.Any())
                        continue;

                    var employeeName = $"{emp.Employee.FirstName} {emp.Employee.LastName}".Trim();
                    var safeSheetName = employeeName.Length > 31 ? employeeName.Substring(0, 31) : employeeName;
                    safeSheetName = safeSheetName.Replace("/", "_").Replace("\\", "_").Replace("?", "_").Replace("*", "_").Replace("[", "_").Replace("]", "_");

                    var wsEmp = package.Workbook.Worksheets.Add(safeSheetName);
                    wsEmp.Cells[1, 1].Value = $"Danh sách điểm số - {employeeName} - {selectedPeriod}";
                    wsEmp.Cells[1, 1, 1, 10].Merge = true;
                    wsEmp.Cells[1, 1].Style.Font.Bold = true;
                    wsEmp.Cells[1, 1].Style.Font.Size = 14;
                    wsEmp.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    wsEmp.Cells[2, 1].Value = "STT";
                    wsEmp.Cells[2, 2].Value = "Hạng mục";
                    wsEmp.Cells[2, 3].Value = "Công việc";
                    wsEmp.Cells[2, 4].Value = "Deadline";
                    wsEmp.Cells[2, 5].Value = "Ngày hoàn thành";
                    wsEmp.Cells[2, 6].Value = "Trạng thái";
                    wsEmp.Cells[2, 7].Value = "Đánh giá khối lượng";
                    wsEmp.Cells[2, 8].Value = "Đánh giá tiến độ";
                    wsEmp.Cells[2, 9].Value = "Đánh giá chất lượng";
                    wsEmp.Cells[2, 10].Value = "Đánh giá tổng hợp";

                    wsEmp.Cells[2, 1, 2, 10].Style.Font.Bold = true;
                    wsEmp.Cells[2, 1, 2, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    wsEmp.Cells[2, 1, 2, 10].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                    for (int i = 0; i < employeeScoresList.Count; i++)
                    {
                        var score = employeeScoresList[i];
                        wsEmp.Cells[i + 3, 1].Value = i + 1;
                        wsEmp.Cells[i + 3, 2].Value = score.JobMapEmployee.Job.Category?.Name ?? "N/A";
                        wsEmp.Cells[i + 3, 3].Value = score.JobMapEmployee.Job?.Name ?? "N/A";
                        wsEmp.Cells[i + 3, 4].Value = score.CreateDate?.ToString("dd/MM/yyyy");
                        wsEmp.Cells[i + 3, 5].Value = score.CompletionDate?.ToString("dd/MM/yyyy");
                        wsEmp.Cells[i + 3, 6].Value = score.Status switch
                        {
                            1 => "Hoàn thành",
                            2 => "Chưa hoàn thành",
                            3 => "Hoàn thành muộn",
                            4 => "Đang xử lý",
                            _ => "Chưa bắt đầu"
                        };
                        wsEmp.Cells[i + 3, 7].Value = score.VolumeAssessment;
                        wsEmp.Cells[i + 3, 8].Value = score.ProgressAssessment;
                        wsEmp.Cells[i + 3, 9].Value = score.QualityAssessment;
                        wsEmp.Cells[i + 3, 10].Value = score.SummaryOfReviews ?? 0;
                        wsEmp.Cells[i + 3, 10].Style.Numberformat.Format = "0.00";
                    }

                    wsEmp.Cells.AutoFitColumns();
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Dashboard_Export_{selectedPeriod.Replace("/", "_").Replace(" ", "_")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}