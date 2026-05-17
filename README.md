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

# EasySave V2

Graphical and efficient backup management tool. This is the V2 release featuring a complete WPF Graphical User Interface.

## How to Run

### 1. From Visual Studio
* Open the solution file `EasySave.slnx`.
* **Important:** Right-click the **EasySaveGUI** project and select **"Set as Startup Project"**. (Do not run the old console project).
* Press `F5` to build and run the graphical application.

### 2. From Executable (.exe)
* Navigate to the GUI build folder: `EasySaveGUI/bin/Debug/net8.0-windows/` (path may vary depending on your build configuration).
* Run **`EasySaveGUI.exe`** directly.

## CLI Arguments (Headless Execution)
You can bypass the graphical interface and execute jobs directly in the background by passing arguments to the executable:
* Execute specific jobs: `EasySaveGUI.exe 1;3` (Runs job 1 and 3)
* Execute a range of jobs: `EasySaveGUI.exe 1-3` (Runs jobs 1, 2, and 3)

## Important File Locations
All application data is stored in `%AppData%/EasySave/` for persistence:
* **Configuration & State (`/data` folder)**
  * `Listjobs.json`: Stores the configuration of your backup jobs.
  * `state.json` / `state.xml`: Real-time status and progress of the active backup task.
* **History (`/logs` folder)**
  * `YYYY-MM-DD.json` / `YYYY-MM-DD.xml`: Daily logs containing detailed transfer and encryption metrics.

## Features (V2)
* **Graphical Interface (GUI):** Clean, intuitive, and responsive WPF interface.
* **File Encryption (CryptoSoft):** Seamlessly encrypt specific user-defined file extensions (e.g., `.txt;.pdf`) during the backup process.
* **Business Software Detection:** Automatically blocks or pauses backups if predefined business applications are running.
* **Log Formats:** Switch dynamically between JSON and XML for daily logs and real-time state tracking.
* **Job Management:** Create, list, delete, and execute jobs (individually or simultaneously).
* **Strategies:** Support for both Complete and Differential backups.
* **Real-time Tracking:** Live progress bar and current file operation tracking.
* **Multi-language:** Real-time localization support for English and French.

# EasySave

## V1 — Console Backup Tool

Simple and efficient console backup tool. This is the V1 release for Deliverable 1.

### How to Run

**From Visual Studio**
1. Open `EasySave.slnx`.
2. Right-click the **EasySave** project → *Set as Startup Project*.
3. Press `F5`.

**From Executable**
Navigate to `EasySave/bin/Debug/net8.0/` and run `EasySave.exe`.

### CLI Arguments

```
EasySave.exe 1;3     # Run jobs 1 and 3
```

### Important File Locations

All data is stored in `%AppData%/EasySave/`:

| File | Description |
|---|---|
| `data/Listjobs.json` | Backup job configurations |
| `data/state.json` | Real-time backup progress |
| `logs/YYYY-MM-DD.json` | Daily transfer logs |

### Features

- Job Management: create, list, delete up to 5 jobs
- Complete & Differential backup strategies
- Real-time progress in console & window title
- Multi-language: English / French

---

## V2 — WPF Graphical Interface

Graphical and efficient backup management tool. This is the V2 release featuring a complete WPF GUI.

### How to Run

**From Visual Studio**
1. Open `EasySave.slnx`.
2. Right-click **EasySaveGUI** → *Set as Startup Project*.
3. Press `F5`.

**From Executable**
Navigate to `EasySaveGUI/bin/Debug/net8.0-windows/` and run `EasySaveGUI.exe`.

### CLI Arguments (Headless)

```
EasySaveGUI.exe 1;3    # Run jobs 1 and 3
EasySaveGUI.exe 1-3    # Run jobs 1, 2, and 3
```

### Important File Locations

All data is stored in `%AppData%/EasySave/`:

| File | Description |
|---|---|
| `data/Listjobs.json` | Backup job configurations |
| `data/state.json` / `state.xml` | Real-time backup progress |
| `logs/YYYY-MM-DD.json` / `.xml` | Daily transfer & encryption logs |

### Features

- WPF GUI: clean, responsive interface
- File Encryption via CryptoSoft (e.g. `.txt;.pdf`)
- Business Software Detection: pauses backup if a blocked app is running
- Log format: JSON or XML (switchable at runtime)
- Simultaneous job execution
- Real-time progress bar & current file tracking
- Multi-language: English / French (real-time switch)

---

## V3 — Parallel Processing & Centralized Logging

Advanced backup tool with parallel job execution and a centralized remote log server. This is the V3 release for Deliverable 3.

### How to Run

**From Visual Studio**
1. Open `EasySave.slnx`.
2. Right-click **EasySave** → *Set as Startup Project*.
3. Press `F5`.

**From Executable**
Navigate to `EasySave/bin/Debug/net8.0-windows/` and run `EasySave.exe`.

### Log Server (Docker)

The V3 log server centralizes logs from all running instances of EasySave.

```bash
# Build and start the server
cd EasySaveLogServer
docker build -t easysave-log-server .
docker run -d -p 8080:8080 easysave-log-server
```

The server exposes a REST endpoint at `http://localhost:8080/api/logs` and writes daily log files to the `/app/logs` folder inside the container.

> To persist logs outside the container, mount a volume:
> ```bash
> docker run -d -p 8080:8080 -v "$PWD/logs:/app/logs" easysave-log-server
> ```

### Important File Locations

Local data is still stored in `%AppData%/EasySave/`:

| File | Description |
|---|---|
| `data/Listjobs.json` | Backup job configurations |
| `data/state.json` / `state.xml` | Real-time backup progress |
| `logs/YYYY-MM-DD.json` / `.xml` | Local daily logs |

Centralized logs (written by the Docker server):

| File | Description |
|---|---|
| `centralized_log_YYYY-MM-DD.json` | Aggregated JSON logs from all clients |
| `centralized_log_YYYY-MM-DD.xml` | Aggregated XML logs from all clients |

### Features

All V2 features, plus:

- **Parallel Execution**: all jobs run concurrently across multiple threads
- **Priority Files**: files matching user-defined extensions are transferred first
- **Pause / Resume / Stop**: fine-grained control over each running job
- **Centralized Log Server**: remote ASP.NET server (Dockerized) collects logs from all EasySave instances in real time
- **Multi-instance Support**: multiple EasySave clients can run simultaneously and log to the same server
