using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Areas.ProjectManager.Models;
namespace WorkTrackingSystem.Models​;

public partial class WorkTrackingSystemContext : DbContext
{
    public WorkTrackingSystemContext()
    {
    }

    public WorkTrackingSystemContext(DbContextOptions<WorkTrackingSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Analysis> Analyses { get; set; }

    public virtual DbSet<Baselineassessment> Baselineassessments { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<Jobmapemployee> Jobmapemployees { get; set; }

    public virtual DbSet<Jobrepeat> Jobrepeats { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<Scoreemployee> Scoreemployees { get; set; }

    public virtual DbSet<Systemsw> Systemsws { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DAMQUOCDAN;Database=WorkTrackingSystem;uid=sa;pwd=1234;MultipleActiveResultSets=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Analysis>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ANALYSIS__3214EC07400D5EE4");

            entity.ToTable("ANALYSIS");

            entity.Property(e => e.CreateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Create_By");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_Date");
            entity.Property(e => e.EmployeeId).HasColumnName("Employee_Id");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Late).HasDefaultValue(0);
            entity.Property(e => e.Ontime).HasDefaultValue(0);
            entity.Property(e => e.Overdue).HasDefaultValue(0);
            entity.Property(e => e.Processing).HasDefaultValue(0);
            entity.Property(e => e.Time).HasColumnType("datetime");
            entity.Property(e => e.Total).HasDefaultValue(0.0);
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Update_By");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");

            entity.HasOne(d => d.Employee).WithMany(p => p.Analyses)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__ANALYSIS__Employ__2739D489");
        });

        modelBuilder.Entity<Baselineassessment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BASELINE__3214EC07995C3A9A");

            entity.ToTable("BASELINEASSESSMENT");

            entity.Property(e => e.CreateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Create_By");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_Date");
            entity.Property(e => e.EmployeeId).HasColumnName("Employee_Id");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.ProgressAssessment).HasDefaultValue(0.0);
            entity.Property(e => e.QualityAssessment).HasDefaultValue(0.0);
            entity.Property(e => e.SummaryOfReviews).HasDefaultValue(0.0);
            entity.Property(e => e.Time).HasColumnType("datetime");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Update_By");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");
            entity.Property(e => e.VolumeAssessment).HasDefaultValue(0.0);

            entity.HasOne(d => d.Employee).WithMany(p => p.Baselineassessments)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__BASELINEA__Emplo__282DF8C2");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CATEGORY__3214EC07D45447C3");

            entity.ToTable("CATEGORY");

            entity.HasIndex(e => e.Code, "UQ__CATEGORY__A25C5AA74E76B80E").IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Create_By");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_Date");
            entity.Property(e => e.IdParent)
                .HasDefaultValue(0L)
                .HasColumnName("Id_Parent");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Update_By");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DEPARTME__3214EC07D534E2F1");

            entity.ToTable("DEPARTMENT");

            entity.HasIndex(e => e.Code, "UQ__DEPARTME__A25C5AA775CD911D").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Create_By");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_Date");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Update_By");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EMPLOYEE__3214EC077404DA32");

            entity.ToTable("EMPLOYEE");

            entity.HasIndex(e => e.Code, "UQ__EMPLOYEE__A25C5AA764DD30DB").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Create_By");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_Date");
            entity.Property(e => e.DepartmentId).HasColumnName("Department_Id");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .HasColumnName("First_Name");
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.HireDate).HasColumnName("Hire_Date");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .HasColumnName("Last_Name");
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.PositionId).HasColumnName("Position_Id");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Update_By");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__EMPLOYEE__Depart__29221CFB");

            entity.HasOne(d => d.Position).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PositionId)
                .HasConstraintName("FK__EMPLOYEE__Positi__2A164134");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JOB__3214EC078EDA6055");

            entity.ToTable("JOB");

            entity.Property(e => e.CategoryId).HasColumnName("Category_Id");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Create_By");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_Date");
            entity.Property(e => e.Deadline1).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Deadline2).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Deadline3).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.RecurrenceEndDate).HasColumnName("Recurrence_End_Date");
            entity.Property(e => e.RecurrenceInterval)
                .HasDefaultValue((byte)1)
                .HasColumnName("Recurrence_Interval");
            entity.Property(e => e.RecurrenceType)
                .HasDefaultValue((byte)0)
                .HasColumnName("Recurrence_Type");
            entity.Property(e => e.Recurring).HasDefaultValue(false);
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Update_By");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");

            entity.HasOne(d => d.Category).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__JOB__Category_Id__245D67DE");
        });

        modelBuilder.Entity<Jobmapemployee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JOBMAPEM__3214EC07922EFC05");

            entity.ToTable("JOBMAPEMPLOYEE");

            entity.Property(e => e.CreateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Create_By");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_Date");
            entity.Property(e => e.EmployeeId).HasColumnName("Employee_Id");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.JobId).HasColumnName("Job_Id");
            entity.Property(e => e.JobRepeatId).HasColumnName("JobRepeat_Id");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Update_By");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");

            entity.HasOne(d => d.Employee).WithMany(p => p.Jobmapemployees)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__JOBMAPEMP__Emplo__2FCF1A8A");

            entity.HasOne(d => d.Job).WithMany(p => p.Jobmapemployees)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__JOBMAPEMP__Job_I__30C33EC3");

            entity.HasOne(d => d.JobRepeat).WithMany(p => p.Jobmapemployees)
                .HasForeignKey(d => d.JobRepeatId)
                .HasConstraintName("FK_JOBMAPEMPLOYEE_JOBREPEAT");
        });

        modelBuilder.Entity<Jobrepeat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_JobRepeat");

            entity.ToTable("JOBREPEAT");

            entity.Property(e => e.CreateBy)
                .HasMaxLength(100)
                .HasColumnName("Create_By");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("Create_Date");
            entity.Property(e => e.JobId).HasColumnName("Job_Id");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .HasColumnName("Update_By");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");

            entity.HasOne(d => d.Job).WithMany(p => p.Jobrepeats)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_JOBREPEAT_JOB");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__POSITION__3214EC078A0F3D81");

            entity.ToTable("POSITION");

            entity.Property(e => e.CreateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Create_By");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_Date");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Status).HasDefaultValue(false);
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Update_By");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SCORE__3214EC071B698CBB");

            entity.ToTable("SCORE");

            entity.Property(e => e.CompletionDate).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Create_By");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_Date");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.JobMapEmployeeId).HasColumnName("JobMapEmployee_Id");
            entity.Property(e => e.Progress).HasDefaultValue(0.0);
            entity.Property(e => e.ProgressAssessment).HasDefaultValue(0.0);
            entity.Property(e => e.QualityAssessment).HasDefaultValue(0.0);
            entity.Property(e => e.SummaryOfReviews).HasDefaultValue(0.0);
            entity.Property(e => e.Time).HasColumnType("datetime");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Update_By");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");
            entity.Property(e => e.VolumeAssessment).HasDefaultValue(0.0);

            entity.HasOne(d => d.JobMapEmployee).WithMany(p => p.Scores)
                .HasForeignKey(d => d.JobMapEmployeeId)
                .HasConstraintName("FK__SCORE__JobMapEmp__395884C4");
        });

        modelBuilder.Entity<Scoreemployee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CoreEmployee");

            entity.ToTable("SCOREEMPLOYEE", tb =>
                {
                    tb.HasTrigger("trg_insert");
                    tb.HasTrigger("trg_update");
                });

            entity.Property(e => e.CreateBy)
                .HasMaxLength(100)
                .HasColumnName("Create_By");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("Create_Date");
            entity.Property(e => e.JobMapEmployeeId).HasColumnName("JobMapEmployee_Id");
            entity.Property(e => e.Time).HasColumnType("datetime");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .HasColumnName("Update_By");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");

            entity.HasOne(d => d.JobMapEmployee).WithMany(p => p.Scoreemployees)
                .HasForeignKey(d => d.JobMapEmployeeId)
                .HasConstraintName("FK_SCOREEMPLOYEE_JOBMAPEMPLOYEE");
        });

        modelBuilder.Entity<Systemsw>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SYSTEMSW__3214EC072D86F414");

            entity.ToTable("SYSTEMSW");

            entity.Property(e => e.CreateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Create_By");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_Date");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Name).IsUnicode(false);
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Update_By");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__USERS__3214EC0794F2164C");

            entity.ToTable("USERS");

            entity.HasIndex(e => e.UserName, "UQ__USERS__C9F2845686B24373").IsUnique();

            entity.Property(e => e.CreateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Create_By");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Create_Date");
            entity.Property(e => e.EmployeeId).HasColumnName("Employee_Id");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.Password).IsUnicode(false);
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("Update_By");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Update_Date");
            entity.Property(e => e.UserName)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Employee).WithMany(p => p.Users)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__USERS__Employee___31B762FC");
        });
        modelBuilder.Entity<EmployeeScoreSummary>()
                .HasNoKey()
                .ToView(null);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
