using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml;
using WorkTrackingSystem.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using X.PagedList.Extensions;
using Microsoft.Data.SqlClient;
using WorkTrackingSystem.Areas.ProjectManager.Models;

namespace WorkTrackingSystem.Areas.AdminSystem.Controllers
{
    [Area("AdminSystem")]
    public class AnalysesController : Controller
    {
        private readonly WorkTrackingSystemContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AnalysesController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: AdminSystem/Analyses
        public async Task<IActionResult> Index(
             int? DepartmentId,
   string searchText,
   string time,
   string sortOrder,
   string filterType,
   int page = 1)
        {
            var limit = 8;
            //var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

            //if (string.IsNullOrEmpty(managerUsername))
            //{
            //    return RedirectToAction("Index", "Login");
            //}

            //var manager = await _context.Users
            //    .Include(u => u.Employee)
            //    .ThenInclude(e => e.Department)
            //    .FirstOrDefaultAsync(u => u.UserName == managerUsername);

            //if (manager == null || manager.Employee?.DepartmentId == null)
            //{
            //    return RedirectToAction("Index", "Login");
            //}

            //ViewBag.DepartmentName = manager.Employee.Department?.Name ?? "Không xác định";
            var employeeIdsInManagedDepartment = await _context.Employees
                //.Where(e => e.DepartmentId == manager.Employee.DepartmentId)
                .Select(e => e.Id)
                .ToListAsync();

            var analyses = _context.Analyses
                .Include(a => a.Employee)
                .ThenInclude(e=>e.Department)
                .Where(a => a.EmployeeId.HasValue && employeeIdsInManagedDepartment.Contains(a.EmployeeId.Value));

            // Tìm kiếm theo mã/tên nhân viên
            if (!string.IsNullOrEmpty(searchText))
            {
                analyses = analyses.Where(a =>
                    a.Employee.Code.Contains(searchText) ||
                    a.Employee.FirstName.Contains(searchText) ||
                    a.Employee.LastName.Contains(searchText));
            }
            //lọc theo phòng ban
            if (DepartmentId.HasValue && DepartmentId > 0)
            {
                analyses = analyses.Where(s => s.Employee.DepartmentId == DepartmentId);
            }
            IEnumerable<Analysis> finalAnalyses; // Sử dụng IEnumerable thay vì IQueryable

            // Nếu không có time, tính tổng toàn bộ các tháng
            if (string.IsNullOrEmpty(time))
            {
                // Lấy dữ liệu thô từ cơ sở dữ liệu trước
                var rawAnalyses = await analyses.ToListAsync();

                // Nhóm và tính tổng trong bộ nhớ
                finalAnalyses = rawAnalyses
                    .GroupBy(a => a.EmployeeId)
                    .Select(g => new Analysis
                    {
                        EmployeeId = g.Key,
                        Employee = g.First().Employee, // Lấy thông tin Employee từ bản ghi đầu tiên
                        Total = g.Sum(x => x.Total ?? 0),
                        Ontime = g.Sum(x => x.Ontime ?? 0),
                        Late = g.Sum(x => x.Late ?? 0),
                        Overdue = g.Sum(x => x.Overdue ?? 0),
                        Processing = g.Sum(x => x.Processing ?? 0),
                        Time = null // Không hiển thị thời gian cụ thể vì đây là tổng
                    });
            }
            else // Nếu có time, lọc theo tháng/năm như cũ
            {
                DateTime selectedDate;
                if (DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedDate))
                {
                    analyses = analyses.Where(a => a.Time.HasValue &&
                                                   a.Time.Value.Month == selectedDate.Month &&
                                                   a.Time.Value.Year == selectedDate.Year);
                }
                finalAnalyses = await analyses.ToListAsync(); // Chuyển trực tiếp sang List
            }

            // Sắp xếp kết quả theo lựa chọn của người dùng
            switch (sortOrder)
            {
                case "total_asc":
                    finalAnalyses = finalAnalyses.OrderBy(a => a.Total);
                    break;
                case "total_desc":
                    finalAnalyses = finalAnalyses.OrderByDescending(a => a.Total);
                    break;
                case "ontime_asc":
                    finalAnalyses = finalAnalyses.OrderBy(a => a.Ontime);
                    break;
                case "ontime_desc":
                    finalAnalyses = finalAnalyses.OrderByDescending(a => a.Ontime);
                    break;
                case "late_asc":
                    finalAnalyses = finalAnalyses.OrderBy(a => a.Late);
                    break;
                case "late_desc":
                    finalAnalyses = finalAnalyses.OrderByDescending(a => a.Late);
                    break;
                case "overdue_asc":
                    finalAnalyses = finalAnalyses.OrderBy(a => a.Overdue);
                    break;
                case "overdue_desc":
                    finalAnalyses = finalAnalyses.OrderByDescending(a => a.Overdue);
                    break;
                case "processing_asc":
                    finalAnalyses = finalAnalyses.OrderBy(a => a.Processing);
                    break;
                case "processing_desc":
                    finalAnalyses = finalAnalyses.OrderByDescending(a => a.Processing);
                    break;
                case "time_asc":
                    finalAnalyses = finalAnalyses.OrderBy(a => a.Time);
                    break;
                case "time_desc":
                    finalAnalyses = finalAnalyses.OrderByDescending(a => a.Time);
                    break;
                default:
                    finalAnalyses = finalAnalyses.OrderBy(a => a.Id);
                    break;
            }

            var analysesList = finalAnalyses.ToList(); // Chuyển sang List để xử lý phân trang
            if (!analysesList.Any())
            {
                TempData["NoDataMessage"] = "Không có dữ liệu để hiển thị hoặc xuất Excel.";
            }
            ViewBag.Department = new SelectList(_context.Departments, "Id", "Name");
            ViewBag.DepartmentId = DepartmentId;
            ViewBag.SearchText = searchText;
            ViewBag.Time = time;
            ViewBag.SortOrder = sortOrder;
            return View(finalAnalyses.ToPagedList(page, limit));
        }
        public async Task<IActionResult> AnalysesEmployees(
    string search,
    int? positionId,
    string departmentId,
    string timeType,
    DateTime? fromDate,
    DateTime? toDate,
    string time,
    int? quarter,
    int? quarterYear,
    int? year,
    string sortOrder,
    int page = 1)
        {
            var limit = 10;

            // Get list of departments managed by the manager
            var managedDepartments = await _context.Departments
                .Select(d => d.Id)
                .ToListAsync();

            // Handle time parameters
            int? month = null;
            int? monthYear = null;

            if (timeType == "month" && !string.IsNullOrEmpty(time) && DateTime.TryParse(time + "-01", out var parsedTime))
            {
                month = parsedTime.Month;
                monthYear = parsedTime.Year;
                fromDate = new DateTime(parsedTime.Year, parsedTime.Month, 1);
                toDate = fromDate.Value.AddMonths(1).AddDays(-1);
            }
            else if (timeType == "quarter")
            {
                if (quarter.HasValue && quarterYear.HasValue)
                {
                    // Both quarter and year
                    fromDate = new DateTime(quarterYear.Value, (quarter.Value - 1) * 3 + 1, 1);
                    toDate = fromDate.Value.AddMonths(3).AddDays(-1);
                }
                else if (quarter.HasValue)
                {
                    // Only quarter: rely on stored procedure to filter by month
                    fromDate = null;
                    toDate = null;
                }
                else if (quarterYear.HasValue)
                {
                    // Only year
                    fromDate = new DateTime(quarterYear.Value, 1, 1);
                    toDate = new DateTime(quarterYear.Value, 12, 31);
                }
                else
                {
                    // No quarter or year: treat as total
                    fromDate = null;
                    toDate = null;
                    quarter = null;
                    quarterYear = null;
                }
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

            // Handle departmentId: Convert from string to long
            long? departmentToQuery = null;
            if (!string.IsNullOrEmpty(departmentId) && long.TryParse(departmentId, out long parsedDepartmentId))
            {
                if (managedDepartments.Contains(parsedDepartmentId))
                {
                    departmentToQuery = parsedDepartmentId;
                }
            }

            // If no valid departmentId, use all managed departments
            var departmentsToQuery = departmentToQuery.HasValue
                ? new List<long> { departmentToQuery.Value }
                : managedDepartments;

            // Call stored procedure for each department
            var employeeScores = new List<EmployeeScoreSummary>();
            foreach (var deptId in departmentsToQuery)
            {
                var parameters = new List<SqlParameter>
        {
            new SqlParameter("@DepartmentId", deptId),
            new SqlParameter("@FromDate", (object)fromDate ?? DBNull.Value),
            new SqlParameter("@ToDate", (object)toDate ?? DBNull.Value),
            new SqlParameter("@Quarter", (object)quarter ?? DBNull.Value),
            new SqlParameter("@QuarterYear", (object)quarterYear ?? DBNull.Value),
            new SqlParameter("@Month", (object)month ?? DBNull.Value),
            new SqlParameter("@MonthYear", (object)monthYear ?? DBNull.Value),
            new SqlParameter("@Year", (object)year ?? DBNull.Value)
        };
                    var deptScores = await _context.Set<EmployeeScoreSummary>()
                        .FromSqlRaw("EXEC sp_GetEmployeeScoreSummary @DepartmentId, @FromDate, @ToDate, @Quarter, @QuarterYear, @Month, @MonthYear, @Year", parameters.ToArray())
                        .ToListAsync();
                    employeeScores.AddRange(deptScores);
            }

            // Filter by search term
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                employeeScores = employeeScores
                    .Where(e => e.EmployeeName.ToLower().Contains(search))
                    .ToList();
            }

            // Apply sorting
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

            // Pagination
            var pagedEmployeeScores = employeeScores.ToPagedList(page, limit);

            // Calculate totals for all employees in selected departments
            var totalSum = employeeScores.Sum(e => e.TotalJobs);
            var ontimeSum = employeeScores.Sum(e => e.OnTimeCount);
            var lateSum = employeeScores.Sum(e => e.LateCount);
            var overdueSum = employeeScores.Sum(e => e.OverdueCount);
            var processingSum = employeeScores.Sum(e => e.ProcessingCount);

            // Prepare data for view
            ViewBag.Search = search;
            ViewBag.DepartmentId = departmentId;
            ViewBag.PositionId = positionId;
            ViewBag.TimeType = timeType ?? "total";
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Time = time;
            ViewBag.Quarter = quarter?.ToString();
            ViewBag.QuarterYear = quarterYear?.ToString();
            ViewBag.Year = year?.ToString();
            ViewBag.SortOrder = sortOrder;
            ViewBag.TotalSum = totalSum;
            ViewBag.OntimeSum = ontimeSum;
            ViewBag.LateSum = lateSum;
            ViewBag.OverdueSum = overdueSum;
            ViewBag.ProcessingSum = processingSum;
            ViewBag.DepartmentName = departmentToQuery.HasValue
                ? _context.Departments.Where(d => d.Id == departmentToQuery.Value).Select(d => d.Name).FirstOrDefault() ?? "All Departments"
                : "Tất cả phòng ban";
            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name");
            ViewBag.Positions = new SelectList(_context.Positions, "Id", "Name");

            return View(pagedEmployeeScores);
        }
        public async Task<IActionResult> ExportToExcel(string searchText, string time, string sortOrder, string filterType)
        {
            //var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            //if (string.IsNullOrEmpty(managerUsername))
            //{
            //    return RedirectToAction("Index", "Login");
            //}

            //var manager = await _context.Users
            //    .Where(u => u.UserName == managerUsername)
            //    .Select(u => u.Employee)
            //    .FirstOrDefaultAsync();

            //if (manager == null || manager.DepartmentId == null)
            //{
            //    return RedirectToAction("Index", "Login");
            //}

            var department = await _context.Departments
                //.Where(d => d.Id == manager.DepartmentId)
                .Select(d => d.Name)
                .FirstOrDefaultAsync() ?? "Phòng KTDA";

            var employeeIdsInManagedDepartment = await _context.Employees
                //.Where(e => e.DepartmentId == manager.DepartmentId)
                .Select(e => e.Id)
                .ToListAsync();

            var analyses = _context.Analyses
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId.HasValue && employeeIdsInManagedDepartment.Contains(a.EmployeeId.Value));

            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.Trim();
                analyses = analyses.Where(a => a.Employee != null && (
                    (a.Employee.Code ?? "").Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    (a.Employee.FirstName ?? "").Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    (a.Employee.LastName ?? "").Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    ($"{a.Employee.FirstName} {a.Employee.LastName}" ?? "").Contains(searchText, StringComparison.OrdinalIgnoreCase)));
            }

            IEnumerable<Analysis> finalAnalyses;
            string selectedMonth = "Toàn bộ";

            // Nếu không có time, tính tổng toàn bộ các tháng
            if (string.IsNullOrEmpty(time))
            {
                var rawAnalyses = await analyses.ToListAsync();
                finalAnalyses = rawAnalyses
                    .GroupBy(a => a.EmployeeId)
                    .Select(g => new Analysis
                    {
                        EmployeeId = g.Key,
                        Employee = g.First().Employee,
                        Total = g.Sum(x => x.Total ?? 0),
                        Ontime = g.Sum(x => x.Ontime ?? 0),
                        Late = g.Sum(x => x.Late ?? 0),
                        Overdue = g.Sum(x => x.Overdue ?? 0),
                        Processing = g.Sum(x => x.Processing ?? 0),
                        Time = null
                    });
            }
            else // Nếu có time, lọc theo tháng/năm
            {
                DateTime selectedDate;
                if (DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedDate))
                {
                    selectedMonth = selectedDate.ToString("MM/yyyy");
                    analyses = analyses.Where(a => a.Time.HasValue &&
                                                   a.Time.Value.Month == selectedDate.Month &&
                                                   a.Time.Value.Year == selectedDate.Year);
                }
                finalAnalyses = await analyses.ToListAsync();
            }

            // Sắp xếp kết quả theo lựa chọn của người dùng
            switch (sortOrder)
            {
                case "total_asc": finalAnalyses = finalAnalyses.OrderBy(a => a.Total); break;
                case "total_desc": finalAnalyses = finalAnalyses.OrderByDescending(a => a.Total); break;
                case "ontime_asc": finalAnalyses = finalAnalyses.OrderBy(a => a.Ontime); break;
                case "ontime_desc": finalAnalyses = finalAnalyses.OrderByDescending(a => a.Ontime); break;
                case "late_asc": finalAnalyses = finalAnalyses.OrderBy(a => a.Late); break;
                case "late_desc": finalAnalyses = finalAnalyses.OrderByDescending(a => a.Late); break;
                case "overdue_asc": finalAnalyses = finalAnalyses.OrderBy(a => a.Overdue); break;
                case "overdue_desc": finalAnalyses = finalAnalyses.OrderByDescending(a => a.Overdue); break;
                case "processing_asc": finalAnalyses = finalAnalyses.OrderBy(a => a.Processing); break;
                case "processing_desc": finalAnalyses = finalAnalyses.OrderByDescending(a => a.Processing); break;
                case "time_asc": finalAnalyses = finalAnalyses.OrderBy(a => a.Time); break;
                case "time_desc": finalAnalyses = finalAnalyses.OrderByDescending(a => a.Time); break;
                default: finalAnalyses = finalAnalyses.OrderBy(a => a.Id); break;
            }

            var analysesList = finalAnalyses.ToList();
            if (!analysesList.Any())
            {
                return BadRequest($"Không có dữ liệu để xuất Excel cho tháng {selectedMonth}.");
            }

            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Analysis Summary");

                    worksheet.Cells[1, 1].Value = $"Bảng tổng hợp phân tích tháng {selectedMonth}";
                    worksheet.Cells[1, 1, 1, 7].Merge = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 14;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    worksheet.Cells[2, 1].Value = "STT";
                    worksheet.Cells[2, 2].Value = "Nhân sự";
                    worksheet.Cells[2, 3].Value = "Tổng";
                    worksheet.Cells[2, 4].Value = "Đúng hạn";
                    worksheet.Cells[2, 5].Value = "Trễ hạn";
                    worksheet.Cells[2, 6].Value = "Quá hạn";
                    worksheet.Cells[2, 7].Value = "Đang xử lý";
                    worksheet.Cells[2, 1, 2, 7].Style.Font.Bold = true;
                    worksheet.Cells[2, 1, 2, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[2, 1, 2, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);
                    worksheet.Cells[2, 1, 2, 7].Style.Font.Color.SetColor(System.Drawing.Color.White);
                    worksheet.Cells[2, 1, 2, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    worksheet.Cells[3, 1].Value = 1;
                    worksheet.Cells[3, 2].Value = department;
                    worksheet.Cells[3, 3].Value = analysesList.Sum(a => a.Total);
                    worksheet.Cells[3, 4].Value = analysesList.Sum(a => a.Ontime);
                    worksheet.Cells[3, 5].Value = analysesList.Sum(a => a.Late);
                    worksheet.Cells[3, 6].Value = analysesList.Sum(a => a.Overdue);
                    worksheet.Cells[3, 7].Value = analysesList.Sum(a => a.Processing);
                    worksheet.Cells[3, 1, 3, 7].Style.Font.Bold = true;

                    for (int i = 0; i < analysesList.Count; i++)
                    {
                        worksheet.Cells[i + 4, 1].Value = i + 2;
                        worksheet.Cells[i + 4, 2].Value = $"{analysesList[i].Employee.FirstName} {analysesList[i].Employee.LastName}";
                        worksheet.Cells[i + 4, 3].Value = analysesList[i].Total;
                        worksheet.Cells[i + 4, 4].Value = analysesList[i].Ontime;
                        worksheet.Cells[i + 4, 5].Value = analysesList[i].Late;
                        worksheet.Cells[i + 4, 6].Value = analysesList[i].Overdue;
                        worksheet.Cells[i + 4, 7].Value = analysesList[i].Processing;
                    }
                    worksheet.Cells[3, 1, analysesList.Count + 3, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    if (analysesList.Any())
                    {
                        var columnChart = worksheet.Drawings.AddChart("ColumnChart", eChartType.ColumnClustered) as ExcelBarChart;
                        columnChart.SetPosition(analysesList.Count + 15, 0, 1, 0);
                        columnChart.SetSize(800, 400);

                        int nameColumnIndex = 2;
                        int totalColumnIndex = 3;
                        int ontimeColumnIndex = 4;
                        int lateColumnIndex = 5;
                        int overdueColumnIndex = 6;

                        var labelRange = worksheet.Cells[4, nameColumnIndex, analysesList.Count + 3, nameColumnIndex];
                        var totalRange = worksheet.Cells[4, totalColumnIndex, analysesList.Count + 3, totalColumnIndex];
                        var ontimeRange = worksheet.Cells[4, ontimeColumnIndex, analysesList.Count + 3, ontimeColumnIndex];
                        var lateRange = worksheet.Cells[4, lateColumnIndex, analysesList.Count + 3, lateColumnIndex];
                        var overdueRange = worksheet.Cells[4, overdueColumnIndex, analysesList.Count + 3, overdueColumnIndex];

                        var seriesTotal = columnChart.Series.Add(totalRange, labelRange);
                        seriesTotal.Header = "Tổng";
                        var seriesOntime = columnChart.Series.Add(ontimeRange, labelRange);
                        seriesOntime.Header = "Đúng hạn";
                        var seriesLate = columnChart.Series.Add(lateRange, labelRange);
                        seriesLate.Header = "Trễ hạn";
                        var seriesOverdue = columnChart.Series.Add(overdueRange, labelRange);
                        seriesOverdue.Header = "Quá hạn";

                        seriesTotal.Fill.Color = System.Drawing.Color.Blue;
                        seriesOntime.Fill.Color = System.Drawing.Color.Green;
                        seriesLate.Fill.Color = System.Drawing.Color.Red;
                        seriesOverdue.Fill.Color = System.Drawing.Color.Yellow;

                        columnChart.Title.Text = "Tổng hợp tháng/năm";
                        columnChart.Legend.Position = eLegendPosition.Bottom;
                        columnChart.DataLabel.ShowValue = true;
                        columnChart.DataLabel.Position = eLabelPosition.OutEnd;

                        var pieChart = worksheet.Drawings.AddChart("PieChart", eChartType.Pie3D) as ExcelPieChart;
                        pieChart.SetPosition(1, 0, 9, 0);
                        pieChart.SetSize(500, 350);

                        var pieDataRange = worksheet.Cells[4, totalColumnIndex, analysesList.Count + 3, totalColumnIndex];
                        var pieLabelRange = worksheet.Cells[4, nameColumnIndex, analysesList.Count + 3, nameColumnIndex];

                        var pieSeries = pieChart.Series.Add(pieDataRange, pieLabelRange) as ExcelPieChartSerie;
                        pieChart.Title.Text = "Theo số lượng công việc";
                        pieChart.Legend.Position = eLegendPosition.Left;

                        if (pieSeries != null)
                        {
                            pieSeries.DataLabel.ShowPercent = true;
                            pieSeries.DataLabel.Position = eLabelPosition.Center;
                        }
                    }

                    var stream = new MemoryStream(package.GetAsByteArray());
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Analysis_{selectedMonth.Replace("/", "_")}.xlsx");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tạo Excel: {ex.Message}");
                TempData["NoDataMessage"] = "Có lỗi xảy ra khi xuất Excel.";
                return RedirectToAction("Index");
            }
        }

        public ActionResult SendEmail()
        {
            try
            {
                // Tạo thư mục lưu file nếu chưa tồn tại
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, "Analysis.xlsx");

                // Tạo file Excel
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Analysis");
                    worksheet.Cells[1, 1].Value = "ID";
                    worksheet.Cells[1, 2].Value = "Code";
                    worksheet.Cells[1, 2].Value = "Họ và";
                    worksheet.Cells[1, 2].Value = "Tên";
                    worksheet.Cells[1, 2].Value = "Email";
                    worksheet.Cells[1, 2].Value = "Số điện thoại";
                    worksheet.Cells[1, 3].Value = "Tổng số công việc";
                    worksheet.Cells[1, 4].Value = "Công việc hoàn thành";
                    worksheet.Cells[1, 5].Value = "Hoàn thành muộn";
                    worksheet.Cells[1, 6].Value = "Chưa hoàn thành";
                    worksheet.Cells[1, 7].Value = "Đang xử lý";
                    worksheet.Cells[1, 8].Value = "Tháng";

                    int row = 2;
                    foreach (var item in _context.Analyses.ToList())
                    {
                        worksheet.Cells[row, 1].Value = item.Id;
                        worksheet.Cells[row, 2].Value = item.Employee.Code;
                        worksheet.Cells[row, 2].Value = item.Employee.FirstName;
                        worksheet.Cells[row, 2].Value = item.Employee.LastName;
                        worksheet.Cells[row, 2].Value = item.Employee.Email;
                        worksheet.Cells[row, 2].Value = item.Employee.Phone;
                        worksheet.Cells[row, 3].Value = item.Total;
                        worksheet.Cells[row, 4].Value = item.Ontime;
                        worksheet.Cells[row, 5].Value = item.Late;
                        worksheet.Cells[row, 6].Value = item.Overdue;
                        worksheet.Cells[row, 7].Value = item.Processing;
                        worksheet.Cells[row, 8].Value = item.Time?.ToString("MM-yyyy");
                        row++;
                    }

                    package.SaveAs(new FileInfo(filePath));
                }

                // Cấu hình email
                var fromAddress = new MailAddress("your-email@example.com", "Your Name");
                var toAddress = new MailAddress("recipient@example.com", "Recipient Name");
                const string fromPassword = "your-email-password";
                const string subject = "Analysis Report";
                const string body = "Please find the attached Analysis report.";

                var smtp = new SmtpClient
                {
                    Host = "smtp.example.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    message.Attachments.Add(new Attachment(filePath));
                    smtp.Send(message);
                }

                return Content("Email sent successfully!");
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }
        // GET: ProjectManager/Analyses/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analysis = await _context.Analyses
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (analysis == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details", analysis);
            }
            return View(analysis);
        }

        // GET: ProjectManager/Analyses/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id");
            return View();
        }

        // POST: ProjectManager/Analyses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,Total,Ontime,Late,Overdue,Processing,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Analysis analysis)
        {
            if (ModelState.IsValid)
            {
                _context.Add(analysis);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", analysis.EmployeeId);
            return View(analysis);
        }

        // GET: ProjectManager/Analyses/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analysis = await _context.Analyses.FindAsync(id);
            if (analysis == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", analysis.EmployeeId);
            return View(analysis);
        }

        // POST: ProjectManager/Analyses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,EmployeeId,Total,Ontime,Late,Overdue,Processing,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Analysis analysis)
        {
            if (id != analysis.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(analysis);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnalysisExists(analysis.Id))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", analysis.EmployeeId);
            return View(analysis);
        }

        // GET: ProjectManager/Analyses/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analysis = await _context.Analyses
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (analysis == null)
            {
                return NotFound();
            }

            return View(analysis);
        }

        // POST: ProjectManager/Analyses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var analysis = await _context.Analyses.FindAsync(id);
            if (analysis != null)
            {
                analysis.IsActive = false;
                analysis.IsDelete= true;
                _context.Analyses.Update(analysis);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnalysisExists(long id)
        {
            return _context.Analyses.Any(e => e.Id == id);
        }
    }
}
