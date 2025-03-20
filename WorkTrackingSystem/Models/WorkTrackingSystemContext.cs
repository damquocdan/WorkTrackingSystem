using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<Systemsw> Systemsws { get; set; }

    public virtual DbSet<User> Users { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=DAMQUOCDAN;Database=WorkTrackingSystem;uid=sa;pwd=1234;MultipleActiveResultSets=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Analysis>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ANALYSIS__3214EC07E14945A9");

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
                .HasConstraintName("FK__ANALYSIS__Employ__047AA831");
        });

        modelBuilder.Entity<Baselineassessment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BASELINE__3214EC07D1C43AEF");

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
                .HasConstraintName("FK__BASELINEA__Emplo__056ECC6A");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CATEGORY__3214EC079BEF1BC2");

            entity.ToTable("CATEGORY");

            entity.HasIndex(e => e.Code, "UQ__CATEGORY__A25C5AA7E97CBFA1").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__DEPARTME__3214EC070155D190");

            entity.ToTable("DEPARTMENT");

            entity.HasIndex(e => e.Code, "UQ__DEPARTME__A25C5AA7DD20E978").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__EMPLOYEE__3214EC07114771A5");

            entity.ToTable("EMPLOYEE");

            entity.HasIndex(e => e.Code, "UQ__EMPLOYEE__A25C5AA74D2E5399").IsUnique();

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
                .HasConstraintName("FK__EMPLOYEE__Depart__0662F0A3");

            entity.HasOne(d => d.Position).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PositionId)
                .HasConstraintName("FK__EMPLOYEE__Positi__075714DC");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JOB__3214EC07CF635702");

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
                .HasConstraintName("FK__JOB__Category_Id__084B3915");
        });

        modelBuilder.Entity<Jobmapemployee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JOBMAPEM__3214EC07D21766BE");

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
                .HasConstraintName("FK__JOBMAPEMP__Emplo__093F5D4E");

            entity.HasOne(d => d.Job).WithMany(p => p.Jobmapemployees)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK__JOBMAPEMP__Job_I__0A338187");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__POSITION__3214EC0732AD3B49");

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
            entity.HasKey(e => e.Id).HasName("PK__SCORE__3214EC070872C7AA");

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
                .HasConstraintName("FK__SCORE__JobMapEmp__0B27A5C0");
        });

        modelBuilder.Entity<Systemsw>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SYSTEMSW__3214EC07147B2F97");

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
            entity.HasKey(e => e.Id).HasName("PK__USERS__3214EC072FACE72E");

            entity.ToTable("USERS");

            entity.HasIndex(e => e.UserName, "UQ__USERS__C9F2845633777A70").IsUnique();

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
                .HasConstraintName("FK__USERS__Employee___0C1BC9F9");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
