<div align="center">

![Demo](Clinic/Assets/hospital.png)

# Clinic Management System

[![ENG](https://img.shields.io/badge/lang-English-red)](readme.md)
[![UKR](https://img.shields.io/badge/lang-Ukrainian-blue)](readme.uk.md)

</div>

Desktop application for Windows for medical personnel. Automation of tasks for doctors, receptionists, and administrators related to patient appointment management.

## Overview

This repository contains the complete source code of a diploma-level project, structured as a Visual Studio solution. It includes the main application logic, database-related components, and auxiliary scripts for project setup.

The project is intended for academic evaluation and showcases object-oriented design, database integration, and application-level architecture.

## Technologies

- C# / .NET 8
- WPF (Windows Presentation Foundation)
- MySQL 8.0 (via Docker)
- Visual Studio 2022
- MVVM (Model-View-ViewModel)
- MySql.Data 9.x, Extended.Wpf.Toolkit

## Prerequisites

Before you begin, make sure the following are installed:

| Tool | Version | Download |
|------|---------|----------|
| Windows | 10 or 11 | — |
| Visual Studio | 2022 (any edition) | https://visualstudio.microsoft.com |
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Docker Desktop | latest | https://www.docker.com/products/docker-desktop |

> **Note:** This is a Windows-only application (WPF). It cannot be built or run on Linux or macOS.

## Setup

### 1. Clone the repository

```
git clone <repository-url>
cd Clinic
```

### 2. Configure the environment file

Copy `.env.example` to `.env` in the root directory and set your database password:

```
cp .env.example .env
```

Edit `.env`:
```
MYSQL_ROOT_PASSWORD=your_password_here
MYSQL_HOST=localhost
MYSQL_PORT=3306
```

> `.env` is listed in `.gitignore` and will not be committed.

### 3. Start the database

```
docker-compose up -d
```

Docker will automatically:
- Pull the MySQL 8.0 image
- Create both `Clinic` and `ClinicAuth` databases
- Run all SQL scripts from the `DBs/` folder in order (schema + seed data)

Wait a few seconds for the container to initialize. You can check its status with:

```
docker ps
```

### 4. Open the solution

Open `Clinic.sln` in Visual Studio 2022. NuGet packages will restore automatically on first build.

### 5. Build and run

Press **F5** or use **Debug → Start Debugging**.

## Default login credentials

These accounts are created by the seed data (`DBs/04-ClinicAuthFill.sql`):

| Username | Password | Role |
|----------|----------|------|
| `admin` | `123` | Admin |
| `doctor1` | `123` | Doctor |
| `doctor2` | `123` | Doctor |
| `reception1` | `123` | Receptionist |
| `reception2` | `123` | Receptionist |

> Change these passwords after first login in a real deployment.

## Project structure

```
Clinic/
├── Clinic/              # Main WPF application (C# source)
│   ├── DB/              # Database connection helpers
│   ├── Models/          # Data models
│   ├── View/            # XAML views (Admin, Doctor, Receptionist)
│   ├── ViewModels/      # MVVM view models
│   ├── Languages/       # Localization resources (EN, UK)
│   └── Assets/          # Icons and images
├── DBs/                 # SQL scripts (run by Docker on first start)
│   ├── 00-Init.sql      # Creates databases
│   ├── 01-Clinic.sql    # Clinic schema
│   ├── 02-ClinicAuth.sql# Auth schema
│   ├── 03-ClinicFill.sql# Clinic seed data
│   └── 04-ClinicAuthFill.sql # Auth seed data (users)
├── scripts/             # Utility scripts
├── .env.example         # Environment variable template
├── docker-compose.yml   # MySQL container configuration
└── Clinic.sln           # Visual Studio solution file
```

## Stopping the database

```
docker-compose down
```

To also delete all stored data:

```
docker-compose down -v
```
