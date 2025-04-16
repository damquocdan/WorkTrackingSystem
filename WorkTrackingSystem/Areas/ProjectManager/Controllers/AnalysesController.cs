using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using WorkTrackingSystem.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using X.PagedList.Extensions;

namespace WorkTrackingSystem.Areas.ProjectManager.Controllers
{
    [Area("ProjectManager")]
    public class AnalysesController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AnalysesController(WorkTrackingSystemContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: ProjectManager/Analyses
        public async Task<IActionResult> Index(
            string searchText,
            string time,
            string quarteryear,
            string year,
            string date1,
            string date2,
            string sortOrder,
            string filterType,
            int page = 1)
        {
            var limit = 8;
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var manager = await _context.Users
                .Include(u => u.Employee)
                .ThenInclude(e => e.Department)
                .FirstOrDefaultAsync(u => u.UserName == managerUsername);

            if (manager == null || manager.Employee?.DepartmentId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.DepartmentName = manager.Employee.Department?.Name ?? "Không xác định";
            var employeeIdsInManagedDepartment = await _context.Employees
                .Where(e => e.DepartmentId == manager.Employee.DepartmentId)
                .Select(e => e.Id)
                .ToListAsync();

            IQueryable<Analysis> analyses = _context.Analyses
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId.HasValue && employeeIdsInManagedDepartment.Contains(a.EmployeeId.Value)
                            && a.IsDelete == false && a.IsActive == true);

            // Date range filter
            if (!string.IsNullOrEmpty(date1) && !string.IsNullOrEmpty(date2) &&
                DateTime.TryParse(date1, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) &&
                DateTime.TryParse(date2, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                analyses = analyses.Where(a => a.Time.HasValue &&
                                              a.Time.Value.Date >= startDate.Date &&
                                              a.Time.Value.Date <= endDate.Date);
            }
            // Year filter
            else if (!string.IsNullOrEmpty(year) && int.TryParse(year, out int selectedYear))
            {
                analyses = analyses.Where(a => a.Time.HasValue && a.Time.Value.Year == selectedYear);
            }
            // Quarter filter
            else if (!string.IsNullOrEmpty(quarteryear) && quarteryear.Contains("-"))
            {
                var parts = quarteryear.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out int qYear) && int.TryParse(parts[1], out int quarter) && quarter >= 1 && quarter <= 4)
                {
                    var (startMonth, endMonth) = quarter switch
                    {
                        1 => (1, 3),
                        2 => (4, 6),
                        3 => (7, 9),
                        4 => (10, 12),
                        _ => (1, 12)
                    };
                    analyses = analyses.Where(a => a.Time.HasValue &&
                                                  a.Time.Value.Year == qYear &&
                                                  a.Time.Value.Month >= startMonth &&
                                                  a.Time.Value.Month <= endMonth);
                }
            }
            // Month filter
            else if (!string.IsNullOrEmpty(time))
            {
                if (DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedDate))
                {
                    analyses = analyses.Where(a => a.Time.HasValue &&
                                                  a.Time.Value.Month == selectedDate.Month &&
                                                  a.Time.Value.Year == selectedDate.Year);
                }
            }

            // Search by employee code or name
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.Trim();
                analyses = analyses.Where(a => a.Employee != null && (
                    (a.Employee.Code ?? "").Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    (a.Employee.FirstName ?? "").Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    (a.Employee.LastName ?? "").Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    ($"{a.Employee.FirstName} {a.Employee.LastName}" ?? "").Contains(searchText, StringComparison.OrdinalIgnoreCase)));
            }

            // Sorting logic
            IEnumerable<Analysis> finalAnalyses;
            switch (sortOrder)
            {
                case "total_asc": finalAnalyses = analyses.OrderBy(a => a.Total); break;
                case "total_desc": finalAnalyses = analyses.OrderByDescending(a => a.Total); break;
                case "ontime_asc": finalAnalyses = analyses.OrderBy(a => a.Ontime); break;
                case "ontime_desc": finalAnalyses = analyses.OrderByDescending(a => a.Ontime); break;
                case "late_asc": finalAnalyses = analyses.OrderBy(a => a.Late); break;
                case "late_desc": finalAnalyses = analyses.OrderByDescending(a => a.Late); break;
                case "overdue_asc": finalAnalyses = analyses.OrderBy(a => a.Overdue); break;
                case "overdue_desc": finalAnalyses = analyses.OrderByDescending(a => a.Overdue); break;
                case "processing_asc": finalAnalyses = analyses.OrderBy(a => a.Processing); break;
                case "processing_desc": finalAnalyses = analyses.OrderByDescending(a => a.Processing); break;
                case "time_asc": finalAnalyses = analyses.OrderBy(a => a.Time); break;
                case "time_desc": finalAnalyses = analyses.OrderByDescending(a => a.Time); break;
                default: finalAnalyses = analyses.OrderBy(a => a.EmployeeId ?? 0); break;
            }

            var analysesList = finalAnalyses.ToList();

            if (!analysesList.Any())
            {
                TempData["NoDataMessage"] = "Không có dữ liệu để hiển thị hoặc xuất Excel.";
            }

            ViewBag.SearchText = searchText;
            ViewBag.Time = time;
            ViewBag.Quarteryear = quarteryear;
            ViewBag.Year = year;
            ViewBag.Date1 = date1;
            ViewBag.Date2 = date2;
            ViewBag.SortOrder = sortOrder;

            return View(analysesList.ToPagedList(page, limit));
        }

        // GET: ProjectManager/Analyses/Details/5
        public async Task<IActionResult> Details(long? id, string time = null, string quarteryear = null, string year = null)
        {
            if (id == null)
            {
                return NotFound();
            }

            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var manager = await _context.Users
                .Include(u => u.Employee)
                .ThenInclude(e => e.Department)
                .FirstOrDefaultAsync(u => u.UserName == managerUsername);

            if (manager == null || manager.Employee?.DepartmentId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var employeeIdsInManagedDepartment = await _context.Employees
                .Where(e => e.DepartmentId == manager.Employee.DepartmentId)
                .Select(e => e.Id)
                .ToListAsync();

            var analysesQuery = _context.Analyses
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId.HasValue &&
                            employeeIdsInManagedDepartment.Contains(a.EmployeeId.Value) &&
                            a.IsDelete == false && a.IsActive == true);

            Analysis analysis;

            // Year filter
            if (!string.IsNullOrEmpty(year) && int.TryParse(year, out int selectedYear))
            {
                var rawAnalyses = await analysesQuery
                    .Where(a => a.Id == id)
                    .ToListAsync();

                if (!rawAnalyses.Any())
                {
                    return NotFound();
                }

                var employeeId = rawAnalyses.First().EmployeeId;
                var allAnalysesForEmployee = await analysesQuery
                    .Where(a => a.EmployeeId == employeeId && a.Time.HasValue && a.Time.Value.Year == selectedYear)
                    .ToListAsync();

                analysis = new Analysis
                {
                    Id = rawAnalyses.First().Id,
                    EmployeeId = employeeId,
                    Employee = rawAnalyses.First().Employee,
                    Total = allAnalysesForEmployee.Sum(x => x.Total ?? 0),
                    Ontime = allAnalysesForEmployee.Sum(x => x.Ontime ?? 0),
                    Late = allAnalysesForEmployee.Sum(x => x.Late ?? 0),
                    Overdue = allAnalysesForEmployee.Sum(x => x.Overdue ?? 0),
                    Processing = allAnalysesForEmployee.Sum(x => x.Processing ?? 0),
                    Time = null,
                    IsDelete = rawAnalyses.First().IsDelete,
                    IsActive = rawAnalyses.First().IsActive,
                    CreateDate = rawAnalyses.First().CreateDate,
                    UpdateDate = rawAnalyses.First().UpdateDate,
                    CreateBy = rawAnalyses.First().CreateBy,
                    UpdateBy = rawAnalyses.First().UpdateBy
                };
            }
            // Quarter filter
            else if (!string.IsNullOrEmpty(quarteryear) && quarteryear.Contains("-"))
            {
                var parts = quarteryear.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out int qYear) && int.TryParse(parts[1], out int quarter) && quarter >= 1 && quarter <= 4)
                {
                    var (startMonth, endMonth) = quarter switch
                    {
                        1 => (1, 3),
                        2 => (4, 6),
                        3 => (7, 9),
                        4 => (10, 12),
                        _ => (1, 12)
                    };
                    var rawAnalyses = await analysesQuery
                        .Where(a => a.Id == id)
                        .ToListAsync();

                    if (!rawAnalyses.Any())
                    {
                        return NotFound();
                    }

                    var employeeId = rawAnalyses.First().EmployeeId;
                    var allAnalysesForEmployee = await analysesQuery
                        .Where(a => a.EmployeeId == employeeId &&
                                    a.Time.HasValue &&
                                    a.Time.Value.Year == qYear &&
                                    a.Time.Value.Month >= startMonth &&
                                    a.Time.Value.Month <= endMonth)
                        .ToListAsync();

                    analysis = new Analysis
                    {
                        Id = rawAnalyses.First().Id,
                        EmployeeId = employeeId,
                        Employee = rawAnalyses.First().Employee,
                        Total = allAnalysesForEmployee.Sum(x => x.Total ?? 0),
                        Ontime = allAnalysesForEmployee.Sum(x => x.Ontime ?? 0),
                        Late = allAnalysesForEmployee.Sum(x => x.Late ?? 0),
                        Overdue = allAnalysesForEmployee.Sum(x => x.Overdue ?? 0),
                        Processing = allAnalysesForEmployee.Sum(x => x.Processing ?? 0),
                        Time = null,
                        IsDelete = rawAnalyses.First().IsDelete,
                        IsActive = rawAnalyses.First().IsActive,
                        CreateDate = rawAnalyses.First().CreateDate,
                        UpdateDate = rawAnalyses.First().UpdateDate,
                        CreateBy = rawAnalyses.First().CreateBy,
                        UpdateBy = rawAnalyses.First().UpdateBy
                    };
                }
                else
                {
                    return BadRequest("Invalid quarter format. Use 'yyyy-Q'.");
                }
            }
            // Month filter
            else if (!string.IsNullOrEmpty(time))
            {
                DateTime selectedDate;
                if (!DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedDate))
                {
                    return BadRequest("Invalid time format. Use 'yyyy-MM'.");
                }

                analysis = await analysesQuery
                    .FirstOrDefaultAsync(a => a.Id == id &&
                                              a.Time.HasValue &&
                                              a.Time.Value.Month == selectedDate.Month &&
                                              a.Time.Value.Year == selectedDate.Year);

                if (analysis == null)
                {
                    return NotFound();
                }
            }
            // No filter (aggregate all for the employee)
            else
            {
                var rawAnalyses = await analysesQuery
                    .Where(a => a.Id == id)
                    .ToListAsync();

                if (!rawAnalyses.Any())
                {
                    return NotFound();
                }

                var employeeId = rawAnalyses.First().EmployeeId;
                var allAnalysesForEmployee = await analysesQuery
                    .Where(a => a.EmployeeId == employeeId)
                    .ToListAsync();

                analysis = new Analysis
                {
                    Id = rawAnalyses.First().Id,
                    EmployeeId = employeeId,
                    Employee = rawAnalyses.First().Employee,
                    Total = allAnalysesForEmployee.Sum(x => x.Total ?? 0),
                    Ontime = allAnalysesForEmployee.Sum(x => x.Ontime ?? 0),
                    Late = allAnalysesForEmployee.Sum(x => x.Late ?? 0),
                    Overdue = allAnalysesForEmployee.Sum(x => x.Overdue ?? 0),
                    Processing = allAnalysesForEmployee.Sum(x => x.Processing ?? 0),
                    Time = null,
                    IsDelete = rawAnalyses.First().IsDelete,
                    IsActive = rawAnalyses.First().IsActive,
                    CreateDate = rawAnalyses.First().CreateDate,
                    UpdateDate = rawAnalyses.First().UpdateDate,
                    CreateBy = rawAnalyses.First().CreateBy,
                    UpdateBy = rawAnalyses.First().UpdateBy
                };
            }

            ViewBag.Time = time;
            ViewBag.Quarteryear = quarteryear;
            ViewBag.Year = year;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details", analysis);
            }

            return View(analysis);
        }

        public async Task<IActionResult> ExportToExcel(string searchText, string time, string quarteryear, string year, string date1, string date2, string sortOrder, string filterType)
        {
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var manager = await _context.Users
                .Where(u => u.UserName == managerUsername)
                .Select(u => u.Employee)
                .FirstOrDefaultAsync();

            if (manager == null || manager.DepartmentId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var department = await _context.Departments
                .Where(d => d.Id == manager.DepartmentId)
                .Select(d => d.Name)
                .FirstOrDefaultAsync() ?? "Phòng KTDA";

            var employeeIdsInManagedDepartment = await _context.Employees
                .Where(e => e.DepartmentId == manager.DepartmentId)
                .Select(e => e.Id)
                .ToListAsync();

            IQueryable<Analysis> analyses = _context.Analyses
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId.HasValue &&
                            employeeIdsInManagedDepartment.Contains(a.EmployeeId.Value) &&
                            a.IsDelete == false && a.IsActive == true);

            string selectedPeriod = "Toàn bộ";

            if (!string.IsNullOrEmpty(date1) && !string.IsNullOrEmpty(date2) &&
                DateTime.TryParse(date1, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) &&
                DateTime.TryParse(date2, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                selectedPeriod = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";
                analyses = analyses.Where(a => a.Time.HasValue &&
                                              a.Time.Value.Date >= startDate.Date &&
                                              a.Time.Value.Date <= endDate.Date);
            }
            else if (!string.IsNullOrEmpty(year) && int.TryParse(year, out int selectedYear))
            {
                selectedPeriod = $"Năm {selectedYear}";
                analyses = analyses.Where(a => a.Time.HasValue && a.Time.Value.Year == selectedYear);
            }
            else if (!string.IsNullOrEmpty(quarteryear) && quarteryear.Contains("-"))
            {
                var parts = quarteryear.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out int qYear) && int.TryParse(parts[1], out int quarter) && quarter >= 1 && quarter <= 4)
                {
                    selectedPeriod = $"Quý {quarter}/{qYear}";
                    var (startMonth, endMonth) = quarter switch
                    {
                        1 => (1, 3),
                        2 => (4, 6),
                        3 => (7, 9),
                        4 => (10, 12),
                        _ => (1, 12)
                    };
                    analyses = analyses.Where(a => a.Time.HasValue &&
                                                  a.Time.Value.Year == qYear &&
                                                  a.Time.Value.Month >= startMonth &&
                                                  a.Time.Value.Month <= endMonth);
                }
            }
            else if (!string.IsNullOrEmpty(time))
            {
                if (DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedDate))
                {
                    selectedPeriod = selectedDate.ToString("MM/yyyy");
                    analyses = analyses.Where(a => a.Time.HasValue &&
                                                  a.Time.Value.Month == selectedDate.Month &&
                                                  a.Time.Value.Year == selectedDate.Year);
                }
            }

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
            switch (sortOrder)
            {
                case "total_asc": finalAnalyses = analyses.OrderBy(a => a.Total); break;
                case "total_desc": finalAnalyses = analyses.OrderByDescending(a => a.Total); break;
                case "ontime_asc": finalAnalyses = analyses.OrderBy(a => a.Ontime); break;
                case "ontime_desc": finalAnalyses = analyses.OrderByDescending(a => a.Ontime); break;
                case "late_asc": finalAnalyses = analyses.OrderBy(a => a.Late); break;
                case "late_desc": finalAnalyses = analyses.OrderByDescending(a => a.Late); break;
                case "overdue_asc": finalAnalyses = analyses.OrderBy(a => a.Overdue); break;
                case "overdue_desc": finalAnalyses = analyses.OrderByDescending(a => a.Overdue); break;
                case "processing_asc": finalAnalyses = analyses.OrderBy(a => a.Processing); break;
                case "processing_desc": finalAnalyses = analyses.OrderByDescending(a => a.Processing); break;
                case "time_asc": finalAnalyses = analyses.OrderBy(a => a.Time); break;
                case "time_desc": finalAnalyses = analyses.OrderByDescending(a => a.Time); break;
                default: finalAnalyses = analyses.OrderBy(a => a.EmployeeId ?? 0); break;
            }

            var analysesList = finalAnalyses.ToList();

            if (!analysesList.Any())
            {
                return BadRequest($"Không có dữ liệu để xuất Excel cho khoảng thời gian {selectedPeriod}.");
            }

            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Analysis Summary");

                    worksheet.Cells[1, 1].Value = $"Bảng tổng hợp phân tích {selectedPeriod}";
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
                    worksheet.Cells[3, 3].Value = analysesList.Sum(a => a.Total ?? 0);
                    worksheet.Cells[3, 4].Value = analysesList.Sum(a => a.Ontime ?? 0);
                    worksheet.Cells[3, 5].Value = analysesList.Sum(a => a.Late ?? 0);
                    worksheet.Cells[3, 6].Value = analysesList.Sum(a => a.Overdue ?? 0);
                    worksheet.Cells[3, 7].Value = analysesList.Sum(a => a.Processing ?? 0);
                    worksheet.Cells[3, 1, 3, 7].Style.Font.Bold = true;

                    for (int i = 0; i < analysesList.Count; i++)
                    {
                        worksheet.Cells[i + 4, 1].Value = i + 2;
                        worksheet.Cells[i + 4, 2].Value = $"{analysesList[i].Employee?.FirstName} {analysesList[i].Employee?.LastName}".Trim();
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

                        columnChart.Title.Text = "Tổng hợp khoảng thời gian";
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
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Analysis_{selectedPeriod.Replace("/", "_")}.xlsx");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tạo Excel: {ex.Message}");
                TempData["NoDataMessage"] = "Có lỗi xảy ra khi xuất Excel.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> SendEmail()
        {
            try
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, "Analysis.xlsx");

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Analysis");
                    worksheet.Cells[1, 1].Value = "ID";
                    worksheet.Cells[1, 2].Value = "Code";
                    worksheet.Cells[1, 3].Value = "Họ và Tên";
                    worksheet.Cells[1, 4].Value = "Email";
                    worksheet.Cells[1, 5].Value = "Số điện thoại";
                    worksheet.Cells[1, 6].Value = "Tổng số công việc";
                    worksheet.Cells[1, 7].Value = "Công việc hoàn thành";
                    worksheet.Cells[1, 8].Value = "Hoàn thành muộn";
                    worksheet.Cells[1, 9].Value = "Chưa hoàn thành";
                    worksheet.Cells[1, 10].Value = "Đang xử lý";
                    worksheet.Cells[1, 11].Value = "Tháng";
                    worksheet.Cells[1, 1, 1, 11].Style.Font.Bold = true;

                    var analyses = await _context.Analyses
                        .Include(a => a.Employee)
                        .Where(a => a.IsDelete == false && a.IsActive == true)
                        .ToListAsync();

                    int row = 2;
                    foreach (var item in analyses)
                    {
                        worksheet.Cells[row, 1].Value = item.Id;
                        worksheet.Cells[row, 2].Value = item.Employee?.Code ?? "";
                        worksheet.Cells[row, 3].Value = $"{item.Employee?.FirstName} {item.Employee?.LastName}".Trim();
                        worksheet.Cells[row, 4].Value = item.Employee?.Email ?? "";
                        worksheet.Cells[row, 5].Value = item.Employee?.Phone ?? "";
                        worksheet.Cells[row, 6].Value = item.Total;
                        worksheet.Cells[row, 7].Value = item.Ontime;
                        worksheet.Cells[row, 8].Value = item.Late;
                        worksheet.Cells[row, 9].Value = item.Overdue;
                        worksheet.Cells[row, 10].Value = item.Processing;
                        worksheet.Cells[row, 11].Value = item.Time.HasValue ? item.Time.Value.ToString("MM-yyyy") : "";
                        row++;
                    }

                    package.SaveAs(new FileInfo(filePath));
                }

                // Placeholder: Replace with actual email configuration
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
    }
}