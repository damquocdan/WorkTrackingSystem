namespace WorkTrackingSystem.Areas.EmployeeSystem.Services
{
	using Microsoft.EntityFrameworkCore;
	using MimeKit;
	using MailKit.Net.Smtp;
	using global::WorkTrackingSystem.Models;

	public class EmailNotificationService
	{
		private readonly WorkTrackingSystemContext _context;
		private readonly IConfiguration _configuration;

		public EmailNotificationService(WorkTrackingSystemContext context, IConfiguration configuration)
		{
			_context = context;
			_configuration = configuration;
		}

		public async Task SendDeadlineRemindersAsync()
		{
			var tomorrow = DateTime.Today.AddDays(1);

			var jobsDueTomorrow = await _context.Jobs
				.Where(j => j.Deadline1.HasValue && j.Deadline1.Value == DateOnly.FromDateTime(tomorrow))
				.Join(_context.Jobmapemployees,
					job => job.Id,
					jme => jme.JobId,
					(job, jme) => new { Job = job, JobMapEmployee = jme })
				.Join(_context.Employees,
					jme => jme.JobMapEmployee.EmployeeId,
					emp => emp.Id,
					(jme, emp) => new
					{
						JobTitle = jme.Job.Name,
						Deadline = jme.Job.Deadline1.HasValue ? jme.Job.Deadline1.Value.ToString("yyyy-MM-dd") : "Không có deadline",
						EmployeeEmail = emp.Email,
						EmployeeName = $"{emp.FirstName ?? ""} {emp.LastName ?? ""}".Trim(),
					})
				.ToListAsync();

			foreach (var job in jobsDueTomorrow)
			{
				await SendEmailAsync(
					job.EmployeeEmail,
					"Nhắc nhở Deadline công việc",
					BuildEmailBody(job.EmployeeName, job.JobTitle, job.Deadline)
				);
			}
		}

		private async Task SendEmailAsync(string toEmail, string subject, string body)
		{
			var senderEmail = _configuration["EmailSettings:SenderEmail"];
			var smtpServer = _configuration["EmailSettings:SmtpServer"];
			var port = _configuration["EmailSettings:Port"];
			var username = _configuration["EmailSettings:Username"];
			var password = _configuration["EmailSettings:Password"];

			if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
			{
				throw new InvalidOperationException("Email configuration is missing or incomplete.");
			}

			var email = new MimeMessage();
			email.From.Add(new MailboxAddress("Hệ thống quản lý công việc", senderEmail));
			email.To.Add(new MailboxAddress("", toEmail));
			email.Subject = subject;

			var bodyBuilder = new BodyBuilder
			{
				HtmlBody = body
			};
			email.Body = bodyBuilder.ToMessageBody();

			using var smtp = new SmtpClient();
			await smtp.ConnectAsync(smtpServer, int.Parse(port), true);
			await smtp.AuthenticateAsync(username, password);
			await smtp.SendAsync(email);
			await smtp.DisconnectAsync(true);
		}

		private string BuildEmailBody(string employeeName, string jobTitle, string deadline)
		{
			return $@"
                <h3>Xin chào {employeeName},</h3>
                <p>Đây là thông báo nhắc nhở về công việc sắp đến hạn của bạn:</p>
                <ul>
                    <li><strong>Tên công việc:</strong> {jobTitle}</li>
                    <li><strong>Deadline:</strong> {deadline}</li>
                </ul>
                <p>Vui lòng hoàn thành công việc đúng hạn. Nếu có vấn đề gì, hãy liên hệ với quản lý của bạn.</p>
                <p>Trân trọng,<br/>Hệ thống quản lý công việc</p>";
		}
	}

	public class DeadlineReminderBackgroundService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<DeadlineReminderBackgroundService> _logger;

		public DeadlineReminderBackgroundService(IServiceProvider serviceProvider, ILogger<DeadlineReminderBackgroundService> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("DeadlineReminderBackgroundService started.");
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					_logger.LogInformation("Running deadline reminder at {Time}", DateTime.Now);
					using var scope = _serviceProvider.CreateScope();
					var emailService = scope.ServiceProvider.GetRequiredService<EmailNotificationService>();
					await emailService.SendDeadlineRemindersAsync();
					_logger.LogInformation("Deadline reminder completed at {Time}", DateTime.Now);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error occurred while sending deadline reminders.");
				}

				var now = DateTime.Now;
				var nextRun = now.Date.AddDays(1);
				var delay = nextRun - now;
				_logger.LogInformation("Next run scheduled at {NextRun}", nextRun);
				await Task.Delay(delay, stoppingToken);
			}
			_logger.LogInformation("DeadlineReminderBackgroundService stopped.");
		}
	}
}