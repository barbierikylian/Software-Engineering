# EasySave — Customer Support

**Version 1.0** · ProSoft

This document is intended for customer support teams and system administrators. It describes where EasySave stores its files, the minimum system requirements, and common troubleshooting steps.

## 1. System Requirements

- **Operating system**: Windows 10 or later (Windows 11 recommended)
- **Runtime**: .NET Runtime installed
- **Disk space**: at least 50 MB free on the system drive for logs and configuration
- **Permissions**:
  - Read access on source directories
  - Write access on destination directories and on the user's `%AppData%` folder

## 2. Default File Locations

EasySave follows Windows best practices and never writes to sensitive folders such as `C:\Temp` or `Program Files`.

All files are stored under the current user's **`%AppData%`** folder (i.e. `C:\Users\<username>\AppData\Roaming\`):

| File                  | Full path                                            | Purpose                          |
|-----------------------|------------------------------------------------------|----------------------------------|
| `Listjobs.json`       | `%AppData%\EasySave\data\Listjobs.json`              | List of configured backup jobs   |
| `state.json`          | `%AppData%\EasySave\data\state.json`                 | Real-time status of running jobs |
| `YYYY-MM-DD.json`     | `%AppData%\EasySave\logs\YYYY-MM-DD.json`            | Daily log (one file per day)     |
| `en.json`, `fr.json`  | `<install folder>\Languages\`                        | Translation files                |

**Tip**: to open the `%AppData%\EasySave` folder quickly, press `Windows + R`, type `%AppData%\EasySave` and press Enter.

## 3. Log Files

- A **new log file is created every day**, named with the current date (`yyyy-MM-dd.json`).
- Each backup execution appends a JSON entry with:
  - Backup name
  - Source path (UNC format)
  - Destination path (UNC format)
  - File size
  - Transfer duration in milliseconds (**negative** value indicates an error)
  - Timestamp

Old log files are never deleted automatically. The system administrator can archive or remove them manually if needed.

## 4. Real-Time State File

The file `state.json` reflects the **current** state of the backup engine. It is **overwritten** after each file copy to provide up-to-date progression data to monitoring tools.

## 5. Supported Path Types

EasySave supports the following source and destination locations:

- Local drives (`C:\`, `D:\`, ...)
- External drives (USB, external SSD)
- Network drives and shared folders (UNC format: `\\Server\Share\Folder`)

## 6. Common Issues

| Issue                                   | Cause                                        | Solution                                              |
|-----------------------------------------|----------------------------------------------|-------------------------------------------------------|
| "Max jobs reached"                      | 5 jobs already configured                    | Delete a job (menu option 4) before creating a new one|
| "Language file not found"               | Invalid language code                        | Only `en` and `fr` are supported in version 1.0       |
| Backup starts but nothing is copied     | Source directory empty or does not exist     | Check the source path in option 3 (list jobs)         |
| Daily log not appearing                 | Missing write permissions on `%AppData%`     | Check Windows user permissions                        |
| Negative `fileTransferTime` in log      | An error occurred during the copy            | Check that source exists and destination is writable  |

## 7. Uninstallation

EasySave is a standalone executable. To fully remove it:

1. Delete the application folder (wherever `EasySave.exe` was installed)
2. Delete the user data folder: `%AppData%\EasySave\`

## 8. Contact

For technical support, please contact the ProSoft Software Engineering Team.

**Internal project — CESI**