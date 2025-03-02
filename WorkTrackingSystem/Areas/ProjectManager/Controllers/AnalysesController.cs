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

            return View(await analyses.ToListAsync());
        }


        public ActionResult ExportToExcel()
        {
            var analyses = _context.Analyses.ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Analysis");

                // **1. Tạo Tiêu đề**
                worksheet.Cells["A1"].Value = "BÁO CÁO PHÂN TÍCH CÔNG VIỆC";
                worksheet.Cells["A1:E1"].Merge = true;
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;

                // **2. Header của bảng**
                worksheet.Cells[3, 1].Value = "ID";
                worksheet.Cells[3, 2].Value = "Employee ID";
                worksheet.Cells[3, 3].Value = "Ontime";
                worksheet.Cells[3, 4].Value = "Late";
                worksheet.Cells[3, 5].Value = "Overdue";
                worksheet.Cells[3, 6].Value = "Processing";

                worksheet.Cells["A3:F3"].Style.Font.Bold = true;

                // **3. Thêm dữ liệu vào bảng**
                int row = 4;
                foreach (var item in analyses)
                {
                    worksheet.Cells[row, 1].Value = item.Id;
                    worksheet.Cells[row, 2].Value = item.EmployeeId;
                    worksheet.Cells[row, 3].Value = item.Ontime;
                    worksheet.Cells[row, 4].Value = item.Late;
                    worksheet.Cells[row, 5].Value = item.Overdue;
                    worksheet.Cells[row, 6].Value = item.Processing;
                    row++;
                }

                // **4. Tạo biểu đồ tròn (Pie Chart)**
                var chart = worksheet.Drawings.AddChart("chart", eChartType.Pie) as ExcelPieChart;
                chart.Title.Text = "Tỷ lệ công việc";
                chart.SetPosition(2, 0, 7, 0);
                chart.SetSize(400, 400);

                // **5. Gán dữ liệu cho biểu đồ**
                var dataRange = worksheet.Cells[$"C3:C{row - 1}"]; // Lấy dữ liệu từ cột Ontime
                var labelRange = worksheet.Cells[$"B3:B{row - 1}"]; // Lấy Employee ID làm nhãn

                chart.Series.Add(dataRange, labelRange);
                chart.DataLabel.ShowPercent = true; // Hiển thị phần trăm trên biểu đồ

                // **6. Xuất tệp Excel**
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Analysis_Report.xlsx");
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
