create database WorkTrackingSystem

CREATE TABLE DEPARTMENT(											
											
Id BIGINT  PRIMARY KEY,											
Code NVARCHAR(50) ,											
Name NVARCHAR(255) ,											
Description NVARCHAR(MAX) ,											
IsDelete BIT DEFAULT 0,											
IsActive BIT DEFAULT 1,											
Create_Date DATETIME DEFAULT GETDATE(),											
Update_Date DATETIME DEFAULT GETDATE(),											
Create_By BIGINT DEFAULT NULL,											
Update_By BIGINT DEFAULT NULL,)											
GO											
											
CREATE TABLE POSITION(											
											
Id BIGINT  PRIMARY KEY,											
Name NVARCHAR(255) ,											
Description NVARCHAR(MAX) ,											
Status BIT DEFAULT 0,											
IsDelete BIT DEFAULT 0,											
IsActive BIT DEFAULT 1,											
Create_Date DATETIME DEFAULT GETDATE(),											
Update_Date DATETIME DEFAULT GETDATE(),											
Create_By BIGINT DEFAULT NULL,											
Update_By BIGINT DEFAULT NULL,)											
											
											
											
CREATE TABLE EMPLOYEE(											
											
Id BIGINT  PRIMARY KEY,											
Department_id BIGINT FOREIGN KEY (Department_id) REFERENCES Department(Id),																				
Position_Id BIGINT FOREIGN KEY (Position_Id) REFERENCES Position(Id),											
Code NVARCHAR(50) ,											
First_Name NVARCHAR(255) ,											
Last_Name NVARCHAR(255) ,											
Gender NVARCHAR(10) ,											
Birthday DATE  ,											
Phone NVARCHAR(15) ,											
Email NVARCHAR(255) ,											
hire_date DATE  ,											
Address  NVARCHAR(MAX) ,											
IsDelete BIT DEFAULT 0,											
IsActive BIT DEFAULT 1,											
Create_Date DATETIME DEFAULT GETDATE(),											
Update_Date DATETIME DEFAULT GETDATE(),											
Create_By BIGINT DEFAULT NULL,											
Update_By BIGINT DEFAULT NULL,)											
											
											
GO											
											
CREATE TABLE CATEGORY(											
											
Id BIGINT  PRIMARY KEY,											
Code VARCHAR(25) ,											
Name NVARCHAR(255) ,											
Description NVARCHAR(MAX) ,											
Id_Parent BIGINT DEFAULT 0,											
IsDelete BIT DEFAULT 0,											
IsActive BIT DEFAULT 1,											
Create_Date DATETIME DEFAULT GETDATE(),											
Update_Date DATETIME DEFAULT GETDATE(),											
Create_By BIGINT DEFAULT NULL,											
Update_By BIGINT DEFAULT NULL,)											
GO											
											
											
											
CREATE TABLE JOB(											
											
Id BIGINT  PRIMARY KEY,											
Employee_Id BIGINT FOREIGN KEY (Employee_Id) REFERENCES Employee(Id),											
Category_Id BIGINT FOREIGN KEY (Category_Id) REFERENCES Category(Id),											
Name NVARCHAR(MAX) ,											
Description NVARCHAR(MAX) ,											
Deadline1 DATE DEFAULT NULL,											
Deadline2 DATE DEFAULT NULL,											
Deadline3 DATE DEFAULT NULL,											
CompletionDate DATE DEFAULT NULL,											
Status TINYINT ,											
VolumeAssessment FLOAT DEFAULT 0,											
ProgressAssessment FLOAT DEFAULT 0,											
QualityAssessment FLOAT DEFAULT 0,											
SummaryOfReviews FLOAT DEFAULT 0,											
Time DATETIME ,											
IsDelete BIT DEFAULT 0,											
IsActive BIT DEFAULT 1,											
Create_Date DATETIME DEFAULT GETDATE(),											
Update_Date DATETIME DEFAULT GETDATE(),											
Create_By BIGINT DEFAULT NULL,											
Update_By BIGINT DEFAULT NULL,)											
GO											
											
											
CREATE TABLE BASELINEASSESSMENT(											
											
Id BIGINT  PRIMARY KEY,											
Employee_Id BIGINT FOREIGN KEY (Employee_Id) REFERENCES Employee(Id),											
VolumeAssessment FLOAT DEFAULT 0,											
ProgressAssessment FLOAT DEFAULT 0,											
QualityAssessment FLOAT DEFAULT 0,											
SummaryOfReviews FLOAT DEFAULT 0,											
Time DATETIME ,											
Evaluate BIT ,											
IsDelete BIT DEFAULT 0,											
IsActive BIT DEFAULT 1,											
Create_Date DATETIME DEFAULT GETDATE(),											
Update_Date DATETIME DEFAULT GETDATE(),											
Create_By BIGINT DEFAULT NULL,											
Update_By BIGINT DEFAULT NULL,)											
GO											
											
											
CREATE TABLE ANALYSIS(											
											
Id BIGINT  PRIMARY KEY,											
Employee_Id BIGINT FOREIGN KEY (Employee_Id) REFERENCES Employee(Id),											
Total FLOAT DEFAULT 0,											
Ontime INT DEFAULT 0,											
Late INT DEFAULT 0,											
Overdue INT DEFAULT 0,											
Processing INT DEFAULT 0,											
Time DATETIME ,											
IsDelete BIT DEFAULT 0,											
IsActive BIT DEFAULT 1,											
Create_Date DATETIME DEFAULT GETDATE(),											
Update_Date DATETIME DEFAULT GETDATE(),											
Create_By BIGINT DEFAULT NULL,											
Update_By BIGINT DEFAULT NULL,)											
GO											
											
CREATE TABLE SYSTEMS(											
											
Id BIGINT  PRIMARY KEY,											
Name VARCHAR(MAX) ,											
Vaulue NVARCHAR(MAX) ,											
Description NVARCHAR(MAX) ,											
IsDelete BIT DEFAULT 0,											
IsActive BIT DEFAULT 1,											
Create_Date DATETIME DEFAULT GETDATE(),											
Update_Date DATETIME DEFAULT GETDATE(),											
Create_By BIGINT DEFAULT NULL,											
Update_By BIGINT DEFAULT NULL,)											
GO											
											
CREATE TABLE USERS(											
											
Id BIGINT  PRIMARY KEY,											
UserName VARCHAR(MAX) ,											
Passwword VARCHAR(MAX) ,											
Employee_Id BIGINT FOREIGN KEY (Employee_Id) REFERENCES Employee(Id),											
IsDelete BIT DEFAULT 0,											
IsActive BIT DEFAULT 1,											
Create_Date DATETIME DEFAULT GETDATE(),											
Update_Date DATETIME DEFAULT GETDATE(),											
Create_By BIGINT DEFAULT NULL,											
Update_By BIGINT DEFAULT NULL,)											
GO											
											