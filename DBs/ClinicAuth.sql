DROP DATABASE IF EXISTS ClinicAuth;

CREATE DATABASE IF NOT EXISTS ClinicAuth;
USE ClinicAuth;

CREATE TABLE Users (
    UserID INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash CHAR(32) NOT NULL,
    Role ENUM('Doctor', 'Receptionist', 'Admin') NOT NULL,
    LinkedID INT DEFAULT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    CHECK (
        (Role = 'Admin' AND LinkedID IS NULL) OR
        (Role IN ('Doctor', 'Receptionist') AND LinkedID IS NOT NULL)
    )
);
