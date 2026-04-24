# EasySave

EasySave is a console backup software developed for **ProSoft**.
It allows users to configure and run backup jobs with real-time progress tracking and daily logging.

## Version

**1.0** — First public release (April 2026)

## Features

- Up to **5 backup jobs** (create, list, execute, delete)
- Two backup strategies: **Full** (complete) and **Differential** (modified files only)
- **Real-time progression** tracking written to `state.json`
- **Daily log** files in JSON format, one file per day
- **Bilingual** interface: English / French
- **Multi-job execution** from the menu (e.g. `1;3;4` or `1,3`)
- Support for **local drives, external drives and network drives**

## Requirements

- Windows 10 or later
- .NET Runtime
- Read access on source directories, write access on destination directories

## Getting Started

Launch the application:

    EasySave.exe

The main menu lets you create, execute, list and delete backup jobs, and change the language.

## Project Structure

    Software-Engineering/
    ├── EasyLog/          # Logging library (separate DLL, reusable)
    ├── EasySave/         # Main console application
    │   ├── Model/        # Data models
    │   ├── View/         # Console UI
    │   ├── ViewModel/    # MVVM bridge
    │   ├── Services/     # Business logic & save strategies
    │   └── Languages/    # Translation files (en.json, fr.json)
    └── docs/             # User manual, support & release notes

## Architecture

EasySave follows the **MVVM** pattern for a clear separation between UI and business logic.
It uses the **Strategy** pattern for:
- Backup execution (`SaveComplete` / `SaveDifferential`)
- Logging (`LogDaily` / `LogLive`)

The `EasyLog` logging library is packaged as a **separate DLL**, designed to be reused in future ProSoft projects.

## Documentation

- [User Manual](docs/USER_MANUAL.md) — how to use EasySave
- [Customer Support](docs/SUPPORT.md) — file locations and technical information
- [Release Notes](docs/RELEASE_NOTES.md) — version history

## Authors

**CESI — Software Engineering Team**
- Baptiste
- Adrien
- Kylian
- Ackey

## License

Internal ProSoft project — all rights reserved.