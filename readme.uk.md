<div align="center">

![Demo](Clinic/Assets/hospital.png)

# Система управління клінікою

[![ENG](https://img.shields.io/badge/lang-English-red)](readme.md)
[![UKR](https://img.shields.io/badge/lang-Ukrainian-blue)](readme.uk.md)

</div>

Десктопний застосунок для Windows, призначений для медичного персоналу. Автоматизація задач лікарів, реєстраторів та адміністраторів, пов'язаних з управлінням записами пацієнтів на прийом.

## Огляд

Цей репозиторій містить повний вихідний код дипломного проєкту, структурованого у вигляді рішення Visual Studio. Він включає основну логіку застосунку, компоненти для роботи з базою даних, а також допоміжні скрипти для налаштування проєкту.

Проєкт призначений для академічного оцінювання та демонструє застосування принципів об'єктно-орієнтованого програмування, інтеграцію з базою даних та архітектуру прикладного рівня.

## Технології

- C# / .NET 8
- WPF (Windows Presentation Foundation)
- MySQL 8.0 (через Docker)
- Visual Studio 2022
- MVVM (Model-View-ViewModel)
- MySql.Data 9.x, Extended.Wpf.Toolkit

## Вимоги

Перед початком переконайтесь, що встановлено:

| Інструмент | Версія |
|------------|--------|
| Windows | 10 або 11 |
| Visual Studio | 2022 (будь-яке видання) |
| .NET SDK | 8.0 |
| Docker Desktop | остання |

> **Примітка:** Це застосунок лише для Windows (WPF). Збірка та запуск на Linux або macOS неможливі.

## Налаштування

### 1. Клонування репозиторію

```
git clone <repository-url>
cd Clinic
```

### 2. Активація git хуків

```
git config core.hooksPath .githooks
```

> Це активує pre-commit хук, який автоматично оновлює структуру проєкту в обох README при кожному коміті.

### 3. Налаштування файлу оточення

Скопіюйте `.env.example` до `.env` у кореневій директорії та встановіть пароль бази даних:

```
cp .env.example .env
```

Відредагуйте `.env`:
```
MYSQL_ROOT_PASSWORD=ваш_пароль
MYSQL_HOST=localhost
MYSQL_PORT=3306
```

> `.env` вказано у `.gitignore` і не буде потрапляти до репозиторію.

### 4. Запуск бази даних

```
docker-compose up -d
```

Docker автоматично:
- Завантажить образ MySQL 8.0
- Створить бази даних `Clinic` та `ClinicAuth`
- Виконає всі SQL-скрипти з папки `DBs/` у правильному порядку (схема + тестові дані)

Зачекайте кілька секунд на ініціалізацію контейнера. Перевірити статус можна командою:

```
docker ps
```

### 5. Відкриття рішення

Відкрийте `Clinic.sln` у Visual Studio 2022. Пакети NuGet відновляться автоматично при першій збірці.

### 6. Збірка та запуск

Натисніть **F5** або скористайтесь **Debug → Start Debugging**.

## Облікові дані за замовчуванням

Ці акаунти створюються разом із тестовими даними (`DBs/04-ClinicAuthFill.sql`):

| Логін | Пароль | Роль |
|-------|--------|------|
| `admin` | `123` | Адміністратор |
| `doctor1` | `123` | Лікар |
| `doctor2` | `123` | Лікар |
| `reception1` | `123` | Реєстратор |
| `reception2` | `123` | Реєстратор |

> Змініть ці паролі після першого входу при реальному використанні.

## Структура проєкту

> Автоматично генерується скриптом `scripts/generate_structure.ps1` при кожному коміті.

```
Clinic/
├── Clinic
│   ├── Assets
│   │   ├── catalog.png
│   │   ├── hospital.ico
│   │   └── hospital.png
│   ├── Converters
│   │   ├── DateOnlyConverter.cs
│   │   ├── InverseBoolConverter.cs
│   │   └── InverseBooleanToVisibilityConverter.cs
│   ├── DB
│   │   ├── AuthDB.cs
│   │   ├── ClinicDB.cs
│   │   ├── DBHelper.cs
│   │   └── EnvLoader.cs
│   ├── Langs
│   │   └── PublishProfiles
│   │       ├── FolderProfile.pubxml
│   │       └── FolderProfile.pubxml.user
│   ├── Languages
│   │   ├── Resources.en.xaml
│   │   └── Resources.uk.xaml
│   ├── Models
│   │   ├── Admin.cs
│   │   ├── Appointment.cs
│   │   ├── AppointmentStatuses.cs
│   │   ├── Doctor.cs
│   │   ├── Patient.cs
│   │   ├── Receptionist.cs
│   │   ├── ScheduleCell.cs
│   │   ├── ScheduleRow.cs
│   │   └── User.cs
│   ├── Properties
│   │   ├── Settings.Designer.cs
│   │   └── Settings.settings
│   ├── View
│   │   ├── Admin
│   │   │   ├── AdminUserManagementView.xaml
│   │   │   ├── AdminUserManagementView.xaml.cs
│   │   │   ├── ConfirmAdminPasswordDialog.xaml
│   │   │   ├── ConfirmAdminPasswordDialog.xaml.cs
│   │   │   ├── EditUserDialog.xaml
│   │   │   └── EditUserDialog.xaml.cs
│   │   ├── Doctor
│   │   │   ├── CompleteAppointmentWindow.xaml
│   │   │   ├── CompleteAppointmentWindow.xaml.cs
│   │   │   ├── DoctorAppointmentsView.xaml
│   │   │   └── DoctorAppointmentsView.xaml.cs
│   │   ├── Receptionist
│   │   │   ├── AppointmentManagementView.xaml
│   │   │   └── AppointmentManagementView.xaml.cs
│   │   ├── Login.xaml
│   │   ├── Login.xaml.cs
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── MakeAppointmentWindow.xaml
│   │   ├── MakeAppointmentWindow.xaml.cs
│   │   ├── ProfileView.xaml
│   │   ├── ProfileView.xaml.cs
│   │   ├── RegisterPatientWindow.xaml
│   │   └── RegisterPatientWindow.xaml.cs
│   ├── ViewModels
│   │   ├── Admin
│   │   │   └── AdminUserManagementViewModel.cs
│   │   ├── Doctor
│   │   │   └── DoctorAppointmentsViewModel.cs
│   │   ├── Receptionist
│   │   │   └── AppointmentManagementViewModel.cs
│   │   ├── AppointmentService.cs
│   │   ├── BaseViewModel.cs
│   │   ├── LanguageManager.cs
│   │   ├── LoginViewModel.cs
│   │   ├── MainWindowViewModel.cs
│   │   ├── MenuItem.cs
│   │   ├── ProfileViewModel.cs
│   │   ├── RegisterPatientViewModel.cs
│   │   ├── RelayCommand.cs
│   │   └── ViewResolver.cs
│   ├── App.config
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── App.xaml.Designer.cs
│   ├── AssemblyInfo.cs
│   ├── Clinic.csproj
│   └── Clinic.csproj.user
├── DBs
│   ├── 00-Init.sql
│   ├── 01-Clinic.sql
│   ├── 02-ClinicAuth.sql
│   ├── 03-ClinicFill.sql
│   └── 04-ClinicAuthFill.sql
├── scripts
│   ├── fix_encoding.bat
│   ├── fix_encoding.ps1
│   ├── generate_structure.bat
│   ├── generate_structure.ps1
│   └── structure.txt
├── .env
├── .env.example
├── .gitignore
├── Clinic.sln
├── docker-compose.yml
├── LICENSE
├── readme.md
└── readme.uk.md
```

## Зупинка бази даних

```
docker-compose down
```

Щоб також видалити всі збережені дані:

```
docker-compose down -v
```
