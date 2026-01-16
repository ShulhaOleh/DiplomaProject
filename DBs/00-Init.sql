-- Инициализация баз данных для клиники
-- Этот файл должен быть в папке DBs/DB Code/
-- Он выполняется первым (00-) при первом запуске контейнера

CREATE DATABASE IF NOT EXISTS Clinic 
    CHARACTER SET utf8mb4 
    COLLATE utf8mb4_unicode_ci;

CREATE DATABASE IF NOT EXISTS ClinicAuth 
    CHARACTER SET utf8mb4 
    COLLATE utf8mb4_unicode_ci;

-- Используем базу Clinic для следующих скриптов
USE Clinic;
USE ClinicAuth;

-- Создаем пользователя для приложения (опционально, но безопаснее чем root)
-- CREATE USER IF NOT EXISTS 'clinic_user'@'%' IDENTIFIED BY 'clinic_password';
-- GRANT ALL PRIVILEGES ON Clinic.* TO 'clinic_user'@'%';
-- GRANT ALL PRIVILEGES ON ClinicAuth.* TO 'clinic_user'@'%';
-- FLUSH PRIVILEGES;