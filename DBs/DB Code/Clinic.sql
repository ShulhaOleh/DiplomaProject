DROP DATABASE IF EXISTS Clinic;

CREATE DATABASE IF NOT EXISTS Clinic;
USE Clinic;

-- Table of doctor's Specialty
CREATE TABLE Specialties (
    SpecialtyID INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE
);

-- Table of doctors
CREATE TABLE Doctors (
    DoctorID INT AUTO_INCREMENT PRIMARY KEY,
    LastName VARCHAR(50) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    FathersName VARCHAR(50),
    DateOfBirth DATE NOT NULL,
    PhoneNumber VARCHAR(20),
    BreakHour TIME NOT NULL DEFAULT '13:00:00',
    SpecialtyID INT NOT NULL,
	FOREIGN KEY (SpecialtyID) REFERENCES Specialties(SpecialtyID)
);


-- Table of patients
CREATE TABLE Patients (
    PatientID INT AUTO_INCREMENT PRIMARY KEY,
    LastName VARCHAR(50) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    FathersName VARCHAR(50),
    DateOfBirth DATE NOT NULL,
    Gender ENUM('Male', 'Female') NOT NULL,
    PhoneNumber VARCHAR(15)
);

-- Outpatient record
CREATE TABLE AmbulatoryCards (
    AmbulatoryCardID INT AUTO_INCREMENT PRIMARY KEY,
    PatientID INT NOT NULL,
    CreationDate DATE NOT NULL,
    CardNumber VARCHAR(20) UNIQUE,
    BloodType ENUM('A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
);

-- Allergy Guide
CREATE TABLE Allergies (
    AllergyID INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE
);

-- Guide to Chronic Diseases
CREATE TABLE ChronicDiseases (
    DiseaseID INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE
);

-- Linking allergies to the card
CREATE TABLE CardAllergies (
    AmbulatoryCardID INT NOT NULL,
    AllergyID INT NOT NULL,
    PRIMARY KEY (AmbulatoryCardID, AllergyID),
    FOREIGN KEY (AmbulatoryCardID) REFERENCES AmbulatoryCards(AmbulatoryCardID),
    FOREIGN KEY (AllergyID) REFERENCES Allergies(AllergyID)
);

-- Linking chronic diseases to the card
CREATE TABLE CardChronicDiseases (
    AmbulatoryCardID INT NOT NULL,
    DiseaseID INT NOT NULL,
    PRIMARY KEY (AmbulatoryCardID, DiseaseID),
    FOREIGN KEY (AmbulatoryCardID) REFERENCES AmbulatoryCards(AmbulatoryCardID),
    FOREIGN KEY (DiseaseID) REFERENCES ChronicDiseases(DiseaseID)
);

-- Receptionists table
CREATE TABLE Receptionist (
    ReceptionistID INT AUTO_INCREMENT PRIMARY KEY,
    LastName VARCHAR(50) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    FathersName VARCHAR(50),
    PhoneNumber VARCHAR(15)
);


-- Table of appointments
CREATE TABLE Appointments (
    AppointmentID INT AUTO_INCREMENT PRIMARY KEY,
    PatientID INT NOT NULL,
    DoctorID INT NOT NULL,
    ReceptionistID INT,
    AmbulatoryCardID INT NOT NULL,
    AppointmentDate DATETIME NOT NULL,
    Status ENUM('Прийом завершено', 'Пацієнт не з’явився', 'Очікується') DEFAULT 'Очікується',
    Notes TEXT,
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID),
    FOREIGN KEY (ReceptionistID) REFERENCES Receptionist(ReceptionistID),
    FOREIGN KEY (AmbulatoryCardID) REFERENCES AmbulatoryCards(AmbulatoryCardID)
);



