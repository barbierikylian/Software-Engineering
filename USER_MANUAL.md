# EasySave — User Manual

**Version 1.0** · ProSoft

## 1. Starting the application

Launch **EasySave.exe**. The main menu is displayed in the console:

    ===================================
            Welcome to EasySave
    ===================================
    1. Create a backup job
    2. Execute a job
    3. List jobs
    4. Delete a job
    5. Change language
    6. Quit

Type the number of the option you want, then press **Enter**.

## 2. Creating a backup job

From the menu, choose **1** and fill in:

| Field       | Example                     |
|-------------|-----------------------------|
| Job name    | `DocumentsBackup`           |
| Source path | `C:\Users\Me\Documents`     |
| Destination | `D:\Backups\Docs`           |
| Type        | `Full` or `Differential`    |

- **Full**: copies every file from the source.
- **Differential**: copies only files that are new or have been modified.

Up to **5 jobs** can be configured. Type `exit` at any prompt to cancel.

## 3. Executing jobs

From the menu, choose **2**. The list of configured jobs is displayed, each with an index.

You can then:

- Press **Enter** → executes **all jobs** sequentially.
- Type `1;3;4` (or `1,3` or `1 3`) → executes the **selected jobs** only.
- Type `exit` → cancels and goes back to the menu.

While a job is running, the progression is displayed live in the console:

    Progress: 72% | Files left: 5

## 4. Listing and deleting jobs

- Choose **3** to list all configured jobs.
- Choose **4**, then enter the index of the job you want to remove.

## 5. Changing language

Choose **5** and enter the language code:

- `en` for English
- `fr` for French

## 6. Where are my files saved?

All application data is stored under your Windows user folder:

- Configured jobs → `%AppData%\EasySave\data\Listjobs.json`
- Real-time status → `%AppData%\EasySave\data\state.json`
- Daily log files  → `%AppData%\EasySave\logs\YYYY-MM-DD.json`

For more details, see the [Customer Support](SUPPORT.md) document.