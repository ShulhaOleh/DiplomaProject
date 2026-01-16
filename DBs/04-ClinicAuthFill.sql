SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;
USE ClinicAuth;

INSERT INTO `Users` (`Username`, `PasswordHash`, `Role`, `LinkedID`, `CreatedAt`) VALUES
('doctor1', '202cb962ac59075b964b07152d234b70', 'Doctor', 1, '2025-05-22 17:50:54'),
('doctor2', '202cb962ac59075b964b07152d234b70', 'Doctor', 2, '2025-05-22 17:50:54'),
('reception1', '202cb962ac59075b964b07152d234b70', 'Receptionist', 1, '2025-05-22 17:50:54'),
('reception2', '202cb962ac59075b964b07152d234b70', 'Receptionist', 2, '2025-05-22 17:50:54'),
('admin', '202cb962ac59075b964b07152d234b70', 'Admin', NULL, '2025-05-22 17:50:54');

