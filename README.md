![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white) 

# EasySave V1

Simple and efficient console backup tool. This is the **V1** release for Deliverable 1.

## How to Run

### 1. From Visual Studio
1. Open the solution file `EasySave.slnx`.
2. **Important:** If `EasyLog` is set as the startup project by default (name in bold), right-click the **EasySave** project and select **"Set as Startup Project"**.
3. Press `F5` to build and run the application.

### 2. From Executable (.exe)
1. Navigate to the build folder: `EasySave/bin/Debug/net8.0/`.
2. Run `EasySave.exe` directly.

## CLI Arguments

You can bypass the main menu by passing arguments directly to the executable:

**Execute Jobs (by index)**
- Run jobs 1 and 3: `EasySave.exe 1;3`

## Important File Locations

All application data is stored in `%AppData%/EasySave/` for persistence:

**Configuration & State (/data folder)**
- Listjobs.json: Stores the configuration of your 5 backup jobs.
- state.json: Real-time status and progress of the active backup task.

**History (/logs folder)**
- YYYY-MM-DD.json: Daily logs containing detailed transfer metrics.

## Features (V1)
- Job Management: Create, list, and delete up to 5 backup jobs.
- Strategies: Support for both Complete and Differential backups.
- Real-time Tracking: Live progress (percentage and file count) in console and window title.
- Git-style UI: Clean, color-coded CLI output.
- Multi-language: Support for English and French.
