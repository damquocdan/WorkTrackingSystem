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
    string time, // Thay vì month và year riêng lẻ
    string sortOrder,
    string filterType,int page=1)
        {
            var limit = 8;
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

            var employeeIdsInManagedDepartment = await _context.Employees
                .Where(e => e.DepartmentId == manager.DepartmentId)
                .Select(e => e.Id)
                .ToListAsync();

            var analyses = _context.Analyses
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId.HasValue && employeeIdsInManagedDepartment.Contains(a.EmployeeId.Value));

            // Tìm kiếm theo mã/tên nhân viên
            if (!string.IsNullOrEmpty(searchText))
            {
                analyses = analyses.Where(a =>
                    a.Employee.Code.Contains(searchText) ||
                    a.Employee.FirstName.Contains(searchText) ||
                    a.Employee.LastName.Contains(searchText));
            }

            // Bộ lọc theo tháng/năm
            if (!string.IsNullOrEmpty(time))
            {
                DateTime selectedDate;
                if (DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedDate))
                {
                    analyses = analyses.Where(a => a.Time.HasValue &&
                                                   a.Time.Value.Month == selectedDate.Month &&
                                                   a.Time.Value.Year == selectedDate.Year);
                }
            }

            // Sắp xếp kết quả theo lựa chọn của người dùng
            switch (sortOrder)
            {
                case "total_asc":
                    analyses = analyses.OrderBy(a => a.Total);
                    break;
                case "total_desc":
                    analyses = analyses.OrderByDescending(a => a.Total);
                    break;
                case "ontime_asc":
                    analyses = analyses.OrderBy(a => a.Ontime);
                    break;
                case "ontime_desc":
                    analyses = analyses.OrderByDescending(a => a.Ontime);
                    break;
                case "late_asc":
                    analyses = analyses.OrderBy(a => a.Late);
                    break;
                case "late_desc":
                    analyses = analyses.OrderByDescending(a => a.Late);
                    break;
                case "overdue_asc":
                    analyses = analyses.OrderBy(a => a.Overdue);
                    break;
                case "overdue_desc":
                    analyses = analyses.OrderByDescending(a => a.Overdue);
                    break;
                case "processing_asc":
                    analyses = analyses.OrderBy(a => a.Processing);
                    break;
                case "processing_desc":
                    analyses = analyses.OrderByDescending(a => a.Processing);
                    break;
                case "time_asc":
                    analyses = analyses.OrderBy(a => a.Time);
                    break;
                case "time_desc":
                    analyses = analyses.OrderByDescending(a => a.Time);
                    break;
                default:
                    analyses = analyses.OrderBy(a => a.Id);
                    break;
            }
            var analysesList = await analyses.ToListAsync();
            if (!analysesList.Any())
            {
                TempData["NoDataMessage"] = "Không có dữ liệu để hiển thị hoặc xuất Excel.";
            }
            ViewBag.SearchText = searchText;
            ViewBag.Time = time;
            ViewBag.SortOrder = sortOrder;
            return View(analyses.ToPagedList(page,limit));
        }


        //public async Task<IActionResult> ExportToExcel(
        //string searchText,
        //string time,
        //string sortOrder,
        //string filterType)
        //{
        //    var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

        //    if (string.IsNullOrEmpty(managerUsername))
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }

        //    var manager = await _context.Users
        //        .Where(u => u.UserName == managerUsername)
        //        .Select(u => u.Employee)
        //        .FirstOrDefaultAsync();

        //    if (manager == null || manager.DepartmentId == null)
        //    {
        //        return RedirectToAction("Index", "Login");
        //    }

        //    // Lấy tên phòng từ DepartmentId
        //    var department = await _context.Departments
        //        .Where(d => d.Id == manager.DepartmentId)
        //        .Select(d => d.Name)
        //        .FirstOrDefaultAsync() ?? "Phòng KTDA";

        //    var employeeIdsInManagedDepartment = await _context.Employees
        //        .Where(e => e.DepartmentId == manager.DepartmentId)
        //        .Select(e => e.Id)
        //        .ToListAsync();

        //    var analyses = _context.Analyses
        //        .Include(a => a.Employee)
        //        .Where(a => a.EmployeeId.HasValue && employeeIdsInManagedDepartment.Contains(a.EmployeeId.Value));

        //    // Lọc theo tên/mã nhân viên
        //    if (!string.IsNullOrEmpty(searchText))
        //    {
        //        analyses = analyses.Where(a =>
        //            a.Employee.Code.Contains(searchText) ||
        //            a.Employee.FirstName.Contains(searchText) ||
        //            a.Employee.LastName.Contains(searchText));
        //    }
        //    string selectedMonth = "Toàn bộ";
        //    // Lọc theo tháng/năm
        //    if (!string.IsNullOrEmpty(time))
        //    {
        //        DateTime selectedDate;
        //        if (DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedDate))
        //        {
        //            selectedMonth = selectedDate.ToString("MM/yyyy");
        //            analyses = analyses.Where(a => a.Time.HasValue &&
        //                                           a.Time.Value.Month == selectedDate.Month &&
        //                                           a.Time.Value.Year == selectedDate.Year);
        //        }
        //    }

        //    // Nhóm dữ liệu theo Employee để tổng hợp
        //    var groupedData = await analyses
        //        .GroupBy(a => new { a.EmployeeId, a.Employee.Code, a.Employee.LastName, a.Employee.FirstName })
        //        .Select(g => new
        //        {
        //            EmployeeId = g.Key.EmployeeId,
        //            Code = g.Key.Code,
        //            LastName = g.Key.LastName,
        //            FirstName = g.Key.FirstName,
        //            Total = g.Sum(a => a.Total),
        //            Ontime = g.Sum(a => a.Ontime),
        //            Late = g.Sum(a => a.Late),
        //            Overdue = g.Sum(a => a.Overdue),
        //            Processing = g.Sum(a => a.Processing)
        //        })
        //        .ToListAsync();

        //    // Sắp xếp dữ liệu theo lựa chọn
        //    switch (sortOrder)
        //    {
        //        case "total_asc":
        //            groupedData = groupedData.OrderBy(a => a.Total).ToList();
        //            break;
        //        case "total_desc":
        //            groupedData = groupedData.OrderByDescending(a => a.Total).ToList();
        //            break;
        //        case "ontime_asc":
        //            groupedData = groupedData.OrderBy(a => a.Ontime).ToList();
        //            break;
        //        case "ontime_desc":
        //            groupedData = groupedData.OrderByDescending(a => a.Ontime).ToList();
        //            break;
        //        case "late_asc":
        //            groupedData = groupedData.OrderBy(a => a.Late).ToList();
        //            break;
        //        case "late_desc":
        //            groupedData = groupedData.OrderByDescending(a => a.Late).ToList();
        //            break;
        //        case "overdue_asc":
        //            groupedData = groupedData.OrderBy(a => a.Overdue).ToList();
        //            break;
        //        case "overdue_desc":
        //            groupedData = groupedData.OrderByDescending(a => a.Overdue).ToList();
        //            break;
        //        case "processing_asc":
        //            groupedData = groupedData.OrderBy(a => a.Processing).ToList();
        //            break;
        //        case "processing_desc":
        //            groupedData = groupedData.OrderByDescending(a => a.Processing).ToList();
        //            break;
        //        default:
        //            groupedData = groupedData.OrderBy(a => a.EmployeeId).ToList();
        //            break;
        //    }

        //    using (var package = new ExcelPackage())
        //    {
        //        var worksheet = package.Workbook.Worksheets.Add("Analysis Summary");

        //        worksheet.Cells[1, 1].Value = $"Bảng tổng hợp phân tích tháng {selectedMonth}";
        //        worksheet.Cells[1, 1, 1, 8].Merge = true;
        //        worksheet.Cells[1, 1].Style.Font.Size = 14;
        //        worksheet.Cells[1, 1].Style.Font.Bold = true;
        //        worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

        //        // Tiêu đề cột (dòng 2)
        //        worksheet.Cells[2, 1].Value = "STT";
        //        worksheet.Cells[2, 2].Value = "Nhân sự"; // Gộp họ và tên
        //        worksheet.Cells[2, 3].Value = "Tổng";
        //        worksheet.Cells[2, 4].Value = "Đúng hạn";
        //        worksheet.Cells[2, 5].Value = "Trễ hạn";
        //        worksheet.Cells[2, 6].Value = "Quá hạn";
        //        worksheet.Cells[2, 7].Value = "Đang xử lý";
        //        // Định dạng tiêu đề
        //        worksheet.Cells[2, 1, 2, 7].Style.Font.Bold = true;
        //        worksheet.Cells[2, 1, 2, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //        worksheet.Cells[2, 1, 2, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);
        //        worksheet.Cells[2, 1, 2, 7].Style.Font.Color.SetColor(System.Drawing.Color.White);
        //        worksheet.Cells[2, 1, 2, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;


        //        // Dữ liệu
        //        for (int i = 0; i < groupedData.Count; i++)
        //        {
        //            worksheet.Cells[i + 3, 1].Value = i + 1; // STT
        //            worksheet.Cells[i + 3, 2].Value = $"{groupedData[i].FirstName} {groupedData[i].LastName}"; // Gộp Họ và Tên
        //            worksheet.Cells[i + 3, 3].Value = groupedData[i].Total;
        //            worksheet.Cells[i + 3, 4].Value = groupedData[i].Ontime;
        //            worksheet.Cells[i + 3, 5].Value = groupedData[i].Late;
        //            worksheet.Cells[i + 3, 6].Value = groupedData[i].Overdue;
        //            worksheet.Cells[i + 3, 7].Value = groupedData[i].Processing;
        //        }
        //        worksheet.Cells[3, 1, groupedData.Count + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //        if (groupedData.Any())
        //        {
        //            // Biểu đồ cột cho các loại công việc (Tổng, Đúng hạn, Trễ hạn, Quá hạn)
        //            var columnChart = worksheet.Drawings.AddChart("ColumnChart", eChartType.ColumnClustered) as ExcelBarChart;
        //            columnChart.SetPosition(groupedData.Count + 15, 0, 1, 0); // Đặt vị trí dưới dữ liệu (dòng 15, cột 1)
        //            columnChart.SetSize(800, 400); // Kích thước biểu đồ

        //            // Dữ liệu cho biểu đồ cột
        //            int nameColumnIndex = 2;  // Cột "Nhân sự" (B)
        //            int totalColumnIndex = 3; // Cột "Tổng" (C)
        //            int ontimeColumnIndex = 4; // Cột "Đúng hạn" (D)
        //            int lateColumnIndex = 5;   // Cột "Trễ hạn" (E)
        //            int overdueColumnIndex = 6; // Cột "Quá hạn" (F)

        //            var labelRange = worksheet.Cells[3, nameColumnIndex, groupedData.Count + 2, nameColumnIndex]; // Dải nhãn (tên nhân viên)
        //            var totalRange = worksheet.Cells[3, totalColumnIndex, groupedData.Count + 2, totalColumnIndex]; // Dải dữ liệu "Tổng"
        //            var ontimeRange = worksheet.Cells[3, ontimeColumnIndex, groupedData.Count + 2, ontimeColumnIndex]; // Dải dữ liệu "Đúng hạn"
        //            var lateRange = worksheet.Cells[3, lateColumnIndex, groupedData.Count + 2, lateColumnIndex]; // Dải dữ liệu "Trễ hạn"
        //            var overdueRange = worksheet.Cells[3, overdueColumnIndex, groupedData.Count + 2, overdueColumnIndex]; // Dải dữ liệu "Quá hạn"

        //            // Thêm các series cho biểu đồ cột
        //            var seriesTotal = columnChart.Series.Add(totalRange, labelRange);
        //            seriesTotal.Header = "Tổng"; // Gán nhãn cho series "Tổng"

        //            var seriesOntime = columnChart.Series.Add(ontimeRange, labelRange);
        //            seriesOntime.Header = "Đúng hạn"; // Gán nhãn cho series "Đúng hạn"

        //            var seriesLate = columnChart.Series.Add(lateRange, labelRange);
        //            seriesLate.Header = "Trễ hạn"; // Gán nhãn cho series "Trễ hạn"

        //            var seriesOverdue = columnChart.Series.Add(overdueRange, labelRange);
        //            seriesOverdue.Header = "Quá hạn"; // Gán nhãn cho series "Quá hạn"

        //            // Cài đặt màu sắc cho các series (tương tự hình ảnh)
        //            seriesTotal.Fill.Color = System.Drawing.Color.Blue; // Màu xanh dương cho "Tổng"
        //            seriesOntime.Fill.Color = System.Drawing.Color.Green; // Màu xanh lá cho "Đúng hạn"
        //            seriesLate.Fill.Color = System.Drawing.Color.Red; // Màu đỏ cho "Trễ hạn"
        //            seriesOverdue.Fill.Color = System.Drawing.Color.Yellow; // Màu vàng cho "Quá hạn" (hoặc màu khác nếu cần)

        //            columnChart.Title.Text = "Tổng hợp tháng/năm"; // Tiêu đề biểu đồ
        //            columnChart.Legend.Position = eLegendPosition.Bottom; // Hiển thị chú thích dưới biểu đồ
        //            columnChart.DataLabel.ShowValue = true; // Hiển thị giá trị trên cột
        //            columnChart.DataLabel.Position = eLabelPosition.OutEnd; // Đặt nhãn ngoài cùng trên cột
        //        }
        //        // Thêm biểu đồ tròn
        //        if (groupedData.Any())
        //        {
        //            var chart = worksheet.Drawings.AddChart("PieChart", eChartType.Pie3D) as ExcelPieChart;
        //            chart.SetPosition(1, 0, 9, 0); // Đặt vị trí biểu đồ (dòng 10, cột 1)
        //            chart.SetSize(500, 350); // Kích thước biểu đồ
        //            chart.DataLabel.Fill.Color = System.Drawing.Color.Gray;

        //            int totalColumnIndex = 4; // Giả sử cột "Tổng công việc" ở cột D (4)
        //            int nameColumnIndex = 2;  // Giả sử cột đã gộp "Họ và Tên" ở cột B (2)

        //            var dataRange = worksheet.Cells[2, totalColumnIndex, groupedData.Count + 1, totalColumnIndex]; // Cột "Tổng"
        //            var labelRange = worksheet.Cells[2, nameColumnIndex, groupedData.Count + 1, nameColumnIndex]; // Sử dụng trực tiếp cột đã gộp "Họ và Tên"

        //            var series = chart.Series.Add(dataRange, labelRange) as ExcelPieChartSerie;

        //            chart.Title.Text = "Theo số lượng công việc";
        //            chart.Legend.Position = eLegendPosition.Left; // Hiển thị chú thích bên trái

        //            // Hiển thị phần trăm trên biểu đồ
        //            if (series != null)
        //            {
        //                /*series.DataLabel.ShowCategory = true;*/ // Hiển thị tên nhân viên (lấy từ cột đã gộp)
        //                series.DataLabel.ShowPercent = true; // Hiển thị phần trăm
        //                series.DataLabel.Position = eLabelPosition.Center; // Đặt vị trí nhãn trong biểu đồ
        //            }
        //        }


        //        // Tự động điều chỉnh kích thước cột
        //        worksheet.Cells.AutoFitColumns();

        //        using (var stream = new MemoryStream())
        //        {
        //            package.SaveAs(stream);
        //            var content = stream.ToArray();
        //            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AnalysisSummary.xlsx");
        //        }
        //    }
        //}
        public async Task<IActionResult> ExportToExcel(
    string searchText,
    string time,
    string sortOrder,
    string filterType)
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

            // Lấy tên phòng từ DepartmentId
            var department = await _context.Departments
                .Where(d => d.Id == manager.DepartmentId)
                .Select(d => d.Name)
                .FirstOrDefaultAsync() ?? "Phòng KTDA";

            var employeeIdsInManagedDepartment = await _context.Employees
                .Where(e => e.DepartmentId == manager.DepartmentId)
                .Select(e => e.Id)
                .ToListAsync();

            var analyses = _context.Analyses
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId.HasValue && employeeIdsInManagedDepartment.Contains(a.EmployeeId.Value));
            if (!analyses.Any())
            {
                TempData["NoDataMessage"] = "Không có dữ liệu để xuất Excel.";
                return RedirectToAction("Index");
            }

            // Lọc theo tên/mã nhân viên
            if (!string.IsNullOrEmpty(searchText))
            {
                analyses = analyses.Where(a =>
                    a.Employee.Code.Contains(searchText) ||
                    a.Employee.FirstName.Contains(searchText) ||
                    a.Employee.LastName.Contains(searchText));
            }

            string selectedMonth = "Toàn bộ";
            // Lọc theo tháng/năm
            if (!string.IsNullOrEmpty(time))
            {
                DateTime selectedDate;
                if (DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedDate))
                {
                    selectedMonth = selectedDate.ToString("MM/yyyy");
                    analyses = analyses.Where(a => a.Time.HasValue &&
                                                   a.Time.Value.Month == selectedDate.Month &&
                                                   a.Time.Value.Year == selectedDate.Year);
                }
            }

            // Sắp xếp kết quả theo lựa chọn của người dùng
            switch (sortOrder)
            {
                case "total_asc":
                    analyses = analyses.OrderBy(a => a.Total);
                    break;
                case "total_desc":
                    analyses = analyses.OrderByDescending(a => a.Total);
                    break;
                case "ontime_asc":
                    analyses = analyses.OrderBy(a => a.Ontime);
                    break;
                case "ontime_desc":
                    analyses = analyses.OrderByDescending(a => a.Ontime);
                    break;
                case "late_asc":
                    analyses = analyses.OrderBy(a => a.Late);
                    break;
                case "late_desc":
                    analyses = analyses.OrderByDescending(a => a.Late);
                    break;
                case "overdue_asc":
                    analyses = analyses.OrderBy(a => a.Overdue);
                    break;
                case "overdue_desc":
                    analyses = analyses.OrderByDescending(a => a.Overdue);
                    break;
                case "processing_asc":
                    analyses = analyses.OrderBy(a => a.Processing);
                    break;
                case "processing_desc":
                    analyses = analyses.OrderByDescending(a => a.Processing);
                    break;
                case "time_asc":
                    analyses = analyses.OrderBy(a => a.Time);
                    break;
                case "time_desc":
                    analyses = analyses.OrderByDescending(a => a.Time);
                    break;
                default:
                    analyses = analyses.OrderBy(a => a.Id);
                    break;
            }

            var analysesList = await analyses.ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Analysis Summary");

                worksheet.Cells[1, 1].Value = $"Bảng tổng hợp phân tích tháng {selectedMonth}";
                worksheet.Cells[1, 1, 1, 8].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Tiêu đề cột (dòng 2)
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Cells[2, 2].Value = "Nhân sự"; // Gộp họ và tên
                worksheet.Cells[2, 3].Value = "Tổng";
                worksheet.Cells[2, 4].Value = "Đúng hạn";
                worksheet.Cells[2, 5].Value = "Trễ hạn";
                worksheet.Cells[2, 6].Value = "Quá hạn";
                worksheet.Cells[2, 7].Value = "Đang xử lý";
                worksheet.Cells[2, 8].Value = "Thời gian";
                // Định dạng tiêu đề
                worksheet.Cells[2, 1, 2, 8].Style.Font.Bold = true;
                worksheet.Cells[2, 1, 2, 8].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[2, 1, 2, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);
                worksheet.Cells[2, 1, 2, 8].Style.Font.Color.SetColor(System.Drawing.Color.White);
                worksheet.Cells[2, 1, 2, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Dữ liệu
                for (int i = 0; i < analysesList.Count; i++)
                {
                    worksheet.Cells[i + 3, 1].Value = i + 1; // STT
                    worksheet.Cells[i + 3, 2].Value = $"{analysesList[i].Employee.FirstName} {analysesList[i].Employee.LastName}"; // Gộp Họ và Tên
                    worksheet.Cells[i + 3, 3].Value = analysesList[i].Total;
                    worksheet.Cells[i + 3, 4].Value = analysesList[i].Ontime;
                    worksheet.Cells[i + 3, 5].Value = analysesList[i].Late;
                    worksheet.Cells[i + 3, 6].Value = analysesList[i].Overdue;
                    worksheet.Cells[i + 3, 7].Value = analysesList[i].Processing;
                    worksheet.Cells[i + 3, 8].Value = analysesList[i].Time?.ToString("MM/yyyy") ?? "";
                }
                worksheet.Cells[3, 1, analysesList.Count + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                if (analysesList.Any())
                {
                    // Biểu đồ cột
                    var columnChart = worksheet.Drawings.AddChart("ColumnChart", eChartType.ColumnClustered) as ExcelBarChart;
                    columnChart.SetPosition(analysesList.Count + 15, 0, 1, 0);
                    columnChart.SetSize(800, 400);

                    int nameColumnIndex = 2;  // Cột "Nhân sự" (B)
                    int totalColumnIndex = 3; // Cột "Tổng" (C)
                    int ontimeColumnIndex = 4; // Cột "Đúng hạn" (D)
                    int lateColumnIndex = 5;   // Cột "Trễ hạn" (E)
                    int overdueColumnIndex = 6; // Cột "Quá hạn" (F)

                    var labelRange = worksheet.Cells[3, nameColumnIndex, analysesList.Count + 2, nameColumnIndex];
                    var totalRange = worksheet.Cells[3, totalColumnIndex, analysesList.Count + 2, totalColumnIndex];
                    var ontimeRange = worksheet.Cells[3, ontimeColumnIndex, analysesList.Count + 2, ontimeColumnIndex];
                    var lateRange = worksheet.Cells[3, lateColumnIndex, analysesList.Count + 2, lateColumnIndex];
                    var overdueRange = worksheet.Cells[3, overdueColumnIndex, analysesList.Count + 2, overdueColumnIndex];

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
                }

                if (analysesList.Any())
                {
                    var chart = worksheet.Drawings.AddChart("PieChart", eChartType.Pie3D) as ExcelPieChart;
                    chart.SetPosition(1, 0, 9, 0);
                    chart.SetSize(500, 350);

                    int totalColumnIndex = 3; // Cột "Tổng" (C)
                    int nameColumnIndex = 2;  // Cột "Nhân sự" (B)

                    var dataRange = worksheet.Cells[3, totalColumnIndex, analysesList.Count + 2, totalColumnIndex];
                    var labelRange = worksheet.Cells[3, nameColumnIndex, analysesList.Count + 2, nameColumnIndex];

                    var series = chart.Series.Add(dataRange, labelRange) as ExcelPieChartSerie;
                    chart.Title.Text = "Theo số lượng công việc";
                    chart.Legend.Position = eLegendPosition.Left;

                    if (series != null)
                    {
                        series.DataLabel.ShowPercent = true;
                        series.DataLabel.Position = eLabelPosition.Center;
                    }
                }

                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Analysis_{selectedMonth}.xlsx");
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
                _context.Analyses.Remove(analysis);
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