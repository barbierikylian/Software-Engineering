# EasySave — Release Notes

## Version 1.0 — April 2026

First public release of **EasySave**, a console backup software developed for ProSoft.

### Features

- Console application based on **.NET**
- Support for **up to 5 backup jobs**
- Two backup strategies:
  - **Full** — copies every file from source to destination
  - **Differential** — copies only files that are new or have been modified
- **Real-time progression** updated after each file copied, written to `state.json`
- **Daily log** in JSON format, one file per day (`YYYY-MM-DD.json`)
- **Bilingual interface** (English / French) with instant language switching
- **Multi-job execution** from the menu (e.g. `1;3;4`, `1,3` or `1 3`)
- Support for **local, external and network drives** (UNC paths)

### Architecture

- **MVVM** pattern for clear separation between UI and business logic
- **Strategy** pattern for backup execution and logging
- **EasyLog** packaged as a **separate DLL**, reusable in future ProSoft projects

### File Storage

All application data is stored under `%AppData%\EasySave\`, following Windows best practices.

### Known Limitations

- Backup jobs are executed **sequentially**, not in parallel
- The `state.json` file holds the state of a **single active job** at a time
- Command-line arguments (e.g. `EasySave.exe 1-3`) are **not yet supported**
- Only **English** and **French** languages are currently available

### Credits

CESI — Software Engineering Team
- Baptiste
- Adrien
- Kylian
- Ackey