/* RaceDay Part 1 - SQL Server schema.  This script matches ERD.png exactly. */
IF DB_ID('RaceDayDB') IS NULL
CREATE DATABASE RaceDayDb;
GO
USE RaceDayDb;
GO
CREATE TABLE [User] (UserID INT IDENTITY(1,1) PRIMARY KEY, Username VARCHAR(80) NOT NULL UNIQUE, Email VARCHAR(160) NOT NULL UNIQUE, PasswordHash VARCHAR(255) NOT NULL, Role VARCHAR(20) NOT NULL CHECK(Role IN ('Organiser','Participant')), CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());
CREATE TABLE Organizer (OrganizerID INT IDENTITY(1,1) PRIMARY KEY, UserID INT NOT NULL UNIQUE FOREIGN KEY REFERENCES [User](UserID), OrganisationName VARCHAR(120) NOT NULL, ContactName VARCHAR(100) NOT NULL, Phone VARCHAR(30) NOT NULL);
CREATE TABLE Participant (ParticipantID INT IDENTITY(1,1) PRIMARY KEY, UserID INT NOT NULL UNIQUE FOREIGN KEY REFERENCES [User](UserID), FirstName VARCHAR(80) NOT NULL, LastName VARCHAR(80) NOT NULL, DateOfBirth DATE NOT NULL, Gender VARCHAR(20) NOT NULL, EmergencyContact VARCHAR(160) NOT NULL, ProfilePictureUrl VARCHAR(500) NULL);
CREATE TABLE Event (EventID INT IDENTITY(1,1) PRIMARY KEY, OrganizerID INT NOT NULL FOREIGN KEY REFERENCES Organizer(OrganizerID), EventName VARCHAR(160) NOT NULL, Description VARCHAR(1000) NOT NULL, EventDate DATE NOT NULL, Location VARCHAR(160) NOT NULL, Distance DECIMAL(8,2) NOT NULL CHECK(Distance>0), EventType VARCHAR(20) NOT NULL CHECK(EventType IN ('run','walk','cycle')), BannerImageUrl VARCHAR(500) NULL, Status VARCHAR(20) NOT NULL DEFAULT 'Open', CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());
CREATE TABLE Category (
CategoryID INT IDENTITY(1,1) PRIMARY KEY,
EventID INT NOT NULL FOREIGN KEY REFERENCES Event(EventID),
CategoryName VARCHAR(100) NOT NULL,
Distance DECIMAL(8,2) NOT NULL CHECK (Distance > 0),
MinAge INT NOT NULL CHECK (MinAge >= 0),
MaxAge INT NOT NULL,
EntryFee DECIMAL(10,2) NOT NULL CHECK (EntryFee >= 0),
CONSTRAINT CK_Category_MaxAge_MinAge CHECK (MaxAge >= MinAge)
);CREATE TABLE Enrollment (EnrollmentID INT IDENTITY(1,1) PRIMARY KEY, ParticipantID INT NOT NULL FOREIGN KEY REFERENCES Participant(ParticipantID), CategoryID INT NOT NULL FOREIGN KEY REFERENCES Category(CategoryID), EnrollmentDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), Status VARCHAR(20) NOT NULL DEFAULT 'Active', PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'Pending', BibNumber VARCHAR(30) NULL, CONSTRAINT UQ_Enrollment UNIQUE(ParticipantID,CategoryID));
CREATE TABLE Result (ResultID INT IDENTITY(1,1) PRIMARY KEY, EnrollmentID INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Enrollment(EnrollmentID), FinishTime TIME NOT NULL, Position INT NOT NULL CHECK(Position>0), Remarks VARCHAR(500) NULL);
GO
/* BCrypt hashes below are intentionally placeholders: create real accounts through /api/auth/register. */
INSERT INTO [User](Username,Email,PasswordHash,Role) VALUES ('trailmasters','organiser1@raceday.test','$2a$11$replaceWithBCryptHash','Organiser'),('citysports','organiser2@raceday.test','$2a$11$replaceWithBCryptHash','Organiser'),('samrunner','participant1@raceday.test','$2a$11$replaceWithBCryptHash','Participant'),('leewalker','participant2@raceday.test','$2a$11$replaceWithBCryptHash','Participant');
INSERT INTO Organizer(UserID,OrganisationName,ContactName,Phone) VALUES (1,'Trail Masters','Ava Mokoena','0115551001'),(2,'City Sports','Ben Naidoo','0115551002');
INSERT INTO Participant(UserID,FirstName,LastName,DateOfBirth,Gender,EmergencyContact) VALUES (3,'Sam','Runner','1998-04-12','Non-binary','T. Runner 0825551003'),(4,'Lee','Walker','1987-09-20','Female','M. Walker 0825551004');
INSERT INTO Event(OrganizerID,EventName,Description,EventDate,Location,Distance,EventType) VALUES (1,'Mountain Dash','Trail race','2026-11-10','Magaliesberg',15,'run'),(1,'Spring Fun Walk','Family walk','2026-10-05','Johannesburg',5,'walk'),(2,'City Cycle','Road cycling event','2026-12-01','Pretoria',40,'cycle');
INSERT INTO Category(EventID,CategoryName,Distance,MinAge,MaxAge,EntryFee) VALUES (1,'Open 15 km',15,18,99,250),(2,'Family 5 km',5,8,99,80),(3,'Open 40 km',40,18,99,300);
INSERT INTO Enrollment(ParticipantID,CategoryID,BibNumber) VALUES (1,1,'MD101'),(2,2,'SW201');
INSERT INTO Result(EnrollmentID,FinishTime,Position,Remarks) VALUES (1,'01:18:32',1,'Excellent trail finish');
