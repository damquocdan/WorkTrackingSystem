USE [WorkTrackingSystem]
GO
/****** Object:  Table [dbo].[ANALYSIS]    Script Date: 3/25/2025 7:08:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ANALYSIS](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Employee_Id] [bigint] NULL,
	[Total] [float] NULL,
	[Ontime] [int] NULL,
	[Late] [int] NULL,
	[Overdue] [int] NULL,
	[Processing] [int] NULL,
	[Time] [datetime] NULL,
	[IsDelete] [bit] NULL,
	[IsActive] [bit] NULL,
	[Create_Date] [datetime] NULL,
	[Update_Date] [datetime] NULL,
	[Create_By] [nvarchar](100) NULL,
	[Update_By] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BASELINEASSESSMENT]    Script Date: 3/25/2025 7:08:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BASELINEASSESSMENT](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Employee_Id] [bigint] NULL,
	[VolumeAssessment] [float] NULL,
	[ProgressAssessment] [float] NULL,
	[QualityAssessment] [float] NULL,
	[SummaryOfReviews] [float] NULL,
	[Time] [datetime] NULL,
	[Evaluate] [bit] NULL,
	[IsDelete] [bit] NULL,
	[IsActive] [bit] NULL,
	[Create_Date] [datetime] NULL,
	[Update_Date] [datetime] NULL,
	[Create_By] [nvarchar](100) NULL,
	[Update_By] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CATEGORY]    Script Date: 3/25/2025 7:08:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CATEGORY](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](50) NOT NULL,
	[Name] [nvarchar](255) NULL,
	[Description] [nvarchar](max) NULL,
	[Id_Parent] [bigint] NULL,
	[IsDelete] [bit] NULL,
	[IsActive] [bit] NULL,
	[Create_Date] [datetime] NULL,
	[Update_Date] [datetime] NULL,
	[Create_By] [nvarchar](100) NULL,
	[Update_By] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DEPARTMENT]    Script Date: 3/25/2025 7:08:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DEPARTMENT](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](255) NULL,
	[Description] [nvarchar](max) NULL,
	[IsDelete] [bit] NULL,
	[IsActive] [bit] NULL,
	[Create_Date] [datetime] NULL,
	[Update_Date] [datetime] NULL,
	[Create_By] [nvarchar](100) NULL,
	[Update_By] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EMPLOYEE]    Script Date: 3/25/2025 7:08:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EMPLOYEE](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Department_Id] [bigint] NULL,
	[Position_Id] [bigint] NULL,
	[Code] [nvarchar](50) NOT NULL,
	[First_Name] [nvarchar](255) NULL,
	[Last_Name] [nvarchar](255) NULL,
	[Gender] [nvarchar](10) NULL,
	[Birthday] [date] NULL,
	[Phone] [nvarchar](15) NULL,
	[Email] [nvarchar](255) NULL,
	[Hire_Date] [date] NULL,
	[Address] [nvarchar](max) NULL,
	[Avatar] [nvarchar](max) NULL,
	[IsDelete] [bit] NULL,
	[IsActive] [bit] NULL,
	[Create_Date] [datetime] NULL,
	[Update_Date] [datetime] NULL,
	[Create_By] [nvarchar](100) NULL,
	[Update_By] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[JOB]    Script Date: 3/25/2025 7:08:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JOB](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Category_Id] [bigint] NULL,
	[Name] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Deadline1] [date] NULL,
	[Deadline2] [date] NULL,
	[Deadline3] [date] NULL,
	[IsDelete] [bit] NULL,
	[IsActive] [bit] NULL,
	[Create_Date] [datetime] NULL,
	[Update_Date] [datetime] NULL,
	[Create_By] [nvarchar](100) NULL,
	[Update_By] [nvarchar](100) NULL,
 CONSTRAINT [PK__JOB__3214EC078EDA6055] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[JOBMAPEMPLOYEE]    Script Date: 3/25/2025 7:08:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JOBMAPEMPLOYEE](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Employee_Id] [bigint] NULL,
	[Job_Id] [bigint] NULL,
	[IsDelete] [bit] NULL,
	[IsActive] [bit] NULL,
	[Create_Date] [datetime] NULL,
	[Update_Date] [datetime] NULL,
	[Create_By] [nvarchar](100) NULL,
	[Update_By] [nvarchar](100) NULL,
 CONSTRAINT [PK__JOBMAPEM__3214EC07922EFC05] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[POSITION]    Script Date: 3/25/2025 7:08:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[POSITION](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NULL,
	[Description] [nvarchar](max) NULL,
	[Status] [bit] NULL,
	[IsDelete] [bit] NULL,
	[IsActive] [bit] NULL,
	[Create_Date] [datetime] NULL,
	[Update_Date] [datetime] NULL,
	[Create_By] [nvarchar](100) NULL,
	[Update_By] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SCORE]    Script Date: 3/25/2025 7:08:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SCORE](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[JobMapEmployee_Id] [bigint] NULL,
	[CompletionDate] [date] NULL,
	[Status] [tinyint] NULL,
	[VolumeAssessment] [float] NULL,
	[ProgressAssessment] [float] NULL,
	[QualityAssessment] [float] NULL,
	[SummaryOfReviews] [float] NULL,
	[Progress] [float] NULL,
	[Time] [datetime] NULL,
	[IsDelete] [bit] NULL,
	[IsActive] [bit] NULL,
	[Create_Date] [datetime] NULL,
	[Update_Date] [datetime] NULL,
	[Create_By] [nvarchar](100) NULL,
	[Update_By] [nvarchar](100) NULL,
 CONSTRAINT [PK__SCORE__3214EC071B698CBB] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SYSTEMSW]    Script Date: 3/25/2025 7:08:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SYSTEMSW](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](max) NULL,
	[Value] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[IsDelete] [bit] NULL,
	[IsActive] [bit] NULL,
	[Create_Date] [datetime] NULL,
	[Update_Date] [datetime] NULL,
	[Create_By] [nvarchar](100) NULL,
	[Update_By] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[USERS]    Script Date: 3/25/2025 7:08:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[USERS](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserName] [varchar](255) NOT NULL,
	[Password] [varchar](max) NULL,
	[Employee_Id] [bigint] NULL,
	[IsDelete] [bit] NULL,
	[IsActive] [bit] NULL,
	[Create_Date] [datetime] NULL,
	[Update_Date] [datetime] NULL,
	[Create_By] [nvarchar](100) NULL,
	[Update_By] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[ANALYSIS] ON 

INSERT [dbo].[ANALYSIS] ([Id], [Employee_Id], [Total], [Ontime], [Late], [Overdue], [Processing], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (1, 4, 34, 0, 7, 27, 0, CAST(N'2025-03-01T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-11T21:27:30.283' AS DateTime), CAST(N'2025-03-25T17:45:02.990' AS DateTime), NULL, NULL)
INSERT [dbo].[ANALYSIS] ([Id], [Employee_Id], [Total], [Ontime], [Late], [Overdue], [Processing], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (2, 5, 24, 24, 0, 0, 0, CAST(N'2025-03-01T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-11T21:27:30.413' AS DateTime), CAST(N'2025-03-17T13:28:49.693' AS DateTime), NULL, NULL)
INSERT [dbo].[ANALYSIS] ([Id], [Employee_Id], [Total], [Ontime], [Late], [Overdue], [Processing], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (3, 5, 0, 0, 0, 0, 0, CAST(N'2024-12-01T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-17T13:28:49.510' AS DateTime), CAST(N'2025-03-17T13:28:49.510' AS DateTime), NULL, NULL)
INSERT [dbo].[ANALYSIS] ([Id], [Employee_Id], [Total], [Ontime], [Late], [Overdue], [Processing], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (4, 5, 1, 1, 0, 0, 0, CAST(N'2025-04-01T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-17T13:28:49.727' AS DateTime), CAST(N'2025-03-17T13:28:49.727' AS DateTime), NULL, NULL)
INSERT [dbo].[ANALYSIS] ([Id], [Employee_Id], [Total], [Ontime], [Late], [Overdue], [Processing], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (5, 5, 1, 1, 0, 0, 0, CAST(N'2025-05-01T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-17T13:28:49.740' AS DateTime), CAST(N'2025-03-17T13:28:49.740' AS DateTime), NULL, NULL)
INSERT [dbo].[ANALYSIS] ([Id], [Employee_Id], [Total], [Ontime], [Late], [Overdue], [Processing], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (6, 4, 25, 20, 1, 4, 0, CAST(N'2024-12-01T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-17T13:30:09.053' AS DateTime), CAST(N'2025-03-17T13:30:19.097' AS DateTime), NULL, NULL)
SET IDENTITY_INSERT [dbo].[ANALYSIS] OFF
GO
SET IDENTITY_INSERT [dbo].[BASELINEASSESSMENT] ON 

INSERT [dbo].[BASELINEASSESSMENT] ([Id], [Employee_Id], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Time], [Evaluate], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (1, 4, 3, 3, 3, 3, CAST(N'2025-03-01T00:00:00.000' AS DateTime), 0, 0, 1, CAST(N'2025-03-11T21:29:33.257' AS DateTime), CAST(N'2025-03-17T13:39:45.733' AS DateTime), NULL, NULL)
INSERT [dbo].[BASELINEASSESSMENT] ([Id], [Employee_Id], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Time], [Evaluate], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (2, 5, 28, 66, 64, 42.699999570846558, CAST(N'2025-03-01T00:00:00.000' AS DateTime), 0, 0, 1, CAST(N'2025-03-17T11:03:16.937' AS DateTime), CAST(N'2025-03-17T13:28:49.667' AS DateTime), NULL, NULL)
INSERT [dbo].[BASELINEASSESSMENT] ([Id], [Employee_Id], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Time], [Evaluate], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (3, 5, 1, 3, 2.5, 1.6749999523162842, CAST(N'2025-04-01T00:00:00.000' AS DateTime), 0, 0, 1, CAST(N'2025-03-17T13:28:49.703' AS DateTime), CAST(N'2025-03-17T13:28:49.703' AS DateTime), NULL, NULL)
INSERT [dbo].[BASELINEASSESSMENT] ([Id], [Employee_Id], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Time], [Evaluate], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (4, 5, 1.5, 3, 3, 2.0999999046325684, CAST(N'2025-05-01T00:00:00.000' AS DateTime), 0, 0, 1, CAST(N'2025-03-17T13:28:49.733' AS DateTime), CAST(N'2025-03-17T13:28:49.733' AS DateTime), NULL, NULL)
INSERT [dbo].[BASELINEASSESSMENT] ([Id], [Employee_Id], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Time], [Evaluate], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (5, 4, 28.5, 39, 45, 34.199999570846558, CAST(N'2024-12-01T00:00:00.000' AS DateTime), 0, 0, 1, CAST(N'2025-03-17T13:30:09.043' AS DateTime), CAST(N'2025-03-17T13:30:19.093' AS DateTime), NULL, NULL)
SET IDENTITY_INSERT [dbo].[BASELINEASSESSMENT] OFF
GO
SET IDENTITY_INSERT [dbo].[CATEGORY] ON 

INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (1, N'TTNCTT', N'TTNCTT', N'TTNCTT', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (2, N'CT', N'Cải tạo', N'Cải tạo', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (3, N'KY', N'Khoa Y', N'Khoa Y', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (4, N'KH', N'Khác', N'Khác', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (5, N'BC', N'Báo cáo', N'Báo cáo', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (6, N'VB', N'Văn bản', N'Văn bản', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (7, N'ED', N'EDVS', N'EDVS', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (8, N'QH', N'QH TMB', N'QH TMB', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (9, N'HC', N'Hành chính', N'Hành chính', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (10, N'KT', N'Kiểm toán', N'Kiểm toán', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (11, N'TH', N'Tổng hợp', N'Tổng hợp', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (12, N'DC', N'Điều chỉnh dự án', N'Điều chỉnh dự án', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (13, N'MT', N'MTXH', N'MTXH', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (14, N'TT', N'Thanh toán', N'Thanh toán', 0, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[CATEGORY] ([Id], [Code], [Name], [Description], [Id_Parent], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (15, N'DT', N'Đấu thầu', N'Đấu thầu', 0, 1, 0, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
SET IDENTITY_INSERT [dbo].[CATEGORY] OFF
GO
SET IDENTITY_INSERT [dbo].[DEPARTMENT] ON 

INSERT [dbo].[DEPARTMENT] ([Id], [Code], [Name], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (1, N'HR', N'Phòng Nhân sự', N'Chịu trách nhiệm quản lý nhân sự', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[DEPARTMENT] ([Id], [Code], [Name], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (2, N'IT', N'Phòng Công nghệ thông tin', N'Phát triển và bảo trì hệ thống', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[DEPARTMENT] ([Id], [Code], [Name], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (3, N'TC-KT', N'Phòng Tài chính', N'Quản lý tài chính và kế toán', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[DEPARTMENT] ([Id], [Code], [Name], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (4, N'KT', N'Phòng Kỹ thuật', N'Phòng phụ trách kỹ thuật', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[DEPARTMENT] ([Id], [Code], [Name], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (5, N'KD', N'Phòng Kinh doanh', N'Phòng phụ trách kinh doanh', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[DEPARTMENT] ([Id], [Code], [Name], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (6, N'QL', N'Phòng Quản Lý', N'Phòng Quản Lý', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
SET IDENTITY_INSERT [dbo].[DEPARTMENT] OFF
GO
SET IDENTITY_INSERT [dbo].[EMPLOYEE] ON 

INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (1, 1, 1, N'E001', N'Admin', N'Admin', N'Nam', CAST(N'1990-05-20' AS Date), N'0901234567', N'a.nguyen@example.com', CAST(N'2020-03-15' AS Date), N'Hà Nội', N'/images/employees/76a802af-b5de-4169-b374-b6704a1ae1a5_avatar1.png', 0, 1, CAST(N'2025-02-20T10:10:05.890' AS DateTime), CAST(N'2025-03-24T09:40:42.800' AS DateTime), NULL, N'admin')
INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (2, 2, 3, N'E002', N'Trần', N'Manage', N'Nữ', CAST(N'1985-07-10' AS Date), N'0912345678', N'b.tran@example.com', CAST(N'2018-06-01' AS Date), N'TP. Hồ Chí Minh', N'', 0, 1, CAST(N'2025-02-20T10:10:05.890' AS DateTime), CAST(N'2025-03-24T09:27:42.057' AS DateTime), NULL, N'Trần Manage')
INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (3, 1, 5, N'E003', N'Nguyễn ', N'HR', N'Nam', CAST(N'1980-12-05' AS Date), N'0987654321', N'c.le@example.com', CAST(N'2015-09-20' AS Date), N'Đà Nẵng', N'', 0, 1, CAST(N'2025-02-20T10:10:05.890' AS DateTime), CAST(N'2025-02-20T10:10:05.890' AS DateTime), NULL, NULL)
INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (4, 2, 4, N'EM001', N'Lê ', N'Duy Bình', N'Nam', CAST(N'1900-01-01' AS Date), N'901234567', N'lebinh@worktracking.com', CAST(N'1900-01-01' AS Date), N'Hà Nội', N'/images/6165775c-ff9d-4b44-bc58-367eacb4796d_avatar.png', 0, NULL, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'2025-03-24T17:35:26.027' AS DateTime), N'', N'Lê  Duy Bình')
INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (5, 2, 4, N'EM002', N'Trần', N'Đức Huy', N'Male', CAST(N'1900-01-01' AS Date), N'902345678', N'tranhuy@worktracking.com', CAST(N'1900-01-01' AS Date), N'TP. Hồ Chí Minh', N'', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (6, 4, 4, N'EM003', N'Hoàng', N'Văn Mạnh', N'Male', CAST(N'1900-01-01' AS Date), N'903456789', N'hoangmanh@worktracking.com', CAST(N'1900-01-01' AS Date), N'Đà Nẵng', N'', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (7, 4, 4, N'EM004', N'Nguyễn ', N'Thanh Vũ', N'Male', CAST(N'1900-01-01' AS Date), N'904567890', N'nguyenvu@worktracking.com', CAST(N'1900-01-01' AS Date), N'Tiến Bào, Phù Khê, Từ Sơn, Bắc Ninh', N'/images/employees/bb136455-27b4-417d-9307-2f3608dba191_avatar5.png', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'2025-03-25T09:04:09.667' AS DateTime), N'', N'admin')
INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (8, 4, 5, N'EM005', N'Nguyễn ', N'Thanh Trúc', N'Female', CAST(N'1900-01-01' AS Date), N'905678901', N'nguyentruc@worktracking.com', CAST(N'1900-01-01' AS Date), N'Bắc Giang', N'', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (9, 4, 5, N'EM006', N'Nguyễn ', N'Thị Hoa Phượng', N'Female', CAST(N'1900-01-01' AS Date), N'906789012', N'nguyenphuong@worktracking.com', CAST(N'1900-01-01' AS Date), N'Hà Nội', N'', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (10, 4, 5, N'EM007', N'Đào', N'Khánh Vy', N'Female', CAST(N'1900-01-01' AS Date), N'907890123', N'daovy@worktracking.com', CAST(N'1900-01-01' AS Date), N'Lào Cai', N'', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (11, 4, 5, N'EM008', N'Nguyễn ', N'Phi Long', N'Male', CAST(N'1900-01-01' AS Date), N'908901234', N'nguyenlong@worktracking.com', CAST(N'1900-01-01' AS Date), N'Lạng Sơn', N'', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (12, 1, 5, N'EM009', N'Đào', N'Duy Dũng', N'Other', CAST(N'1900-01-01' AS Date), N'909012345', N'daodung@worktracking.com', CAST(N'1900-01-01' AS Date), N'Thanh Hóa', N'', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (13, 4, 3, N'EM010', N'Nguyễn ', N'Quốc Cường', N'Other', CAST(N'1900-01-01' AS Date), N'910123456', N'nguyencuong@worktracking.com', CAST(N'1900-01-01' AS Date), N'Hải Phòng', N'/images/employees/9603c5d6-e78b-423d-a9e7-3351f9e8bcf1_avatar1.png', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'2025-03-24T09:40:24.097' AS DateTime), N'', N'admin')
INSERT [dbo].[EMPLOYEE] ([Id], [Department_Id], [Position_Id], [Code], [First_Name], [Last_Name], [Gender], [Birthday], [Phone], [Email], [Hire_Date], [Address], [Avatar], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (14, 6, 1, N'EM011', N'Bùi ', N'Huy Hoàng ', N'Male', CAST(N'1900-01-01' AS Date), N'910123456', N'buihoang@worktracking.com', CAST(N'1900-01-01' AS Date), N'Hải Phòng', N'/images/employees/bde9f5be-7d18-4b92-8a1b-671a9e6df291_avatar8.png', 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'2025-03-24T09:34:31.640' AS DateTime), N'', N'admin')
SET IDENTITY_INSERT [dbo].[EMPLOYEE] OFF
GO
SET IDENTITY_INSERT [dbo].[JOB] ON 

INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (1, 5, N'Dự thảo báo cáo kết quả lựa chọn nhà thầu cho ĐHQG-HCM', N'Dự thảo báo cáo kết quả lựa chọn nhà thầu cho ĐHQG-HCM', CAST(N'2025-05-25' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-18T18:43:57.177' AS DateTime), CAST(N'2025-03-18T18:43:57.177' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (2, 6, N'Dự thảo và trình ký CV gửi nhà thầu CW-02 vv triển khai công việc thuộc HĐ', N'Dự thảo và trình ký CV gửi nhà thầu CW-02 vv triển khai công việc thuộc HĐ', CAST(N'2025-03-07' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:16.430' AS DateTime), CAST(N'2025-03-19T09:18:16.430' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (3, 14, N'Rà soát hồ sơ thanh toán gói TV ĐL MTXH', N'Rà soát hồ sơ thanh toán gói TV ĐL MTXH', CAST(N'2025-03-07' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:16.510' AS DateTime), CAST(N'2025-03-19T09:18:16.510' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (4, 14, N'Chỉnh sửa quy trình thanh toán', N'Chỉnh sửa quy trình thanh toán', CAST(N'2025-03-14' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:16.583' AS DateTime), CAST(N'2025-03-19T09:18:16.583' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (5, 15, N'G-06: Nghiên cứu quy định về điều chỉnh giá gói thầu', N'G-06: Nghiên cứu quy định về điều chỉnh giá gói thầu', CAST(N'2025-03-30' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:16.650' AS DateTime), CAST(N'2025-03-19T09:18:16.650' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (6, 15, N'G:06 Dự thảo QĐ phê duyệt cập nhật giá gói thầu', N'G:06 Dự thảo QĐ phê duyệt cập nhật giá gói thầu', CAST(N'2025-03-07' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:16.717' AS DateTime), CAST(N'2025-03-19T09:18:16.717' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (7, 15, N'G:06 Dự thảo Tờ trình phê duyệt cập nhật giá gói thầu', N'G:06 Dự thảo Tờ trình phê duyệt cập nhật giá gói thầu', CAST(N'2025-04-12' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:16.783' AS DateTime), CAST(N'2025-03-19T09:18:16.783' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (8, 15, N'G-06: Chỉnh sửa HSMT theo ý kiến NOL có điều kiện trên STEP', N'G-06: Chỉnh sửa HSMT theo ý kiến NOL có điều kiện trên STEP', CAST(N'2025-03-02' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:16.850' AS DateTime), CAST(N'2025-03-19T09:18:16.850' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (9, 15, N'G:06: SPN', N'G:06: SPN', CAST(N'2025-03-07' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:16.920' AS DateTime), CAST(N'2025-03-19T09:18:16.920' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (10, 15, N'Cập nhật STEP bước mở thầu gói CW-01', N'Cập nhật STEP bước mở thầu gói CW-01', CAST(N'2025-03-07' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:16.990' AS DateTime), CAST(N'2025-03-19T09:18:16.990' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (11, 13, N'Triển khai tập huấn MTXH nhà thầu CW-02', N'Triển khai tập huấn MTXH nhà thầu CW-02', CAST(N'2025-03-07' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.057' AS DateTime), CAST(N'2025-03-19T09:18:17.057' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (12, 15, N'Tổng hợp quá trình triển khai proposals của gói G-08', N'Tổng hợp quá trình triển khai proposals của gói G-08', CAST(N'2025-03-13' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.120' AS DateTime), CAST(N'2025-03-19T09:18:17.120' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (13, 15, N'Cập nhật roadmap', N'Cập nhật roadmap', CAST(N'2025-03-07' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.187' AS DateTime), CAST(N'2025-03-19T09:18:17.187' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (14, 15, N'Dự thảo KHLCNT CW-03', N'Dự thảo KHLCNT CW-03', CAST(N'2025-03-07' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.257' AS DateTime), CAST(N'2025-03-19T09:18:17.257' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (15, 15, N'Dịch NOL BCĐG CW-02 cho Kho Bạc', N'Dịch NOL BCĐG CW-02 cho Kho Bạc', CAST(N'2025-03-07' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.323' AS DateTime), CAST(N'2025-03-19T09:18:17.323' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (16, 15, N'Trả lời kiến nghị của nhà thầu CW-01', N'Trả lời kiến nghị của nhà thầu CW-01', CAST(N'2025-03-08' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.390' AS DateTime), CAST(N'2025-03-19T09:18:17.390' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (17, 15, N'Cập nhật CW-03 trên STEP', N'Cập nhật CW-03 trên STEP', CAST(N'2025-03-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.460' AS DateTime), CAST(N'2025-03-19T09:18:17.460' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (18, 13, N'Rà soát C-ESMP CW-02', N'Rà soát C-ESMP CW-02', CAST(N'2025-03-10' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.527' AS DateTime), CAST(N'2025-03-19T09:18:17.527' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (19, 4, N'Lập file quản lý CV đến CV đi cần xử lý', N'Lập file quản lý CV đến CV đi cần xử lý', CAST(N'2025-03-11' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.590' AS DateTime), CAST(N'2025-03-19T09:18:17.590' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (20, 15, N'Viết lại proposals cho G-08 và G-09', N'Viết lại proposals cho G-08 và G-09', CAST(N'2025-03-12' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.657' AS DateTime), CAST(N'2025-03-19T09:18:17.657' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (21, 15, N'Nghiên cứu thiết bị nào sử dụng vốn ODA. (Đọc hiệp định và Luật)', N'Nghiên cứu thiết bị nào sử dụng vốn ODA. (Đọc hiệp định và Luật)', CAST(N'2025-03-13' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.723' AS DateTime), CAST(N'2025-03-19T09:18:17.723' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (22, 5, N'Báo cáo a Bình QLHĐ CS-01 và CS-02', N'Báo cáo a Bình QLHĐ CS-01 và CS-02', CAST(N'2025-03-14' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.797' AS DateTime), CAST(N'2025-03-19T09:18:17.797' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (23, 15, N'Rà soát roadmap đấu thầu', N'Rà soát roadmap đấu thầu', CAST(N'2025-03-15' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.863' AS DateTime), CAST(N'2025-03-19T09:18:17.863' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (24, 15, N'Soạn Quy trình để có được proposals G-08 và G-09', N'Soạn Quy trình để có được proposals G-08 và G-09', CAST(N'2025-03-16' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.930' AS DateTime), CAST(N'2025-03-19T09:18:17.930' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (25, 15, N'Cập nhật hsmt G-06 theo giá dự toán mới nhất', N'Cập nhật hsmt G-06 theo giá dự toán mới nhất', CAST(N'2025-03-17' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:17.990' AS DateTime), CAST(N'2025-03-19T09:18:17.990' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (26, 15, N'Lập ma trận giải trình góp ý CW-03', N'Lập ma trận giải trình góp ý CW-03', CAST(N'2025-03-18' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.053' AS DateTime), CAST(N'2025-03-19T09:18:18.053' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (27, 15, N'Dự thảo Tờ trình và Quyết định phê duyệt hsmt G-06', N'Dự thảo Tờ trình và Quyết định phê duyệt hsmt G-06', CAST(N'2025-03-19' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.117' AS DateTime), CAST(N'2025-03-19T09:18:18.117' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (28, 5, N'Rà soát báo cáo thẩm định hsmt G-06 của tư vấn', N'Rà soát báo cáo thẩm định hsmt G-06 của tư vấn', CAST(N'2025-03-20' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.173' AS DateTime), CAST(N'2025-03-19T09:18:18.173' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (29, 4, N'Cập nhật file IPMU Tasks', N'Cập nhật file IPMU Tasks', CAST(N'2025-03-21' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.227' AS DateTime), CAST(N'2025-03-19T09:18:18.227' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (30, 15, N'Bổ sung yêu cầu ủy quyền của nhà sản xuất vào hsmt G-06', N'Bổ sung yêu cầu ủy quyền của nhà sản xuất vào hsmt G-06', CAST(N'2025-03-22' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.277' AS DateTime), CAST(N'2025-03-19T09:18:18.277' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (31, 15, N'Soạn email gửi step hsmt CW-03', N'Soạn email gửi step hsmt CW-03', CAST(N'2025-03-23' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.333' AS DateTime), CAST(N'2025-03-19T09:18:18.333' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (32, 2, N'Họp giao ban trường CNTT và NV', N'Họp giao ban trường CNTT và NV', CAST(N'2025-03-24' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.383' AS DateTime), CAST(N'2025-03-19T09:18:18.383' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (33, 2, N'Họp giao ban trường CNTT và NV', N'Họp giao ban trường CNTT và NV', CAST(N'2025-03-25' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.440' AS DateTime), CAST(N'2025-03-19T09:18:18.440' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (34, 2, N'Họp giao ban trường CNTT và NV', N'Họp giao ban trường CNTT và NV', CAST(N'2025-03-26' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.493' AS DateTime), CAST(N'2025-03-19T09:18:18.493' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (35, 2, N'Họp giao ban trường CNTT và NV', N'Họp giao ban trường CNTT và NV', CAST(N'2025-03-27' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.550' AS DateTime), CAST(N'2025-03-19T09:18:18.550' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (36, 2, N'Họp giao ban trường KHTN và BK-KTL', N'Họp giao ban trường KHTN và BK-KTL', CAST(N'2025-03-28' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.603' AS DateTime), CAST(N'2025-03-19T09:18:18.603' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (37, 2, N'Họp giao ban trường KHTN và BK-KTL', N'Họp giao ban trường KHTN và BK-KTL', CAST(N'2025-03-29' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.657' AS DateTime), CAST(N'2025-03-19T09:18:18.657' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (38, 2, N'Họp giao ban trường KHTN và BK-KTL', N'Họp giao ban trường KHTN và BK-KTL', CAST(N'2025-03-30' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.713' AS DateTime), CAST(N'2025-03-19T09:18:18.713' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (39, 2, N'Họp giao ban trường KHTN và BK-KTL', N'Họp giao ban trường KHTN và BK-KTL', CAST(N'2025-03-31' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.767' AS DateTime), CAST(N'2025-03-19T09:18:18.767' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (40, 2, N'Phối hợp kiểm tra bàn giao 02  phòng trường NV', N'Phối hợp kiểm tra bàn giao 02  phòng trường NV', CAST(N'2025-04-01' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.823' AS DateTime), CAST(N'2025-03-19T09:18:18.823' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (41, 2, N'Phối hợp kiểm tra bàn giao zone 02 nhà E trường KHTN', N'Phối hợp kiểm tra bàn giao zone 02 nhà E trường KHTN', CAST(N'2025-04-02' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.880' AS DateTime), CAST(N'2025-03-19T09:18:18.880' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (42, 2, N'Phối hơp kiểm tra công tác bàn giao mặt bằng Zone 3 Trường KHTN', N'Phối hơp kiểm tra công tác bàn giao mặt bằng Zone 3 Trường KHTN', CAST(N'2025-04-03' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:18.937' AS DateTime), CAST(N'2025-03-19T09:18:18.937' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (43, 2, N'Phối hơp kiểm tra công tác nghiệm thu cốt thép móng, giằng móng trục 11-17/F-G TTNCTT', N'Phối hơp kiểm tra công tác nghiệm thu cốt thép móng, giằng móng trục 11-17/F-G TTNCTT', CAST(N'2025-04-04' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.007' AS DateTime), CAST(N'2025-03-19T09:18:19.007' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (44, 2, N'Phối hơp kiểm tra công tác nghiệm thu ván khuôn móng, giằng móng trục 11-17/F-G TTNCTT', N'Phối hơp kiểm tra công tác nghiệm thu ván khuôn móng, giằng móng trục 11-17/F-G TTNCTT', CAST(N'2025-04-05' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.067' AS DateTime), CAST(N'2025-03-19T09:18:19.067' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (45, 2, N'Phối hơp kiểm tra công tác thi công hiện trường và biện pháp thi công cùng Shop thép phần móng hầm TTNCTT', N'Phối hơp kiểm tra công tác thi công hiện trường và biện pháp thi công cùng Shop thép phần móng hầm TTNCTT', CAST(N'2025-04-06' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.127' AS DateTime), CAST(N'2025-03-19T09:18:19.127' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (46, 2, N'Phối hơp kiểm tra nghiệm thu bả, xả matit ngoài nhà E zone 3 Trường KHTN', N'Phối hơp kiểm tra nghiệm thu bả, xả matit ngoài nhà E zone 3 Trường KHTN', CAST(N'2025-04-07' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.190' AS DateTime), CAST(N'2025-03-19T09:18:19.190' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (47, 2, N'Phối hơp kiểm tra nghiệm thu cạo sủi cầu thang 1 Trường KHTN', N'Phối hơp kiểm tra nghiệm thu cạo sủi cầu thang 1 Trường KHTN', CAST(N'2025-04-08' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.250' AS DateTime), CAST(N'2025-03-19T09:18:19.250' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (48, 2, N'Phối hơp kiểm tra nghiệm thu cạo sủi cầu thang 2 Trường KHTN', N'Phối hơp kiểm tra nghiệm thu cạo sủi cầu thang 2 Trường KHTN', CAST(N'2025-04-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.317' AS DateTime), CAST(N'2025-03-19T09:18:19.317' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (49, 2, N'Phối hơp kiểm tra nghiệm thu cạo sủi Hành lang trục A-B/12-21 lầu 1,2 nhà A Trường KTL', N'Phối hơp kiểm tra nghiệm thu cạo sủi Hành lang trục A-B/12-21 lầu 1,2 nhà A Trường KTL', CAST(N'2025-04-10' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.367' AS DateTime), CAST(N'2025-03-19T09:18:19.367' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (50, 2, N'Phối hơp kiểm tra nghiệm thu cạo sủi Hành lang trục A-B/12-21 lầu 1,2 nhà A Trường KTL', N'Phối hơp kiểm tra nghiệm thu cạo sủi Hành lang trục A-B/12-21 lầu 1,2 nhà A Trường KTL', CAST(N'2025-04-11' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.423' AS DateTime), CAST(N'2025-03-19T09:18:19.423' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (51, 2, N'Phối hơp kiểm tra nghiệm thu cạo sủi Hành lang trục A-B/12-21 lầu 3,4 nhà A Trường KTL', N'Phối hơp kiểm tra nghiệm thu cạo sủi Hành lang trục A-B/12-21 lầu 3,4 nhà A Trường KTL', CAST(N'2025-04-12' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.470' AS DateTime), CAST(N'2025-03-19T09:18:19.470' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (52, 2, N'Phối hơp kiểm tra nghiệm thu cạo sủi Hành lang trục A-B/3-21 lầu 5,6,7 nhà A Trường KTL', N'Phối hơp kiểm tra nghiệm thu cạo sủi Hành lang trục A-B/3-21 lầu 5,6,7 nhà A Trường KTL', CAST(N'2025-04-13' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.530' AS DateTime), CAST(N'2025-03-19T09:18:19.530' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (53, 2, N'Phối hơp kiểm tra nghiệm thu cạo sủi ngoài nhà E zone 3 Trường KHTN', N'Phối hơp kiểm tra nghiệm thu cạo sủi ngoài nhà E zone 3 Trường KHTN', CAST(N'2025-04-14' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.597' AS DateTime), CAST(N'2025-03-19T09:18:19.597' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (54, 2, N'Phối hơp kiểm tra nghiệm thu cạo sủi ngoài nhà trục 1-2/B*-E nhà A Trường KTL', N'Phối hơp kiểm tra nghiệm thu cạo sủi ngoài nhà trục 1-2/B*-E nhà A Trường KTL', CAST(N'2025-04-15' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.657' AS DateTime), CAST(N'2025-03-19T09:18:19.657' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (55, 2, N'Phối hơp kiểm tra nghiệm thu cạo sủi ngoài nhà trục 22-23/B*-E nhà A Trường KTL', N'Phối hơp kiểm tra nghiệm thu cạo sủi ngoài nhà trục 22-23/B*-E nhà A Trường KTL', CAST(N'2025-04-20' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.720' AS DateTime), CAST(N'2025-03-19T09:18:19.720' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (56, 2, N'Phối hơp kiểm tra nghiệm thu cạo sủi ngoài nhà trục E/6-8&17-19 nhà A Trường KTL', N'Phối hơp kiểm tra nghiệm thu cạo sủi ngoài nhà trục E/6-8&17-19 nhà A Trường KTL', CAST(N'2025-06-13' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.790' AS DateTime), CAST(N'2025-03-19T09:18:19.790' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (57, 2, N'Phối hơp kiểm tra nghiệm thu cạo sủi ngoài nhà trục E/8-10&13-15 nhà A Trường KTL', N'Phối hơp kiểm tra nghiệm thu cạo sủi ngoài nhà trục E/8-10&13-15 nhà A Trường KTL', CAST(N'2025-04-18' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.857' AS DateTime), CAST(N'2025-03-19T09:18:19.857' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (58, 2, N'Phối hơp kiểm tra nghiệm thu cạo sủi phòng học zone3 nhà E Trường KHTN', N'Phối hơp kiểm tra nghiệm thu cạo sủi phòng học zone3 nhà E Trường KHTN', CAST(N'2025-04-19' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.923' AS DateTime), CAST(N'2025-03-19T09:18:19.923' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (59, 2, N'Phối hơp kiểm tra nghiệm thu công tác bả, xả matit trước khi sơn lót P. A32 Trường NV', N'Phối hơp kiểm tra nghiệm thu công tác bả, xả matit trước khi sơn lót P. A32 Trường NV', CAST(N'2025-04-20' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:19.983' AS DateTime), CAST(N'2025-03-19T09:18:19.983' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (60, 2, N'Phối hơp kiểm tra nghiệm thu công tác bả, xả matit trước khi sơn lót P. A33 Trường NV', N'Phối hơp kiểm tra nghiệm thu công tác bả, xả matit trước khi sơn lót P. A33 Trường NV', CAST(N'2025-04-21' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.037' AS DateTime), CAST(N'2025-03-19T09:18:20.037' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (61, 2, N'Phối hơp kiểm tra nghiệm thu công tác cạo sủi tường P. A32 Trường NV', N'Phối hơp kiểm tra nghiệm thu công tác cạo sủi tường P. A32 Trường NV', CAST(N'2025-04-22' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.093' AS DateTime), CAST(N'2025-03-19T09:18:20.093' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (62, 2, N'Phối hơp kiểm tra nghiệm thu công tác cạo sủi tường P. A33 Trường NV', N'Phối hơp kiểm tra nghiệm thu công tác cạo sủi tường P. A33 Trường NV', CAST(N'2025-04-23' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.160' AS DateTime), CAST(N'2025-03-19T09:18:20.160' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (63, 2, N'Phối hơp kiểm tra nghiệm thu công tác cạo sủi tường trong Hội trường Trường KHTN', N'Phối hơp kiểm tra nghiệm thu công tác cạo sủi tường trong Hội trường Trường KHTN', CAST(N'2025-04-02' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.223' AS DateTime), CAST(N'2025-03-19T09:18:20.223' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (64, 2, N'Phối hơp kiểm tra nghiệm thu công tác đục nền, ghém nền, lắp đặt ống âm nền P. A33 Trường NV', N'Phối hơp kiểm tra nghiệm thu công tác đục nền, ghém nền, lắp đặt ống âm nền P. A33 Trường NV', CAST(N'2025-04-25' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.280' AS DateTime), CAST(N'2025-03-19T09:18:20.280' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (65, 2, N'Phối hơp kiểm tra nghiệm thu công tác sơn lót P. A32 Trường NV', N'Phối hơp kiểm tra nghiệm thu công tác sơn lót P. A32 Trường NV', CAST(N'2025-04-26' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.343' AS DateTime), CAST(N'2025-03-19T09:18:20.343' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (66, 2, N'Phối hơp kiểm tra nghiệm thu công tác sơn lót P. A33 Trường NV', N'Phối hơp kiểm tra nghiệm thu công tác sơn lót P. A33 Trường NV', CAST(N'2025-04-27' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.407' AS DateTime), CAST(N'2025-03-19T09:18:20.407' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (67, 2, N'Phối hơp kiểm tra nghiệm thu công tác sơn nước 1 P. A32 Trường NV', N'Phối hơp kiểm tra nghiệm thu công tác sơn nước 1 P. A32 Trường NV', CAST(N'2025-04-28' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.467' AS DateTime), CAST(N'2025-03-19T09:18:20.467' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (68, 2, N'Phối hơp kiểm tra nghiệm thu công tác sơn nước 1 P. A33 Trường NV', N'Phối hơp kiểm tra nghiệm thu công tác sơn nước 1 P. A33 Trường NV', CAST(N'2025-04-29' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.530' AS DateTime), CAST(N'2025-03-19T09:18:20.530' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (69, 2, N'Phối hơp kiểm tra nghiệm thu công tác sơn nước 2 P. A32 Trường NV', N'Phối hơp kiểm tra nghiệm thu công tác sơn nước 2 P. A32 Trường NV', CAST(N'2025-04-30' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.590' AS DateTime), CAST(N'2025-03-19T09:18:20.590' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (70, 2, N'Phối hơp kiểm tra nghiệm thu công tác sơn nước 2 P. A33 Trường NV', N'Phối hơp kiểm tra nghiệm thu công tác sơn nước 2 P. A33 Trường NV', CAST(N'2025-05-01' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.643' AS DateTime), CAST(N'2025-03-19T09:18:20.643' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (71, 2, N'Phối hơp kiểm tra nghiệm thu ghém mốc và cán nền Hành lang zone 3 nhà E Trường KHTN', N'Phối hơp kiểm tra nghiệm thu ghém mốc và cán nền Hành lang zone 3 nhà E Trường KHTN', CAST(N'2025-03-06' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.700' AS DateTime), CAST(N'2025-03-19T09:18:20.700' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (72, 2, N'Phối hơp kiểm tra nghiệm thu ghém mốc và cán nền phòng 301 Trường BK', N'Phối hơp kiểm tra nghiệm thu ghém mốc và cán nền phòng 301 Trường BK', CAST(N'2025-05-03' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.763' AS DateTime), CAST(N'2025-03-19T09:18:20.763' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (73, 2, N'Phối hơp kiểm tra nghiệm thu ghém mốc và cán nền phòng học zone3 nhà E Trường KHTN', N'Phối hơp kiểm tra nghiệm thu ghém mốc và cán nền phòng học zone3 nhà E Trường KHTN', CAST(N'2025-05-04' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.823' AS DateTime), CAST(N'2025-03-19T09:18:20.823' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (74, 2, N'Phối hơp kiểm tra nghiệm thu ghém nền Trường KHTN', N'Phối hơp kiểm tra nghiệm thu ghém nền Trường KHTN', CAST(N'2025-05-05' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.887' AS DateTime), CAST(N'2025-03-19T09:18:20.887' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (75, 2, N'Phối hơp kiểm tra nghiệm thu sơn lót ngoài nhà E zone 3 Trường KHTN', N'Phối hơp kiểm tra nghiệm thu sơn lót ngoài nhà E zone 3 Trường KHTN', CAST(N'2025-05-06' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:20.943' AS DateTime), CAST(N'2025-03-19T09:18:20.943' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (76, 2, N'Phối hơp kiểm tra nghiệm thu sơn lót ngoài nhà Trường CNTT', N'Phối hơp kiểm tra nghiệm thu sơn lót ngoài nhà Trường CNTT', CAST(N'2025-05-07' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.003' AS DateTime), CAST(N'2025-03-19T09:18:21.003' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (77, 2, N'Phối hơp kiểm tra nghiệm thu sơn lót phòng 301-302 Trường BK', N'Phối hơp kiểm tra nghiệm thu sơn lót phòng 301-302 Trường BK', CAST(N'2025-05-08' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.070' AS DateTime), CAST(N'2025-03-19T09:18:21.070' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (78, 2, N'Phối hơp kiểm tra nghiệm thu sơn lót phòng học zone3 nhà E Trường KHTN', N'Phối hơp kiểm tra nghiệm thu sơn lót phòng học zone3 nhà E Trường KHTN', CAST(N'2025-05-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.123' AS DateTime), CAST(N'2025-03-19T09:18:21.123' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (79, 2, N'Phối hơp kiểm tra nghiệm thu sơn lớp 1 ngoài nhà E zone 3 Trường KHTN', N'Phối hơp kiểm tra nghiệm thu sơn lớp 1 ngoài nhà E zone 3 Trường KHTN', CAST(N'2025-05-10' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.180' AS DateTime), CAST(N'2025-03-19T09:18:21.180' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (80, 2, N'Phối hơp kiểm tra nghiệm thu sơn lớp 1 phòng 210-211 Trường BK', N'Phối hơp kiểm tra nghiệm thu sơn lớp 1 phòng 210-211 Trường BK', CAST(N'2025-03-08' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.237' AS DateTime), CAST(N'2025-03-19T09:18:21.237' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (81, 2, N'Phối hơp kiểm tra nghiệm thu sơn lớp 1 phòng 303-304 Trường BK', N'Phối hơp kiểm tra nghiệm thu sơn lớp 1 phòng 303-304 Trường BK', CAST(N'2025-05-12' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.290' AS DateTime), CAST(N'2025-03-19T09:18:21.290' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (82, 2, N'Phối hơp kiểm tra nghiệm thu sơn lớp 1 phòng học zone3 nhà E Trường KHTN', N'Phối hơp kiểm tra nghiệm thu sơn lớp 1 phòng học zone3 nhà E Trường KHTN', CAST(N'2025-03-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.350' AS DateTime), CAST(N'2025-03-19T09:18:21.350' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (83, 2, N'Phối hơp kiểm tra nghiệm thu sơn lớp 2 ngoài nhà E zone 3 Trường KHTN', N'Phối hơp kiểm tra nghiệm thu sơn lớp 2 ngoài nhà E zone 3 Trường KHTN', CAST(N'2025-03-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.407' AS DateTime), CAST(N'2025-03-19T09:18:21.407' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (84, 2, N'Phối hơp kiểm tra nghiệm thu sơn lớp 2 phòng học zone3 nhà E Trường KHTN', N'Phối hơp kiểm tra nghiệm thu sơn lớp 2 phòng học zone3 nhà E Trường KHTN', CAST(N'2025-03-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.467' AS DateTime), CAST(N'2025-03-19T09:18:21.467' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (85, 2, N'Phối hơp kiểm tra nghiệm thu trần, tấm Compart ngăn phòng, đá ốp lavabo WC tầng 5 nhà A Trường KTL', N'Phối hơp kiểm tra nghiệm thu trần, tấm Compart ngăn phòng, đá ốp lavabo WC tầng 5 nhà A Trường KTL', CAST(N'2025-03-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.520' AS DateTime), CAST(N'2025-03-19T09:18:21.520' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (86, 2, N'Phối hợp kiểm tra nhận mặt bằng 02 phòng  trường NV', N'Phối hợp kiểm tra nhận mặt bằng 02 phòng  trường NV', CAST(N'2025-03-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.587' AS DateTime), CAST(N'2025-03-19T09:18:21.587' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (87, 2, N'Phối hợp kiểm tra nhận mặt bằng Hội trường trường KHTN', N'Phối hợp kiểm tra nhận mặt bằng Hội trường trường KHTN', CAST(N'2025-03-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.657' AS DateTime), CAST(N'2025-03-19T09:18:21.657' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (88, 2, N'Phối hợp kiểm tra nhận mặt bằng zone 03 nhà E trường KHTN', N'Phối hợp kiểm tra nhận mặt bằng zone 03 nhà E trường KHTN', CAST(N'2025-03-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.720' AS DateTime), CAST(N'2025-03-19T09:18:21.720' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (89, 2, N'Phối hơp kiểm tra phê duyệt vật tư chống thấm mái nhà A Trường KTL', N'Phối hơp kiểm tra phê duyệt vật tư chống thấm mái nhà A Trường KTL', CAST(N'2025-03-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.783' AS DateTime), CAST(N'2025-03-19T09:18:21.783' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (90, 1, N'Chỉnh sửa hồ sơ và trình ký công văn gửi dự thảo HĐ gói thầu bảo hiểm công trình CW2', N'Chỉnh sửa hồ sơ và trình ký công văn gửi dự thảo HĐ gói thầu bảo hiểm công trình CW2', CAST(N'2024-12-06' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.857' AS DateTime), CAST(N'2025-03-19T09:18:21.857' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (91, 1, N'Dự thảo biên bản bàn giao cọc thí nghiệm cho nhà thầu thi công công trình trung tâm nghiên cứu Tiên tiến', N'Dự thảo biên bản bàn giao cọc thí nghiệm cho nhà thầu thi công công trình trung tâm nghiên cứu Tiên tiến', CAST(N'2024-12-26' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.920' AS DateTime), CAST(N'2025-03-19T09:18:21.920' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (92, 4, N'Dự thảo Biên bản nghiệm thu hoàn thành bàn giao gói thầu cọc thử', N'Dự thảo Biên bản nghiệm thu hoàn thành bàn giao gói thầu cọc thử', CAST(N'2024-12-11' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:21.987' AS DateTime), CAST(N'2025-03-19T09:18:21.987' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (93, 4, N'Dự thảo Biên bản nghiệm thu hoàn thành bàn giao gói thầu giám sát thi công cọc thí nghiệm', N'Dự thảo Biên bản nghiệm thu hoàn thành bàn giao gói thầu giám sát thi công cọc thí nghiệm', CAST(N'2024-12-16' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.047' AS DateTime), CAST(N'2025-03-19T09:18:22.047' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (94, 4, N'Dự thảo Biên bản thanh lý hợp đồng gói thầu cọc thử', N'Dự thảo Biên bản thanh lý hợp đồng gói thầu cọc thử', CAST(N'2024-12-12' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.110' AS DateTime), CAST(N'2025-03-19T09:18:22.110' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (95, 4, N'Dự thảo Biên bản thanh lý hợp đồng gói thầu giám sát thi công cọc thí nghiệm', N'Dự thảo Biên bản thanh lý hợp đồng gói thầu giám sát thi công cọc thí nghiệm', CAST(N'2025-03-13' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.173' AS DateTime), CAST(N'2025-03-19T09:18:22.173' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (96, 4, N'Dự thảo hồ sơ quyết toán gói thầu cọc thử', N'Dự thảo hồ sơ quyết toán gói thầu cọc thử', CAST(N'2024-12-10' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.240' AS DateTime), CAST(N'2025-03-19T09:18:22.240' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (97, 3, N'Dự thảo hơp đồng bảo hiểm CW1 Khoa y', N'Dự thảo hơp đồng bảo hiểm CW1 Khoa y', CAST(N'2024-12-19' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.307' AS DateTime), CAST(N'2025-03-19T09:18:22.307' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (98, 4, N'Dự thảo phụ lục hợp đồng gói thầu cọc thử', N'Dự thảo phụ lục hợp đồng gói thầu cọc thử', CAST(N'2024-12-11' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.367' AS DateTime), CAST(N'2025-03-19T09:18:22.367' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (99, 4, N'Dự thảo phụ lục hợp đồng gói thầu giám sát thi công cọc thí nghiệm', N'Dự thảo phụ lục hợp đồng gói thầu giám sát thi công cọc thí nghiệm', CAST(N'2024-12-16' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.423' AS DateTime), CAST(N'2025-03-19T09:18:22.423' AS DateTime), NULL, NULL)
GO
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (100, 4, N'Dự thảo phụ lục thanh toán 3a đợt cuối SCGEO gói thầu cọc thử', N'Dự thảo phụ lục thanh toán 3a đợt cuối SCGEO gói thầu cọc thử', CAST(N'2024-12-12' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.480' AS DateTime), CAST(N'2025-03-19T09:18:22.480' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (101, 4, N'Dự thảo phụ lục thanh toán 3a, gói thầu giám sát thi công cọc thí nghiệm', N'Dự thảo phụ lục thanh toán 3a, gói thầu giám sát thi công cọc thí nghiệm', CAST(N'2024-12-17' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.540' AS DateTime), CAST(N'2025-03-19T09:18:22.540' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (102, 4, N'Dự thảo phụ lục thanh toán 3a, Phụ lục 3c đợt cuối TDC gói thầu cọc thử', N'Dự thảo phụ lục thanh toán 3a, Phụ lục 3c đợt cuối TDC gói thầu cọc thử', CAST(N'2024-12-12' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.610' AS DateTime), CAST(N'2025-03-19T09:18:22.610' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (103, 4, N'Dự thảo phụ lục thanh toán 3a, Phụ lục 3c đợt cuối Vimeco gói thầu cọc thử', N'Dự thảo phụ lục thanh toán 3a, Phụ lục 3c đợt cuối Vimeco gói thầu cọc thử', CAST(N'2024-12-12' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.670' AS DateTime), CAST(N'2025-03-19T09:18:22.670' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (104, 3, N'Dự thảo Quyết định chỉ định thầu bảo hiểm CW1 Khoa y', N'Dự thảo Quyết định chỉ định thầu bảo hiểm CW1 Khoa y', CAST(N'2024-12-20' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.723' AS DateTime), CAST(N'2025-03-19T09:18:22.723' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (105, 3, N'Dự thảo Quyết định phê duyệt dự toán gói thầu bảo hiểm Cw1', N'Dự thảo Quyết định phê duyệt dự toán gói thầu bảo hiểm Cw1', CAST(N'2024-12-24' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.777' AS DateTime), CAST(N'2025-03-19T09:18:22.777' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (106, 3, N'Dự thảo tơ trình phê duyệt dự toán gói thầu bảo hiểm Cw1', N'Dự thảo tơ trình phê duyệt dự toán gói thầu bảo hiểm Cw1', CAST(N'2024-12-23' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.833' AS DateTime), CAST(N'2025-03-19T09:18:22.833' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (107, 1, N'Dự thảo và trình ký quyết định phê duyệt dự toán gói thầu bảo hiểm công trình CW2', N'Dự thảo và trình ký quyết định phê duyệt dự toán gói thầu bảo hiểm công trình CW2', CAST(N'2024-12-06' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.897' AS DateTime), CAST(N'2025-03-19T09:18:22.897' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (108, 4, N'Dự thảo và trình ký tơ trình ký phụ lục hợp đồng gói thầu cọc thí nghiệm', N'Dự thảo và trình ký tơ trình ký phụ lục hợp đồng gói thầu cọc thí nghiệm', CAST(N'2024-12-05' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:22.950' AS DateTime), CAST(N'2025-03-19T09:18:22.950' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (109, 1, N'Dự thảo và trình ký tờ trình phê duyệt dự toán gói thầu bảo hiểm công trình CW2', N'Dự thảo và trình ký tờ trình phê duyệt dự toán gói thầu bảo hiểm công trình CW2', CAST(N'2024-12-06' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.000' AS DateTime), CAST(N'2025-03-19T09:18:23.000' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (110, 1, N'Góp ý dự thảo công văn đề xuất quan tâm gói thầu bảo hiểm Cw1', N'Góp ý dự thảo công văn đề xuất quan tâm gói thầu bảo hiểm Cw1', CAST(N'2024-12-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.060' AS DateTime), CAST(N'2025-03-19T09:18:23.060' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (111, 1, N'Họp giao Ban TTNCTT', N'Họp giao Ban TTNCTT', CAST(N'2024-12-25' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.120' AS DateTime), CAST(N'2025-03-19T09:18:23.120' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (112, 1, N'Làm biên bản bàn giao mặt bằng lán trại tạm thi công với Trung tâm Quản lý đô thị', N'Làm biên bản bàn giao mặt bằng lán trại tạm thi công với Trung tâm Quản lý đô thị', CAST(N'2024-12-04' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.200' AS DateTime), CAST(N'2025-03-19T09:18:23.200' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (113, 4, N'làm đề nghị thanh toán mẫu 2a, biên bản giao nhận, scan hồ sơ, trình ký hồ sơ thanh toán gói thầu giám sát thi công cọc thí nghiệm', N'làm đề nghị thanh toán mẫu 2a, biên bản giao nhận, scan hồ sơ, trình ký hồ sơ thanh toán gói thầu giám sát thi công cọc thí nghiệm', CAST(N'2024-12-18' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.260' AS DateTime), CAST(N'2025-03-19T09:18:23.260' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (114, 4, N'Làm hồ sơ thanh toán đợt 2 gói thầu bảo hiểm các gói thầu cải tạo', N'Làm hồ sơ thanh toán đợt 2 gói thầu bảo hiểm các gói thầu cải tạo', CAST(N'2024-12-13' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.333' AS DateTime), CAST(N'2025-03-19T09:18:23.333' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (115, 1, N'Làm hồ sơ thanh toán gói thầu bảo hiểm CW2', N'Làm hồ sơ thanh toán gói thầu bảo hiểm CW2', CAST(N'2024-12-26' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.417' AS DateTime), CAST(N'2025-03-19T09:18:23.417' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (116, 4, N'Làm hồ sơ và chuyển phòng kế toán ra kho bạc thanh toán gói thầu cọc thí nghiệm ', N'Làm hồ sơ và chuyển phòng kế toán ra kho bạc thanh toán gói thầu cọc thí nghiệm ', CAST(N'2024-12-05' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.480' AS DateTime), CAST(N'2025-03-19T09:18:23.480' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (117, 1, N'Phối hợp cung cấp hồ sơ làm đấu nối cấp nước thi công cho Vinaconex', N'Phối hợp cung cấp hồ sơ làm đấu nối cấp nước thi công cho Vinaconex', CAST(N'2024-12-10' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.540' AS DateTime), CAST(N'2025-03-19T09:18:23.540' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (118, 1, N'Phối hợp điều phối xe hiện trường với Trung tâm đô thị trong lễ khởi công Trung tâm nghiên cứu Tiên tiến', N'Phối hợp điều phối xe hiện trường với Trung tâm đô thị trong lễ khởi công Trung tâm nghiên cứu Tiên tiến', CAST(N'2024-12-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.597' AS DateTime), CAST(N'2025-03-19T09:18:23.597' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (119, 2, N'Phối hợp với Dũng và nhà thầu về các mẫu biểu phụ lục thanh toán 3a, 3c, dự toán phát sinh các gói thầu thi công cải tạo', N'Phối hợp với Dũng và nhà thầu về các mẫu biểu phụ lục thanh toán 3a, 3c, dự toán phát sinh các gói thầu thi công cải tạo', CAST(N'2024-12-09' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.667' AS DateTime), CAST(N'2025-03-19T09:18:23.667' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (120, 1, N'Phối hợp xử lý về hạ tầng ngầm trên vỉa hè với tư vấn thiết kế theo nội dung công văn của trung tâm Quản lý và phát triển khu đô thị', N'Phối hợp xử lý về hạ tầng ngầm trên vỉa hè với tư vấn thiết kế theo nội dung công văn của trung tâm Quản lý và phát triển khu đô thị', CAST(N'2024-12-30' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.733' AS DateTime), CAST(N'2025-03-19T09:18:23.733' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (121, 4, N'Rà soát hồ sơ KCS, Bản vẽ hoàn công, nhật ký gói thầu cọc thử', N'Rà soát hồ sơ KCS, Bản vẽ hoàn công, nhật ký gói thầu cọc thử', CAST(N'2024-12-12' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.793' AS DateTime), CAST(N'2025-03-19T09:18:23.793' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (122, 4, N'Thống kê file gửi kiểm toán độc lập', N'Thống kê file gửi kiểm toán độc lập', CAST(N'2024-12-27' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.850' AS DateTime), CAST(N'2025-03-19T09:18:23.850' AS DateTime), NULL, NULL)
INSERT [dbo].[JOB] ([Id], [Category_Id], [Name], [Description], [Deadline1], [Deadline2], [Deadline3], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (123, 1, N'Trao đổi về ranh mốc, hạ tầng ngầm trên vỉa hè Nguyễn Du với Tư vấn thiết kế', N'Trao đổi về ranh mốc, hạ tầng ngầm trên vỉa hè Nguyễn Du với Tư vấn thiết kế', CAST(N'2024-12-25' AS Date), NULL, NULL, 0, 1, CAST(N'2025-03-19T09:18:23.903' AS DateTime), CAST(N'2025-03-19T09:18:23.903' AS DateTime), NULL, NULL)
SET IDENTITY_INSERT [dbo].[JOB] OFF
GO
SET IDENTITY_INSERT [dbo].[JOBMAPEMPLOYEE] ON 

INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (1, 5, 1, 0, 1, CAST(N'2025-03-18T18:51:40.127' AS DateTime), CAST(N'2025-03-18T18:51:40.127' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (2, 5, 2, 0, 1, CAST(N'2025-03-19T09:20:05.953' AS DateTime), CAST(N'2025-03-19T09:20:05.953' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (3, 5, 3, 0, 1, CAST(N'2025-03-19T09:20:06.013' AS DateTime), CAST(N'2025-03-19T09:20:06.013' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (4, 5, 4, 0, 1, CAST(N'2025-03-19T09:20:06.080' AS DateTime), CAST(N'2025-03-19T09:20:06.080' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (5, 5, 5, 0, 1, CAST(N'2025-03-19T09:20:06.137' AS DateTime), CAST(N'2025-03-19T09:20:06.137' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (6, 5, 6, 0, 1, CAST(N'2025-03-19T09:20:06.187' AS DateTime), CAST(N'2025-03-19T09:20:06.187' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (7, 5, 7, 0, 1, CAST(N'2025-03-19T09:20:06.253' AS DateTime), CAST(N'2025-03-19T09:20:06.253' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (8, 5, 8, 0, 1, CAST(N'2025-03-19T09:20:06.310' AS DateTime), CAST(N'2025-03-19T09:20:06.310' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (9, 5, 9, 0, 1, CAST(N'2025-03-19T09:20:06.367' AS DateTime), CAST(N'2025-03-19T09:20:06.367' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (10, 5, 10, 0, 1, CAST(N'2025-03-19T09:20:06.427' AS DateTime), CAST(N'2025-03-19T09:20:06.427' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (11, 5, 11, 0, 1, CAST(N'2025-03-19T09:20:06.483' AS DateTime), CAST(N'2025-03-19T09:20:06.483' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (12, 5, 12, 0, 1, CAST(N'2025-03-19T09:20:06.547' AS DateTime), CAST(N'2025-03-19T09:20:06.547' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (13, 5, 13, 0, 1, CAST(N'2025-03-19T09:20:06.607' AS DateTime), CAST(N'2025-03-19T09:20:06.607' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (14, 5, 14, 0, 1, CAST(N'2025-03-19T09:20:06.667' AS DateTime), CAST(N'2025-03-19T09:20:06.667' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (15, 5, 15, 0, 1, CAST(N'2025-03-19T09:20:06.730' AS DateTime), CAST(N'2025-03-19T09:20:06.730' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (16, 5, 16, 0, 1, CAST(N'2025-03-19T09:20:06.797' AS DateTime), CAST(N'2025-03-19T09:20:06.797' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (17, 5, 17, 0, 1, CAST(N'2025-03-19T09:20:06.857' AS DateTime), CAST(N'2025-03-19T09:20:06.857' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (18, 5, 18, 0, 1, CAST(N'2025-03-19T09:20:06.913' AS DateTime), CAST(N'2025-03-19T09:20:06.913' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (19, 5, 19, 0, 1, CAST(N'2025-03-19T09:20:06.970' AS DateTime), CAST(N'2025-03-19T09:20:06.970' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (20, 5, 20, 0, 1, CAST(N'2025-03-19T09:20:07.027' AS DateTime), CAST(N'2025-03-19T09:20:07.027' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (21, 5, 21, 0, 1, CAST(N'2025-03-19T09:20:07.077' AS DateTime), CAST(N'2025-03-19T09:20:07.077' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (22, 5, 22, 0, 1, CAST(N'2025-03-19T09:20:07.137' AS DateTime), CAST(N'2025-03-19T09:20:07.137' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (23, 5, 23, 0, 1, CAST(N'2025-03-19T09:20:07.193' AS DateTime), CAST(N'2025-03-19T09:20:07.193' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (24, 5, 24, 0, 1, CAST(N'2025-03-19T09:20:07.257' AS DateTime), CAST(N'2025-03-19T09:20:07.257' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (25, 5, 25, 0, 1, CAST(N'2025-03-19T09:20:07.320' AS DateTime), CAST(N'2025-03-19T09:20:07.320' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (26, 5, 26, 0, 1, CAST(N'2025-03-19T09:20:07.387' AS DateTime), CAST(N'2025-03-19T09:20:07.387' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (27, 5, 27, 0, 1, CAST(N'2025-03-19T09:20:07.457' AS DateTime), CAST(N'2025-03-19T09:20:07.457' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (28, 5, 28, 0, 1, CAST(N'2025-03-19T09:20:07.520' AS DateTime), CAST(N'2025-03-19T09:20:07.520' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (29, 5, 29, 0, 1, CAST(N'2025-03-19T09:20:07.587' AS DateTime), CAST(N'2025-03-19T09:20:07.587' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (30, 5, 30, 0, 1, CAST(N'2025-03-19T09:20:07.653' AS DateTime), CAST(N'2025-03-19T09:20:07.653' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (31, 5, 31, 0, 1, CAST(N'2025-03-19T09:20:07.717' AS DateTime), CAST(N'2025-03-19T09:20:07.717' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (32, 6, 32, 0, 1, CAST(N'2025-03-19T09:20:07.783' AS DateTime), CAST(N'2025-03-19T09:20:07.783' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (33, 6, 33, 0, 1, CAST(N'2025-03-19T09:20:07.840' AS DateTime), CAST(N'2025-03-19T09:20:07.840' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (34, 6, 34, 0, 1, CAST(N'2025-03-19T09:20:07.900' AS DateTime), CAST(N'2025-03-19T09:20:07.900' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (35, 6, 35, 0, 1, CAST(N'2025-03-19T09:20:07.957' AS DateTime), CAST(N'2025-03-19T09:20:07.957' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (36, 6, 36, 0, 1, CAST(N'2025-03-19T09:20:08.017' AS DateTime), CAST(N'2025-03-19T09:20:08.017' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (37, 6, 37, 0, 1, CAST(N'2025-03-19T09:20:08.073' AS DateTime), CAST(N'2025-03-19T09:20:08.073' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (38, 6, 38, 0, 1, CAST(N'2025-03-19T09:20:08.130' AS DateTime), CAST(N'2025-03-19T09:20:08.130' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (39, 6, 39, 0, 1, CAST(N'2025-03-19T09:20:08.197' AS DateTime), CAST(N'2025-03-19T09:20:08.197' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (40, 6, 40, 0, 1, CAST(N'2025-03-19T09:20:08.253' AS DateTime), CAST(N'2025-03-19T09:20:08.253' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (41, 6, 41, 0, 1, CAST(N'2025-03-19T09:20:08.310' AS DateTime), CAST(N'2025-03-19T09:20:08.310' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (42, 6, 42, 0, 1, CAST(N'2025-03-19T09:20:08.373' AS DateTime), CAST(N'2025-03-19T09:20:08.373' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (43, 6, 43, 0, 1, CAST(N'2025-03-19T09:20:08.430' AS DateTime), CAST(N'2025-03-19T09:20:08.430' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (44, 6, 44, 0, 1, CAST(N'2025-03-19T09:20:08.483' AS DateTime), CAST(N'2025-03-19T09:20:08.483' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (45, 6, 45, 0, 1, CAST(N'2025-03-19T09:20:08.540' AS DateTime), CAST(N'2025-03-19T09:20:08.540' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (46, 6, 46, 0, 1, CAST(N'2025-03-19T09:20:08.597' AS DateTime), CAST(N'2025-03-19T09:20:08.597' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (47, 6, 47, 0, 1, CAST(N'2025-03-19T09:20:08.650' AS DateTime), CAST(N'2025-03-19T09:20:08.650' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (48, 6, 48, 0, 1, CAST(N'2025-03-19T09:20:08.703' AS DateTime), CAST(N'2025-03-19T09:20:08.703' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (49, 6, 49, 0, 1, CAST(N'2025-03-19T09:20:08.760' AS DateTime), CAST(N'2025-03-19T09:20:08.760' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (50, 6, 50, 0, 1, CAST(N'2025-03-19T09:20:08.820' AS DateTime), CAST(N'2025-03-19T09:20:08.820' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (51, 6, 51, 0, 1, CAST(N'2025-03-19T09:20:08.880' AS DateTime), CAST(N'2025-03-19T09:20:08.880' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (52, 6, 52, 0, 1, CAST(N'2025-03-19T09:20:08.937' AS DateTime), CAST(N'2025-03-19T09:20:08.937' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (53, 6, 53, 0, 1, CAST(N'2025-03-19T09:20:08.980' AS DateTime), CAST(N'2025-03-19T09:20:08.980' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (54, 6, 54, 0, 1, CAST(N'2025-03-19T09:20:09.037' AS DateTime), CAST(N'2025-03-19T09:20:09.037' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (55, 6, 55, 0, 1, CAST(N'2025-03-19T09:20:09.093' AS DateTime), CAST(N'2025-03-19T09:20:09.093' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (56, 6, 56, 0, 1, CAST(N'2025-03-19T09:20:09.153' AS DateTime), CAST(N'2025-03-19T09:20:09.153' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (57, 6, 57, 0, 1, CAST(N'2025-03-19T09:20:09.213' AS DateTime), CAST(N'2025-03-19T09:20:09.213' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (58, 6, 58, 0, 1, CAST(N'2025-03-19T09:20:09.277' AS DateTime), CAST(N'2025-03-19T09:20:09.277' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (59, 6, 59, 0, 1, CAST(N'2025-03-19T09:20:09.333' AS DateTime), CAST(N'2025-03-19T09:20:09.333' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (60, 6, 60, 0, 1, CAST(N'2025-03-19T09:20:09.397' AS DateTime), CAST(N'2025-03-19T09:20:09.397' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (61, 6, 61, 0, 1, CAST(N'2025-03-19T09:20:09.453' AS DateTime), CAST(N'2025-03-19T09:20:09.453' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (62, 6, 62, 0, 1, CAST(N'2025-03-19T09:20:09.513' AS DateTime), CAST(N'2025-03-19T09:20:09.513' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (63, 6, 63, 0, 1, CAST(N'2025-03-19T09:20:09.573' AS DateTime), CAST(N'2025-03-19T09:20:09.573' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (64, 6, 64, 0, 1, CAST(N'2025-03-19T09:20:09.627' AS DateTime), CAST(N'2025-03-19T09:20:09.627' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (65, 6, 65, 0, 1, CAST(N'2025-03-19T09:20:09.687' AS DateTime), CAST(N'2025-03-19T09:20:09.687' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (66, 6, 66, 0, 1, CAST(N'2025-03-19T09:20:09.737' AS DateTime), CAST(N'2025-03-19T09:20:09.737' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (67, 6, 67, 0, 1, CAST(N'2025-03-19T09:20:09.797' AS DateTime), CAST(N'2025-03-19T09:20:09.797' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (68, 6, 68, 0, 1, CAST(N'2025-03-19T09:20:09.850' AS DateTime), CAST(N'2025-03-19T09:20:09.850' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (69, 6, 69, 0, 1, CAST(N'2025-03-19T09:20:09.907' AS DateTime), CAST(N'2025-03-19T09:20:09.907' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (70, 6, 70, 0, 1, CAST(N'2025-03-19T09:20:09.960' AS DateTime), CAST(N'2025-03-19T09:20:09.960' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (71, 6, 71, 0, 1, CAST(N'2025-03-19T09:20:10.013' AS DateTime), CAST(N'2025-03-19T09:20:10.013' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (72, 6, 72, 0, 1, CAST(N'2025-03-19T09:20:10.070' AS DateTime), CAST(N'2025-03-19T09:20:10.070' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (73, 6, 73, 0, 1, CAST(N'2025-03-19T09:20:10.130' AS DateTime), CAST(N'2025-03-19T09:20:10.130' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (74, 6, 74, 0, 1, CAST(N'2025-03-19T09:20:10.187' AS DateTime), CAST(N'2025-03-19T09:20:10.187' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (75, 6, 75, 0, 1, CAST(N'2025-03-19T09:20:10.240' AS DateTime), CAST(N'2025-03-19T09:20:10.240' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (76, 6, 76, 0, 1, CAST(N'2025-03-19T09:20:10.293' AS DateTime), CAST(N'2025-03-19T09:20:10.293' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (77, 6, 77, 0, 1, CAST(N'2025-03-19T09:20:10.350' AS DateTime), CAST(N'2025-03-19T09:20:10.350' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (78, 6, 78, 0, 1, CAST(N'2025-03-19T09:20:10.410' AS DateTime), CAST(N'2025-03-19T09:20:10.410' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (79, 6, 79, 0, 1, CAST(N'2025-03-19T09:20:10.467' AS DateTime), CAST(N'2025-03-19T09:20:10.467' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (80, 6, 80, 0, 1, CAST(N'2025-03-19T09:20:10.527' AS DateTime), CAST(N'2025-03-19T09:20:10.527' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (81, 6, 81, 0, 1, CAST(N'2025-03-19T09:20:10.587' AS DateTime), CAST(N'2025-03-19T09:20:10.587' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (82, 6, 82, 0, 1, CAST(N'2025-03-19T09:20:10.650' AS DateTime), CAST(N'2025-03-19T09:20:10.650' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (83, 6, 83, 0, 1, CAST(N'2025-03-19T09:20:10.703' AS DateTime), CAST(N'2025-03-19T09:20:10.703' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (84, 6, 84, 0, 1, CAST(N'2025-03-19T09:20:10.760' AS DateTime), CAST(N'2025-03-19T09:20:10.760' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (85, 6, 85, 0, 1, CAST(N'2025-03-19T09:20:10.813' AS DateTime), CAST(N'2025-03-19T09:20:10.813' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (86, 6, 86, 0, 1, CAST(N'2025-03-19T09:20:10.877' AS DateTime), CAST(N'2025-03-19T09:20:10.877' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (87, 6, 87, 0, 1, CAST(N'2025-03-19T09:20:10.937' AS DateTime), CAST(N'2025-03-19T09:20:10.937' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (88, 6, 88, 0, 1, CAST(N'2025-03-19T09:20:10.993' AS DateTime), CAST(N'2025-03-19T09:20:10.993' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (89, 6, 89, 0, 1, CAST(N'2025-03-19T09:20:11.057' AS DateTime), CAST(N'2025-03-19T09:20:11.057' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (90, 4, 90, 0, 1, CAST(N'2025-03-19T09:20:11.113' AS DateTime), CAST(N'2025-03-19T09:20:11.113' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (91, 4, 91, 0, 1, CAST(N'2025-03-19T09:20:11.170' AS DateTime), CAST(N'2025-03-19T09:20:11.170' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (92, 4, 92, 0, 1, CAST(N'2025-03-19T09:20:11.230' AS DateTime), CAST(N'2025-03-19T09:20:11.230' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (93, 4, 93, 0, 1, CAST(N'2025-03-19T09:20:11.287' AS DateTime), CAST(N'2025-03-19T09:20:11.287' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (94, 4, 94, 0, 1, CAST(N'2025-03-19T09:20:11.343' AS DateTime), CAST(N'2025-03-19T09:20:11.343' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (95, 4, 95, 0, 1, CAST(N'2025-03-19T09:20:11.400' AS DateTime), CAST(N'2025-03-19T09:20:11.400' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (96, 4, 96, 0, 1, CAST(N'2025-03-19T09:20:11.460' AS DateTime), CAST(N'2025-03-19T09:20:11.460' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (97, 4, 97, 0, 1, CAST(N'2025-03-19T09:20:11.523' AS DateTime), CAST(N'2025-03-19T09:20:11.523' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (98, 4, 98, 0, 1, CAST(N'2025-03-19T09:20:11.580' AS DateTime), CAST(N'2025-03-19T09:20:11.580' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (99, 4, 99, 0, 1, CAST(N'2025-03-19T09:20:11.643' AS DateTime), CAST(N'2025-03-19T09:20:11.643' AS DateTime), NULL, NULL)
GO
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (100, 4, 100, 0, 1, CAST(N'2025-03-19T09:20:11.700' AS DateTime), CAST(N'2025-03-19T09:20:11.700' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (101, 4, 101, 0, 1, CAST(N'2025-03-19T09:20:11.757' AS DateTime), CAST(N'2025-03-19T09:20:11.757' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (102, 4, 102, 0, 1, CAST(N'2025-03-19T09:20:11.817' AS DateTime), CAST(N'2025-03-19T09:20:11.817' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (103, 4, 103, 0, 1, CAST(N'2025-03-19T09:20:11.873' AS DateTime), CAST(N'2025-03-19T09:20:11.873' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (104, 4, 104, 0, 1, CAST(N'2025-03-19T09:20:11.930' AS DateTime), CAST(N'2025-03-19T09:20:11.930' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (105, 4, 105, 0, 1, CAST(N'2025-03-19T09:20:11.983' AS DateTime), CAST(N'2025-03-19T09:20:11.983' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (106, 4, 106, 0, 1, CAST(N'2025-03-19T09:20:12.040' AS DateTime), CAST(N'2025-03-19T09:20:12.040' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (107, 4, 107, 0, 1, CAST(N'2025-03-19T09:20:12.100' AS DateTime), CAST(N'2025-03-19T09:20:12.100' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (108, 4, 108, 0, 1, CAST(N'2025-03-19T09:20:12.153' AS DateTime), CAST(N'2025-03-19T09:20:12.153' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (109, 4, 109, 0, 1, CAST(N'2025-03-19T09:20:12.213' AS DateTime), CAST(N'2025-03-19T09:20:12.213' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (110, 4, 110, 0, 1, CAST(N'2025-03-19T09:20:12.273' AS DateTime), CAST(N'2025-03-19T09:20:12.273' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (111, 4, 111, 0, 1, CAST(N'2025-03-19T09:20:12.333' AS DateTime), CAST(N'2025-03-19T09:20:12.333' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (112, 4, 112, 0, 1, CAST(N'2025-03-19T09:20:12.390' AS DateTime), CAST(N'2025-03-19T09:20:12.390' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (113, 4, 113, 0, 1, CAST(N'2025-03-19T09:20:12.450' AS DateTime), CAST(N'2025-03-19T09:20:12.450' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (114, 4, 114, 0, 1, CAST(N'2025-03-19T09:20:12.507' AS DateTime), CAST(N'2025-03-19T09:20:12.507' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (115, 4, 115, 0, 1, CAST(N'2025-03-19T09:20:12.563' AS DateTime), CAST(N'2025-03-19T09:20:12.563' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (116, 4, 116, 0, 1, CAST(N'2025-03-19T09:20:12.623' AS DateTime), CAST(N'2025-03-19T09:20:12.623' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (117, 4, 117, 0, 1, CAST(N'2025-03-19T09:20:12.683' AS DateTime), CAST(N'2025-03-19T09:20:12.683' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (118, 4, 118, 0, 1, CAST(N'2025-03-19T09:20:12.743' AS DateTime), CAST(N'2025-03-19T09:20:12.743' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (119, 4, 119, 0, 1, CAST(N'2025-03-19T09:20:12.803' AS DateTime), CAST(N'2025-03-19T09:20:12.803' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (120, 4, 120, 0, 1, CAST(N'2025-03-19T09:20:12.860' AS DateTime), CAST(N'2025-03-19T09:20:12.860' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (121, 4, 121, 0, 1, CAST(N'2025-03-19T09:20:12.913' AS DateTime), CAST(N'2025-03-19T09:20:12.913' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (122, 4, 122, 0, 1, CAST(N'2025-03-19T09:20:12.970' AS DateTime), CAST(N'2025-03-19T09:20:12.970' AS DateTime), NULL, NULL)
INSERT [dbo].[JOBMAPEMPLOYEE] ([Id], [Employee_Id], [Job_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (123, 4, 123, 0, 1, CAST(N'2025-03-19T09:20:13.030' AS DateTime), CAST(N'2025-03-19T09:20:13.030' AS DateTime), NULL, NULL)
SET IDENTITY_INSERT [dbo].[JOBMAPEMPLOYEE] OFF
GO
SET IDENTITY_INSERT [dbo].[POSITION] ON 

INSERT [dbo].[POSITION] ([Id], [Name], [Description], [Status], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (1, N'Admin', N'Quản lý toàn hệ thống', 1, 0, 1, CAST(N'2025-02-20T10:10:05.887' AS DateTime), CAST(N'2025-02-20T10:10:05.887' AS DateTime), NULL, NULL)
INSERT [dbo].[POSITION] ([Id], [Name], [Description], [Status], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (2, N'Giám đốc	', N'Người điều hành công ty', 1, 0, 1, CAST(N'2025-02-20T10:10:05.887' AS DateTime), CAST(N'2025-02-20T10:10:05.887' AS DateTime), NULL, NULL)
INSERT [dbo].[POSITION] ([Id], [Name], [Description], [Status], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (3, N'Quản lý', N'Quản lý phòng ban', 0, 0, 1, CAST(N'2025-02-20T10:10:05.887' AS DateTime), CAST(N'2025-02-20T10:10:05.887' AS DateTime), NULL, NULL)
INSERT [dbo].[POSITION] ([Id], [Name], [Description], [Status], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (4, N'Nhân viên', N'Người dùng', 0, 0, 1, CAST(N'2025-02-28T20:31:50.733' AS DateTime), CAST(N'2025-02-28T20:31:50.733' AS DateTime), NULL, NULL)
INSERT [dbo].[POSITION] ([Id], [Name], [Description], [Status], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (5, N'Quản lý nhân sự', N'Quản lý nhân sự', 0, 0, 1, CAST(N'2025-03-02T14:28:10.930' AS DateTime), CAST(N'2025-03-02T14:28:10.930' AS DateTime), NULL, NULL)
SET IDENTITY_INSERT [dbo].[POSITION] OFF
GO
SET IDENTITY_INSERT [dbo].[SCORE] ON 

INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (1, 1, NULL, 4, 1, 0, 3, 1.35, 50, CAST(N'2025-03-18T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-18T18:54:04.793' AS DateTime), CAST(N'2025-03-18T18:54:04.793' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (2, 2, CAST(N'2025-03-19' AS Date), 3, 1, 3, 3, 1.7999999999999998, 100, CAST(N'2025-03-12T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.083' AS DateTime), CAST(N'2025-03-19T09:23:52.083' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (3, 3, NULL, 2, 0, 0, 0, 0, 75, CAST(N'2025-03-13T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.150' AS DateTime), CAST(N'2025-03-19T09:23:52.150' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (4, 4, NULL, 0, 1, 3, 3, 1.7999999999999998, 0, CAST(N'2025-03-14T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.203' AS DateTime), CAST(N'2025-03-19T09:23:52.203' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (5, 5, NULL, 0, 0.5, 3, 3, 1.5, 0, CAST(N'2025-03-15T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.260' AS DateTime), CAST(N'2025-03-19T09:23:52.260' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (6, 6, NULL, 2, 1, 3, 3, 1.7999999999999998, 35, CAST(N'2025-03-16T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.320' AS DateTime), CAST(N'2025-03-19T09:23:52.320' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (7, 7, NULL, 0, 1, 3, 3, 1.7999999999999998, 0, CAST(N'2025-03-17T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.377' AS DateTime), CAST(N'2025-03-19T09:23:52.377' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (8, 8, NULL, 2, 1, 3, 3, 1.7999999999999998, 80, CAST(N'2025-03-18T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.443' AS DateTime), CAST(N'2025-03-19T09:23:52.443' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (9, 9, NULL, 2, 0, 0, 0, 0, 75, CAST(N'2025-03-19T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.500' AS DateTime), CAST(N'2025-03-19T09:23:52.500' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (10, 10, CAST(N'2025-03-19' AS Date), 3, 0.5, 2, 2, 1.1, 100, CAST(N'2025-03-20T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.557' AS DateTime), CAST(N'2025-03-19T09:23:52.557' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (11, 11, NULL, 2, 2, 3, 3, 2.4, 95, CAST(N'2025-03-21T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.613' AS DateTime), CAST(N'2025-03-19T09:23:52.613' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (12, 12, NULL, 0, 1, 3, 3, 1.7999999999999998, 0, CAST(N'2025-03-22T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.667' AS DateTime), CAST(N'2025-03-19T09:23:52.667' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (13, 13, CAST(N'2025-03-19' AS Date), 3, 1, 3, 3, 1.7999999999999998, 100, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.723' AS DateTime), CAST(N'2025-03-19T09:23:52.723' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (14, 14, CAST(N'2025-03-19' AS Date), 3, 1, 3, 3, 1.7999999999999998, 100, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.783' AS DateTime), CAST(N'2025-03-19T09:23:52.783' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (15, 15, NULL, 0, 0.5, 2, 2, 1.1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.847' AS DateTime), CAST(N'2025-03-19T09:23:52.847' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (16, 16, NULL, 0, 1, 3, 3, 1.7999999999999998, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.900' AS DateTime), CAST(N'2025-03-19T09:23:52.900' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (17, 17, NULL, 0, 1, 3, 2, 1.5499999999999998, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:52.960' AS DateTime), CAST(N'2025-03-19T09:23:52.960' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (18, 18, NULL, 0, 3, 3, 3, 3, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.023' AS DateTime), CAST(N'2025-03-19T09:23:53.023' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (19, 19, NULL, 0, 1, 3, 2, 1.5499999999999998, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.080' AS DateTime), CAST(N'2025-03-19T09:23:53.080' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (20, 20, NULL, 0, 3, 3, 3, 3, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.147' AS DateTime), CAST(N'2025-03-19T09:23:53.147' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (21, 21, NULL, 0, 0, 0, 0, 0, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.203' AS DateTime), CAST(N'2025-03-19T09:23:53.203' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (22, 22, NULL, 0, 0, 0, 0, 0, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.257' AS DateTime), CAST(N'2025-03-19T09:23:53.257' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (23, 23, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.313' AS DateTime), CAST(N'2025-03-19T09:23:53.313' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (24, 24, NULL, 0, 0, 0, 0, 0, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.367' AS DateTime), CAST(N'2025-03-19T09:23:53.367' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (25, 25, NULL, 0, 1, 2, 2, 1.4, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.427' AS DateTime), CAST(N'2025-03-19T09:23:53.427' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (26, 26, NULL, 0, 1, 3, 3, 1.7999999999999998, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.480' AS DateTime), CAST(N'2025-03-19T09:23:53.480' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (27, 27, NULL, 0, 2, 3, 3, 2.4, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.537' AS DateTime), CAST(N'2025-03-19T09:23:53.537' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (28, 28, NULL, 0, 0.5, 2, 2, 1.1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.593' AS DateTime), CAST(N'2025-03-19T09:23:53.593' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (29, 29, NULL, 0, 1, 3, 3, 1.7999999999999998, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.647' AS DateTime), CAST(N'2025-03-19T09:23:53.647' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (30, 30, NULL, 0, 1, 3, 3, 1.7999999999999998, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.707' AS DateTime), CAST(N'2025-03-19T09:23:53.707' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (31, 31, NULL, 0, 1, 3, 3, 1.7999999999999998, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.763' AS DateTime), CAST(N'2025-03-19T09:23:53.763' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (32, 32, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.823' AS DateTime), CAST(N'2025-03-19T09:23:53.823' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (33, 33, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.880' AS DateTime), CAST(N'2025-03-19T09:23:53.880' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (34, 34, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.940' AS DateTime), CAST(N'2025-03-19T09:23:53.940' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (35, 35, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:53.997' AS DateTime), CAST(N'2025-03-19T09:23:53.997' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (36, 36, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.057' AS DateTime), CAST(N'2025-03-19T09:23:54.057' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (37, 37, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.113' AS DateTime), CAST(N'2025-03-19T09:23:54.113' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (38, 38, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.167' AS DateTime), CAST(N'2025-03-19T09:23:54.167' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (39, 39, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.223' AS DateTime), CAST(N'2025-03-19T09:23:54.223' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (40, 40, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.273' AS DateTime), CAST(N'2025-03-19T09:23:54.273' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (41, 41, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.333' AS DateTime), CAST(N'2025-03-19T09:23:54.333' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (42, 42, NULL, 0, 1, 3, 3, 1.7999999999999998, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.390' AS DateTime), CAST(N'2025-03-19T09:23:54.390' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (43, 43, NULL, 0, 1, 3, 3, 1.7999999999999998, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.450' AS DateTime), CAST(N'2025-03-19T09:23:54.450' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (44, 44, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.510' AS DateTime), CAST(N'2025-03-19T09:23:54.510' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (45, 45, NULL, 0, 1, 3, 3, 1.7999999999999998, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.570' AS DateTime), CAST(N'2025-03-19T09:23:54.570' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (46, 46, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.633' AS DateTime), CAST(N'2025-03-19T09:23:54.633' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (47, 47, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.690' AS DateTime), CAST(N'2025-03-19T09:23:54.690' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (48, 48, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.750' AS DateTime), CAST(N'2025-03-19T09:23:54.750' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (49, 49, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.810' AS DateTime), CAST(N'2025-03-19T09:23:54.810' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (50, 50, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.870' AS DateTime), CAST(N'2025-03-19T09:23:54.870' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (51, 51, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.927' AS DateTime), CAST(N'2025-03-19T09:23:54.927' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (52, 52, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:54.987' AS DateTime), CAST(N'2025-03-19T09:23:54.987' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (53, 53, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.043' AS DateTime), CAST(N'2025-03-19T09:23:55.043' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (54, 54, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.100' AS DateTime), CAST(N'2025-03-19T09:23:55.100' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (55, 55, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.157' AS DateTime), CAST(N'2025-03-19T09:23:55.157' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (56, 56, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.213' AS DateTime), CAST(N'2025-03-19T09:23:55.213' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (57, 57, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.270' AS DateTime), CAST(N'2025-03-19T09:23:55.270' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (58, 58, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.330' AS DateTime), CAST(N'2025-03-19T09:23:55.330' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (59, 59, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.390' AS DateTime), CAST(N'2025-03-19T09:23:55.390' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (60, 60, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.450' AS DateTime), CAST(N'2025-03-19T09:23:55.450' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (61, 61, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.500' AS DateTime), CAST(N'2025-03-19T09:23:55.500' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (62, 62, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.560' AS DateTime), CAST(N'2025-03-19T09:23:55.560' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (63, 63, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.620' AS DateTime), CAST(N'2025-03-19T09:23:55.620' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (64, 64, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.680' AS DateTime), CAST(N'2025-03-19T09:23:55.680' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (65, 65, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.733' AS DateTime), CAST(N'2025-03-19T09:23:55.733' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (66, 66, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.787' AS DateTime), CAST(N'2025-03-19T09:23:55.787' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (67, 67, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.843' AS DateTime), CAST(N'2025-03-19T09:23:55.843' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (68, 68, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.897' AS DateTime), CAST(N'2025-03-19T09:23:55.897' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (69, 69, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:55.953' AS DateTime), CAST(N'2025-03-19T09:23:55.953' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (70, 70, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.010' AS DateTime), CAST(N'2025-03-19T09:23:56.010' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (71, 71, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.067' AS DateTime), CAST(N'2025-03-19T09:23:56.067' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (72, 72, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.127' AS DateTime), CAST(N'2025-03-19T09:23:56.127' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (73, 73, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.183' AS DateTime), CAST(N'2025-03-19T09:23:56.183' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (74, 74, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.243' AS DateTime), CAST(N'2025-03-19T09:23:56.243' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (75, 75, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.310' AS DateTime), CAST(N'2025-03-19T09:23:56.310' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (76, 76, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.380' AS DateTime), CAST(N'2025-03-19T09:23:56.380' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (77, 77, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.450' AS DateTime), CAST(N'2025-03-19T09:23:56.450' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (78, 78, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.503' AS DateTime), CAST(N'2025-03-19T09:23:56.503' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (79, 79, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.553' AS DateTime), CAST(N'2025-03-19T09:23:56.553' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (80, 80, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.617' AS DateTime), CAST(N'2025-03-19T09:23:56.617' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (81, 81, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.673' AS DateTime), CAST(N'2025-03-19T09:23:56.673' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (82, 82, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.730' AS DateTime), CAST(N'2025-03-19T09:23:56.730' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (83, 83, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.787' AS DateTime), CAST(N'2025-03-19T09:23:56.787' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (84, 84, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.840' AS DateTime), CAST(N'2025-03-19T09:23:56.840' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (85, 85, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.910' AS DateTime), CAST(N'2025-03-19T09:23:56.910' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (86, 86, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:56.963' AS DateTime), CAST(N'2025-03-19T09:23:56.963' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (87, 87, NULL, 0, 0.5, 1, 1, 0.7, 0, CAST(N'2024-11-29T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.017' AS DateTime), CAST(N'2025-03-19T09:23:57.017' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (88, 88, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2024-12-19T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.077' AS DateTime), CAST(N'2025-03-19T09:23:57.077' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (89, 89, NULL, 0, 1, 1, 1, 1, 0, CAST(N'2024-12-04T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.130' AS DateTime), CAST(N'2025-03-19T09:23:57.130' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (90, 90, CAST(N'2025-03-24' AS Date), 3, 1, 2, 3, 1.65, 100, CAST(N'2024-12-09T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.190' AS DateTime), CAST(N'2025-03-19T09:23:57.190' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (91, 91, NULL, 2, 0, 0, 0, 0, 45, CAST(N'2024-12-05T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.247' AS DateTime), CAST(N'2025-03-19T09:23:57.247' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (92, 92, CAST(N'2024-12-11' AS Date), 3, 1, 2, 2, 1.4, 100, CAST(N'2025-03-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.303' AS DateTime), CAST(N'2025-03-19T09:23:57.303' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (93, 93, CAST(N'2024-12-16' AS Date), 3, 1, 1, 1, 1, 100, CAST(N'2024-12-03T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.360' AS DateTime), CAST(N'2025-03-19T09:23:57.360' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (94, 94, CAST(N'2024-12-12' AS Date), 3, 1, 1, 1, 1, 100, CAST(N'2024-12-12T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.420' AS DateTime), CAST(N'2025-03-19T09:23:57.420' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (95, 95, NULL, 2, 0, 0, 0, 0, 50, CAST(N'2024-12-04T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.480' AS DateTime), CAST(N'2025-03-19T09:23:57.480' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (96, 96, CAST(N'2024-12-10' AS Date), 3, 0, 0, 0, 0, 100, CAST(N'2024-12-09T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.550' AS DateTime), CAST(N'2025-03-19T09:23:57.550' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (97, 97, CAST(N'2024-12-19' AS Date), 3, 3, 3, 3, 3, 100, CAST(N'2024-12-05T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.607' AS DateTime), CAST(N'2025-03-19T09:23:57.607' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (98, 98, CAST(N'2024-12-11' AS Date), 3, 0, 0, 0, 0, 100, CAST(N'2024-12-10T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.667' AS DateTime), CAST(N'2025-03-19T09:23:57.667' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (99, 99, CAST(N'2024-12-16' AS Date), 3, 1, 1, 1, 1, 100, CAST(N'2024-12-05T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.727' AS DateTime), CAST(N'2025-03-19T09:23:57.727' AS DateTime), NULL, NULL)
GO
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (100, 100, CAST(N'2024-12-12' AS Date), 3, 0.5, 1, 1, 0.7, 100, CAST(N'2024-12-05T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.783' AS DateTime), CAST(N'2025-03-19T09:23:57.783' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (101, 101, CAST(N'2024-12-17' AS Date), 3, 0, 0, 0, 0, 100, CAST(N'2024-12-13T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.840' AS DateTime), CAST(N'2025-03-19T09:23:57.840' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (102, 102, CAST(N'2024-12-12' AS Date), 3, 0.5, 1, 1, 0.7, 100, CAST(N'2024-12-17T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.897' AS DateTime), CAST(N'2025-03-19T09:23:57.897' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (103, 103, CAST(N'2024-12-12' AS Date), 3, 0.5, 1, 1, 0.7, 100, CAST(N'2024-12-16T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:57.950' AS DateTime), CAST(N'2025-03-19T09:23:57.950' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (104, 104, CAST(N'2024-12-20' AS Date), 3, 3, 3, 3, 3, 100, CAST(N'2024-11-29T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.010' AS DateTime), CAST(N'2025-03-19T09:23:58.010' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (105, 105, CAST(N'2024-12-24' AS Date), 3, 0, 0, 0, 0, 100, CAST(N'2024-11-28T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.067' AS DateTime), CAST(N'2025-03-19T09:23:58.067' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (106, 106, CAST(N'2024-12-23' AS Date), 3, 0, 0, 0, 0, 100, CAST(N'2024-11-29T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.127' AS DateTime), CAST(N'2025-03-19T09:23:58.127' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (107, 107, NULL, 2, 0, 0, 0, 0, 85, CAST(N'2024-12-02T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.190' AS DateTime), CAST(N'2025-03-19T09:23:58.190' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (108, 108, NULL, 2, 2, 1, 2, 1.8499999999999999, 40, CAST(N'2024-12-18T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.253' AS DateTime), CAST(N'2025-03-19T09:23:58.253' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (109, 109, NULL, 2, 3, 2, 3, 2.8499999999999996, 40, CAST(N'2024-11-27T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.313' AS DateTime), CAST(N'2025-03-19T09:23:58.313' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (110, 110, CAST(N'2024-12-09' AS Date), 3, 1, 2, 2, 1.4, 100, CAST(N'2024-12-11T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.367' AS DateTime), CAST(N'2025-03-19T09:23:58.367' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (111, 111, CAST(N'2024-12-25' AS Date), 3, 0, 0, 0, 0, 100, CAST(N'2024-12-06T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.433' AS DateTime), CAST(N'2025-03-19T09:23:58.433' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (112, 112, CAST(N'2025-03-24' AS Date), 3, 1, 1, 1, 1, 100, CAST(N'2024-12-19T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.493' AS DateTime), CAST(N'2025-03-19T09:23:58.493' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (113, 113, CAST(N'2024-12-18' AS Date), 3, 0.5, 1, 1, 0.7, 100, CAST(N'2024-11-28T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.557' AS DateTime), CAST(N'2025-03-19T09:23:58.557' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (114, 114, CAST(N'2024-12-13' AS Date), 3, 1, 3, 3, 1.7999999999999998, 100, CAST(N'2024-12-03T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.613' AS DateTime), CAST(N'2025-03-19T09:23:58.613' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (115, 115, CAST(N'2024-12-26' AS Date), 3, 1, 3, 3, 1.7999999999999998, 100, CAST(N'2024-12-02T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.680' AS DateTime), CAST(N'2025-03-19T09:23:58.680' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (116, 116, NULL, 2, 1, 1, 3, 1.5, 50, CAST(N'2024-12-02T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.743' AS DateTime), CAST(N'2025-03-19T09:23:58.743' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (117, 117, CAST(N'2024-12-10' AS Date), 3, 1, 2, 2, 1.4, 100, CAST(N'2024-12-23T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.800' AS DateTime), CAST(N'2025-03-19T09:23:58.800' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (118, 118, CAST(N'2024-12-09' AS Date), 3, 1, 1, 1, 1, 100, CAST(N'2024-12-05T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.857' AS DateTime), CAST(N'2025-03-19T09:23:58.857' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (119, 119, CAST(N'2024-12-09' AS Date), 3, 1, 1, 1, 1, 100, CAST(N'2024-12-20T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.917' AS DateTime), CAST(N'2025-03-19T09:23:58.917' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (120, 120, NULL, 2, 1, 2, 1, 1.15, 50, CAST(N'2024-12-18T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:58.970' AS DateTime), CAST(N'2025-03-19T09:23:58.970' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (121, 121, CAST(N'2024-12-12' AS Date), 3, 1, 1, 1, 1, 100, CAST(N'2024-12-18T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:59.033' AS DateTime), CAST(N'2025-03-19T09:23:59.033' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (122, 122, CAST(N'2024-12-27' AS Date), 3, 1, 3, 3, 1.7999999999999998, 100, CAST(N'2024-12-18T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:59.093' AS DateTime), CAST(N'2025-03-19T09:23:59.093' AS DateTime), NULL, NULL)
INSERT [dbo].[SCORE] ([Id], [JobMapEmployee_Id], [CompletionDate], [Status], [VolumeAssessment], [ProgressAssessment], [QualityAssessment], [SummaryOfReviews], [Progress], [Time], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (123, 123, CAST(N'2024-12-25' AS Date), 3, 0, 0, 0, 0, 100, CAST(N'2024-12-18T00:00:00.000' AS DateTime), 0, 1, CAST(N'2025-03-19T09:23:59.150' AS DateTime), CAST(N'2025-03-19T09:23:59.150' AS DateTime), NULL, NULL)
SET IDENTITY_INSERT [dbo].[SCORE] OFF
GO
SET IDENTITY_INSERT [dbo].[SYSTEMSW] ON 

INSERT [dbo].[SYSTEMSW] ([Id], [Name], [Value], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (1, N'MaxLoginAttempts', N'5', N'Số lần đăng nhập sai tối đa', 0, 1, CAST(N'2025-02-20T10:10:05.903' AS DateTime), CAST(N'2025-02-20T10:10:05.903' AS DateTime), NULL, NULL)
INSERT [dbo].[SYSTEMSW] ([Id], [Name], [Value], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (2, N'SessionTimeout', N'30', N'Thời gian hết hạn phiên làm việc', 0, 1, CAST(N'2025-02-20T10:10:05.903' AS DateTime), CAST(N'2025-02-20T10:10:05.903' AS DateTime), NULL, NULL)
INSERT [dbo].[SYSTEMSW] ([Id], [Name], [Value], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (3, N'AllowGuestAccess', N'1', N'Cho phép khách truy cập', 0, 1, CAST(N'2025-02-20T10:10:05.903' AS DateTime), CAST(N'2025-02-20T10:10:05.903' AS DateTime), NULL, NULL)
INSERT [dbo].[SYSTEMSW] ([Id], [Name], [Value], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (4, N'AdminSystem', N'1', N'Đăng nhập area admin', 0, 1, CAST(N'2025-03-02T14:20:14.350' AS DateTime), CAST(N'2025-03-02T14:20:14.350' AS DateTime), NULL, NULL)
INSERT [dbo].[SYSTEMSW] ([Id], [Name], [Value], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (5, N'EmployeeSystem', N'4', N'Đăng nhập area EmployeeSystem ', 0, 1, CAST(N'2025-03-02T14:20:25.553' AS DateTime), CAST(N'2025-03-02T14:20:25.553' AS DateTime), NULL, NULL)
INSERT [dbo].[SYSTEMSW] ([Id], [Name], [Value], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (6, N'HRManager', N'5', N'Đăng nhập area HRManager', 0, 1, CAST(N'2025-03-02T14:20:42.680' AS DateTime), CAST(N'2025-03-02T14:20:42.680' AS DateTime), NULL, NULL)
INSERT [dbo].[SYSTEMSW] ([Id], [Name], [Value], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (7, N'ProjectManager', N'3', N'Đăng nhập area ProjectManager', 0, 1, CAST(N'2025-03-02T14:20:43.697' AS DateTime), CAST(N'2025-03-02T14:20:43.697' AS DateTime), NULL, NULL)
INSERT [dbo].[SYSTEMSW] ([Id], [Name], [Value], [Description], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (8, N'HRDepartment', N'1', N'Đăng nhập area HRManager', 0, 1, CAST(N'2025-03-02T15:01:01.783' AS DateTime), CAST(N'2025-03-02T15:01:01.783' AS DateTime), NULL, NULL)
SET IDENTITY_INSERT [dbo].[SYSTEMSW] OFF
GO
SET IDENTITY_INSERT [dbo].[USERS] ON 

INSERT [dbo].[USERS] ([Id], [UserName], [Password], [Employee_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (1, N'admin', N'74bbe0e185d65998d4fecc8517dd98d0bed47036b603779d470c0eaafb04c196', 1, 0, 1, CAST(N'2025-02-20T10:10:05.907' AS DateTime), CAST(N'2025-03-02T15:15:05.547' AS DateTime), NULL, N'1')
INSERT [dbo].[USERS] ([Id], [UserName], [Password], [Employee_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (2, N'manageProject', N'74bbe0e185d65998d4fecc8517dd98d0bed47036b603779d470c0eaafb04c196', 2, 0, 1, CAST(N'2025-02-20T10:10:05.907' AS DateTime), CAST(N'2025-02-20T10:10:05.907' AS DateTime), NULL, NULL)
INSERT [dbo].[USERS] ([Id], [UserName], [Password], [Employee_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (3, N'manageHr', N'74bbe0e185d65998d4fecc8517dd98d0bed47036b603779d470c0eaafb04c196', 3, 0, 1, CAST(N'2025-02-20T10:10:05.907' AS DateTime), CAST(N'2025-02-20T10:10:05.907' AS DateTime), NULL, NULL)
INSERT [dbo].[USERS] ([Id], [UserName], [Password], [Employee_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (4, N'HuyBinh', N'74bbe0e185d65998d4fecc8517dd98d0bed47036b603779d470c0eaafb04c196', 4, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[USERS] ([Id], [UserName], [Password], [Employee_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (5, N'TranHuy', N'74bbe0e185d65998d4fecc8517dd98d0bed47036b603779d470c0eaafb04c196', 5, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
INSERT [dbo].[USERS] ([Id], [UserName], [Password], [Employee_Id], [IsDelete], [IsActive], [Create_Date], [Update_Date], [Create_By], [Update_By]) VALUES (6, N'HoangManh', N'74bbe0e185d65998d4fecc8517dd98d0bed47036b603779d470c0eaafb04c196', 6, 0, 1, CAST(N'1900-01-01T00:00:00.000' AS DateTime), CAST(N'1900-01-01T00:00:00.000' AS DateTime), N'', N'')
SET IDENTITY_INSERT [dbo].[USERS] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__CATEGORY__A25C5AA7AD2DBF43]    Script Date: 3/25/2025 7:08:05 PM ******/
ALTER TABLE [dbo].[CATEGORY] ADD UNIQUE NONCLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__DEPARTME__A25C5AA70DA9BD48]    Script Date: 3/25/2025 7:08:05 PM ******/
ALTER TABLE [dbo].[DEPARTMENT] ADD UNIQUE NONCLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__EMPLOYEE__A25C5AA7E6BF44E4]    Script Date: 3/25/2025 7:08:05 PM ******/
ALTER TABLE [dbo].[EMPLOYEE] ADD UNIQUE NONCLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__USERS__C9F28456F9CEABC6]    Script Date: 3/25/2025 7:08:05 PM ******/
ALTER TABLE [dbo].[USERS] ADD UNIQUE NONCLUSTERED 
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ANALYSIS] ADD  DEFAULT ((0)) FOR [Total]
GO
ALTER TABLE [dbo].[ANALYSIS] ADD  DEFAULT ((0)) FOR [Ontime]
GO
ALTER TABLE [dbo].[ANALYSIS] ADD  DEFAULT ((0)) FOR [Late]
GO
ALTER TABLE [dbo].[ANALYSIS] ADD  DEFAULT ((0)) FOR [Overdue]
GO
ALTER TABLE [dbo].[ANALYSIS] ADD  DEFAULT ((0)) FOR [Processing]
GO
ALTER TABLE [dbo].[ANALYSIS] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[ANALYSIS] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ANALYSIS] ADD  DEFAULT (getdate()) FOR [Create_Date]
GO
ALTER TABLE [dbo].[ANALYSIS] ADD  DEFAULT (getdate()) FOR [Update_Date]
GO
ALTER TABLE [dbo].[ANALYSIS] ADD  DEFAULT (NULL) FOR [Create_By]
GO
ALTER TABLE [dbo].[ANALYSIS] ADD  DEFAULT (NULL) FOR [Update_By]
GO
ALTER TABLE [dbo].[BASELINEASSESSMENT] ADD  DEFAULT ((0)) FOR [VolumeAssessment]
GO
ALTER TABLE [dbo].[BASELINEASSESSMENT] ADD  DEFAULT ((0)) FOR [ProgressAssessment]
GO
ALTER TABLE [dbo].[BASELINEASSESSMENT] ADD  DEFAULT ((0)) FOR [QualityAssessment]
GO
ALTER TABLE [dbo].[BASELINEASSESSMENT] ADD  DEFAULT ((0)) FOR [SummaryOfReviews]
GO
ALTER TABLE [dbo].[BASELINEASSESSMENT] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[BASELINEASSESSMENT] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[BASELINEASSESSMENT] ADD  DEFAULT (getdate()) FOR [Create_Date]
GO
ALTER TABLE [dbo].[BASELINEASSESSMENT] ADD  DEFAULT (getdate()) FOR [Update_Date]
GO
ALTER TABLE [dbo].[BASELINEASSESSMENT] ADD  DEFAULT (NULL) FOR [Create_By]
GO
ALTER TABLE [dbo].[BASELINEASSESSMENT] ADD  DEFAULT (NULL) FOR [Update_By]
GO
ALTER TABLE [dbo].[CATEGORY] ADD  DEFAULT ((0)) FOR [Id_Parent]
GO
ALTER TABLE [dbo].[CATEGORY] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[CATEGORY] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[CATEGORY] ADD  DEFAULT (getdate()) FOR [Create_Date]
GO
ALTER TABLE [dbo].[CATEGORY] ADD  DEFAULT (getdate()) FOR [Update_Date]
GO
ALTER TABLE [dbo].[CATEGORY] ADD  DEFAULT (NULL) FOR [Create_By]
GO
ALTER TABLE [dbo].[CATEGORY] ADD  DEFAULT (NULL) FOR [Update_By]
GO
ALTER TABLE [dbo].[DEPARTMENT] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[DEPARTMENT] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[DEPARTMENT] ADD  DEFAULT (getdate()) FOR [Create_Date]
GO
ALTER TABLE [dbo].[DEPARTMENT] ADD  DEFAULT (getdate()) FOR [Update_Date]
GO
ALTER TABLE [dbo].[DEPARTMENT] ADD  DEFAULT (NULL) FOR [Create_By]
GO
ALTER TABLE [dbo].[DEPARTMENT] ADD  DEFAULT (NULL) FOR [Update_By]
GO
ALTER TABLE [dbo].[EMPLOYEE] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[EMPLOYEE] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[EMPLOYEE] ADD  DEFAULT (getdate()) FOR [Create_Date]
GO
ALTER TABLE [dbo].[EMPLOYEE] ADD  DEFAULT (getdate()) FOR [Update_Date]
GO
ALTER TABLE [dbo].[EMPLOYEE] ADD  DEFAULT (NULL) FOR [Create_By]
GO
ALTER TABLE [dbo].[EMPLOYEE] ADD  DEFAULT (NULL) FOR [Update_By]
GO
ALTER TABLE [dbo].[JOB] ADD  CONSTRAINT [DF__JOB__Deadline1__25518C17]  DEFAULT (NULL) FOR [Deadline1]
GO
ALTER TABLE [dbo].[JOB] ADD  CONSTRAINT [DF__JOB__Deadline2__2645B050]  DEFAULT (NULL) FOR [Deadline2]
GO
ALTER TABLE [dbo].[JOB] ADD  CONSTRAINT [DF__JOB__Deadline3__2739D489]  DEFAULT (NULL) FOR [Deadline3]
GO
ALTER TABLE [dbo].[JOB] ADD  CONSTRAINT [DF__JOB__IsDelete__282DF8C2]  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[JOB] ADD  CONSTRAINT [DF__JOB__IsActive__29221CFB]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[JOB] ADD  CONSTRAINT [DF__JOB__Create_Date__2A164134]  DEFAULT (getdate()) FOR [Create_Date]
GO
ALTER TABLE [dbo].[JOB] ADD  CONSTRAINT [DF__JOB__Update_Date__2B0A656D]  DEFAULT (getdate()) FOR [Update_Date]
GO
ALTER TABLE [dbo].[JOB] ADD  CONSTRAINT [DF__JOB__Create_By__2BFE89A6]  DEFAULT (NULL) FOR [Create_By]
GO
ALTER TABLE [dbo].[JOB] ADD  CONSTRAINT [DF__JOB__Update_By__2CF2ADDF]  DEFAULT (NULL) FOR [Update_By]
GO
ALTER TABLE [dbo].[JOBMAPEMPLOYEE] ADD  CONSTRAINT [DF__JOBMAPEMP__IsDel__31B762FC]  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[JOBMAPEMPLOYEE] ADD  CONSTRAINT [DF__JOBMAPEMP__IsAct__32AB8735]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[JOBMAPEMPLOYEE] ADD  CONSTRAINT [DF__JOBMAPEMP__Creat__339FAB6E]  DEFAULT (getdate()) FOR [Create_Date]
GO
ALTER TABLE [dbo].[JOBMAPEMPLOYEE] ADD  CONSTRAINT [DF__JOBMAPEMP__Updat__3493CFA7]  DEFAULT (getdate()) FOR [Update_Date]
GO
ALTER TABLE [dbo].[JOBMAPEMPLOYEE] ADD  CONSTRAINT [DF__JOBMAPEMP__Creat__3587F3E0]  DEFAULT (NULL) FOR [Create_By]
GO
ALTER TABLE [dbo].[JOBMAPEMPLOYEE] ADD  CONSTRAINT [DF__JOBMAPEMP__Updat__367C1819]  DEFAULT (NULL) FOR [Update_By]
GO
ALTER TABLE [dbo].[POSITION] ADD  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[POSITION] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[POSITION] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[POSITION] ADD  DEFAULT (getdate()) FOR [Create_Date]
GO
ALTER TABLE [dbo].[POSITION] ADD  DEFAULT (getdate()) FOR [Update_Date]
GO
ALTER TABLE [dbo].[POSITION] ADD  DEFAULT (NULL) FOR [Create_By]
GO
ALTER TABLE [dbo].[POSITION] ADD  DEFAULT (NULL) FOR [Update_By]
GO
ALTER TABLE [dbo].[SCORE] ADD  CONSTRAINT [DF__SCORE__Completio__3A4CA8FD]  DEFAULT (NULL) FOR [CompletionDate]
GO
ALTER TABLE [dbo].[SCORE] ADD  CONSTRAINT [DF__SCORE__VolumeAss__3B40CD36]  DEFAULT ((0)) FOR [VolumeAssessment]
GO
ALTER TABLE [dbo].[SCORE] ADD  CONSTRAINT [DF__SCORE__ProgressA__3C34F16F]  DEFAULT ((0)) FOR [ProgressAssessment]
GO
ALTER TABLE [dbo].[SCORE] ADD  CONSTRAINT [DF__SCORE__QualityAs__3D2915A8]  DEFAULT ((0)) FOR [QualityAssessment]
GO
ALTER TABLE [dbo].[SCORE] ADD  CONSTRAINT [DF__SCORE__SummaryOf__3E1D39E1]  DEFAULT ((0)) FOR [SummaryOfReviews]
GO
ALTER TABLE [dbo].[SCORE] ADD  CONSTRAINT [DF_SCORE_Progress]  DEFAULT ((0)) FOR [Progress]
GO
ALTER TABLE [dbo].[SCORE] ADD  CONSTRAINT [DF__SCORE__IsDelete__3F115E1A]  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[SCORE] ADD  CONSTRAINT [DF__SCORE__IsActive__40058253]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[SCORE] ADD  CONSTRAINT [DF__SCORE__Create_Da__40F9A68C]  DEFAULT (getdate()) FOR [Create_Date]
GO
ALTER TABLE [dbo].[SCORE] ADD  CONSTRAINT [DF__SCORE__Update_Da__41EDCAC5]  DEFAULT (getdate()) FOR [Update_Date]
GO
ALTER TABLE [dbo].[SCORE] ADD  CONSTRAINT [DF__SCORE__Create_By__42E1EEFE]  DEFAULT (NULL) FOR [Create_By]
GO
ALTER TABLE [dbo].[SCORE] ADD  CONSTRAINT [DF__SCORE__Update_By__43D61337]  DEFAULT (NULL) FOR [Update_By]
GO
ALTER TABLE [dbo].[SYSTEMSW] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[SYSTEMSW] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[SYSTEMSW] ADD  DEFAULT (getdate()) FOR [Create_Date]
GO
ALTER TABLE [dbo].[SYSTEMSW] ADD  DEFAULT (getdate()) FOR [Update_Date]
GO
ALTER TABLE [dbo].[SYSTEMSW] ADD  DEFAULT (NULL) FOR [Create_By]
GO
ALTER TABLE [dbo].[SYSTEMSW] ADD  DEFAULT (NULL) FOR [Update_By]
GO
ALTER TABLE [dbo].[USERS] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[USERS] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[USERS] ADD  DEFAULT (getdate()) FOR [Create_Date]
GO
ALTER TABLE [dbo].[USERS] ADD  DEFAULT (getdate()) FOR [Update_Date]
GO
ALTER TABLE [dbo].[USERS] ADD  DEFAULT (NULL) FOR [Create_By]
GO
ALTER TABLE [dbo].[USERS] ADD  DEFAULT (NULL) FOR [Update_By]
GO
ALTER TABLE [dbo].[ANALYSIS]  WITH CHECK ADD FOREIGN KEY([Employee_Id])
REFERENCES [dbo].[EMPLOYEE] ([Id])
GO
ALTER TABLE [dbo].[BASELINEASSESSMENT]  WITH CHECK ADD FOREIGN KEY([Employee_Id])
REFERENCES [dbo].[EMPLOYEE] ([Id])
GO
ALTER TABLE [dbo].[EMPLOYEE]  WITH CHECK ADD FOREIGN KEY([Department_Id])
REFERENCES [dbo].[DEPARTMENT] ([Id])
GO
ALTER TABLE [dbo].[EMPLOYEE]  WITH CHECK ADD FOREIGN KEY([Position_Id])
REFERENCES [dbo].[POSITION] ([Id])
GO
ALTER TABLE [dbo].[JOB]  WITH CHECK ADD  CONSTRAINT [FK__JOB__Category_Id__245D67DE] FOREIGN KEY([Category_Id])
REFERENCES [dbo].[CATEGORY] ([Id])
GO
ALTER TABLE [dbo].[JOB] CHECK CONSTRAINT [FK__JOB__Category_Id__245D67DE]
GO
ALTER TABLE [dbo].[JOBMAPEMPLOYEE]  WITH CHECK ADD  CONSTRAINT [FK__JOBMAPEMP__Emplo__2FCF1A8A] FOREIGN KEY([Employee_Id])
REFERENCES [dbo].[EMPLOYEE] ([Id])
GO
ALTER TABLE [dbo].[JOBMAPEMPLOYEE] CHECK CONSTRAINT [FK__JOBMAPEMP__Emplo__2FCF1A8A]
GO
ALTER TABLE [dbo].[JOBMAPEMPLOYEE]  WITH CHECK ADD  CONSTRAINT [FK__JOBMAPEMP__Job_I__30C33EC3] FOREIGN KEY([Job_Id])
REFERENCES [dbo].[JOB] ([Id])
GO
ALTER TABLE [dbo].[JOBMAPEMPLOYEE] CHECK CONSTRAINT [FK__JOBMAPEMP__Job_I__30C33EC3]
GO
ALTER TABLE [dbo].[SCORE]  WITH CHECK ADD  CONSTRAINT [FK__SCORE__JobMapEmp__395884C4] FOREIGN KEY([JobMapEmployee_Id])
REFERENCES [dbo].[JOBMAPEMPLOYEE] ([Id])
GO
ALTER TABLE [dbo].[SCORE] CHECK CONSTRAINT [FK__SCORE__JobMapEmp__395884C4]
GO
ALTER TABLE [dbo].[USERS]  WITH CHECK ADD FOREIGN KEY([Employee_Id])
REFERENCES [dbo].[EMPLOYEE] ([Id])
GO
