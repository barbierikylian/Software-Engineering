using EasySave.Model;
using EasySave.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace EasySave.ViewModel
{
    public class SaveViewModel : INotifyPropertyChanged
    {
        private BackupService _backupService;

        public ObservableCollection<SelectableBackup> Jobs { get; set; }
        public ObservableCollection<JobProgressInfo> ActiveJobs { get; set; }

        private string _inputName;
        public string InputName
        {
            get
            {
                return _inputName;
            }
            set
            {
                _inputName = value;
                OnPropertyChanged("InputName");
            }
        }

        private string _inputSource;
        public string InputSource
        {
            get
            {
                return _inputSource;
            }
            set
            {
                _inputSource = value;
                OnPropertyChanged("InputSource");
            }
        }

        private string _inputTarget;
        public string InputTarget
        {
            get
            {
                return _inputTarget;
            }
            set
            {
                _inputTarget = value;
                OnPropertyChanged("InputTarget");
            }
        }

        private string _inputType = "Full";
        public string InputType
        {
            get
            {
                return _inputType;
            }
            set
            {
                _inputType = value;
                OnPropertyChanged("InputType");
            }
        }

        private string _businessSoft = "CalculatorApp";
        public string BusinessSoft
        {
            get
            {
                return _businessSoft;
            }
            set
            {
                _businessSoft = value;
                OnPropertyChanged("BusinessSoft");
            }
        }

        private string _encryptedExt = ".txt;.pdf";
        public string EncryptedExt
        {
            get
            {
                return _encryptedExt;
            }
            set
            {
                _encryptedExt = value;
                OnPropertyChanged("EncryptedExt");
            }
        }

        private string _priorityExt = ".iso;.png";
        public string PriorityExt
        {
            get
            {
                return _priorityExt;
            }
            set
            {
                _priorityExt = value;
                OnPropertyChanged("PriorityExt");
            }
        }

        private string _maxFileSize = "50000";
        public string MaxFileSize
        {
            get
            {
                return _maxFileSize;
            }
            set
            {
                _maxFileSize = value;
                OnPropertyChanged("MaxFileSize");
            }
        }

        private string _serverUrl;
        public string ServerUrl
        {
            get
            {
                return _serverUrl;
            }
            set
            {
                _serverUrl = value;
                _backupService.SetServerUrl(value);
                OnPropertyChanged("ServerUrl");
            }
        }

        private string _userName = Environment.UserName;
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;

                if (string.IsNullOrWhiteSpace(value))
                {
                    _backupService.SetLogUserName(Environment.UserName);
                }
                else
                {
                    _backupService.SetLogUserName(value);
                }

                OnPropertyChanged("UserName");
            }
        }

        private bool _isJson = true;
        public bool IsJson
        {
            get
            {
                return _isJson;
            }
            set
            {
                _isJson = value;

                if (value == true)
                {
                    _backupService.SetLogFormat("json");
                }

                OnPropertyChanged("IsJson");
            }
        }

        private bool _isXml;
        public bool IsXml
        {
            get
            {
                return _isXml;
            }
            set
            {
                _isXml = value;

                if (value == true)
                {
                    _backupService.SetLogFormat("xml");
                }

                OnPropertyChanged("IsXml");
            }
        }

        private bool _isLogLocal;
        public bool IsLogLocal
        {
            get
            {
                return _isLogLocal;
            }
            set
            {
                _isLogLocal = value;

                if (value == true)
                {
                    _backupService.SetLogDestination("Local");
                }

                OnPropertyChanged("IsLogLocal");
            }
        }

        private bool _isLogServer;
        public bool IsLogServer
        {
            get
            {
                return _isLogServer;
            }
            set
            {
                _isLogServer = value;

                if (value == true)
                {
                    _backupService.SetLogDestination("Centralized");
                }

                OnPropertyChanged("IsLogServer");
            }
        }

        private bool _isLogBoth = true;
        public bool IsLogBoth
        {
            get
            {
                return _isLogBoth;
            }
            set
            {
                _isLogBoth = value;

                if (value == true)
                {
                    _backupService.SetLogDestination("Both");
                }

                OnPropertyChanged("IsLogBoth");
            }
        }

        private bool _isButtonsEnabled = true;
        public bool IsButtonsEnabled
        {
            get
            {
                return _isButtonsEnabled;
            }
            set
            {
                _isButtonsEnabled = value;
                OnPropertyChanged("IsButtonsEnabled");
            }
        }

        private string _bannerErrorText;
        public string BannerErrorText
        {
            get
            {
                return _bannerErrorText;
            }
            set
            {
                _bannerErrorText = value;
                OnPropertyChanged("BannerErrorText");
            }
        }

        private bool _isBannerErrorVisible;
        public bool IsBannerErrorVisible
        {
            get
            {
                return _isBannerErrorVisible;
            }
            set
            {
                _isBannerErrorVisible = value;
                OnPropertyChanged("IsBannerErrorVisible");
            }
        }

        private string _bannerSuccessText;
        public string BannerSuccessText
        {
            get
            {
                return _bannerSuccessText;
            }
            set
            {
                _bannerSuccessText = value;
                OnPropertyChanged("BannerSuccessText");
            }
        }

        private bool _isBannerSuccessVisible;
        public bool IsBannerSuccessVisible
        {
            get
            {
                return _isBannerSuccessVisible;
            }
            set
            {
                _isBannerSuccessVisible = value;
                OnPropertyChanged("IsBannerSuccessVisible");
            }
        }

        public RelayCommand AddJobCommand { get; set; }
        public RelayCommand DeleteJobCommand { get; set; }
        public RelayCommand ExecuteCommand { get; set; }
        public RelayCommand ExecuteAllCommand { get; set; }
        public RelayCommand PlayPauseCommand { get; set; }
        public RelayCommand StopCommand { get; set; }
        public RelayCommand CloseBannersCommand { get; set; }
        public RelayCommand OpenStateLogsCommand { get; set; }
        public RelayCommand OpenDailyLogsCommand { get; set; }
        public RelayCommand RemoveActiveJobCommand { get; set; }

        public SaveViewModel()
        {
            _backupService = new BackupService();
            Jobs = new ObservableCollection<SelectableBackup>();
            ActiveJobs = new ObservableCollection<JobProgressInfo>();

            AddJobCommand = new RelayCommand(AddJob);
            DeleteJobCommand = new RelayCommand(DeleteJob);
            ExecuteCommand = new RelayCommand(ExecuteSelected);
            ExecuteAllCommand = new RelayCommand(ExecuteAll);
            PlayPauseCommand = new RelayCommand(PlayPauseJob);
            StopCommand = new RelayCommand(StopJob);
            CloseBannersCommand = new RelayCommand(CloseBanners);
            OpenStateLogsCommand = new RelayCommand(OpenStateLogs);
            OpenDailyLogsCommand = new RelayCommand(OpenDailyLogs);
            RemoveActiveJobCommand = new RelayCommand(RemoveActiveJob);

            RefreshGrid();
            _backupService.SetLogUserName(Environment.UserName);
            _backupService.SetServerUrl("http://localhost:8080/api/logs");
        }

        private void RefreshGrid()
        {
            Jobs.Clear();
            foreach (Backup job in _backupService.GetAllJobs())
            {
                Jobs.Add(new SelectableBackup(job));
            }
        }

        private void AddJob(object parameter)
        {
            IsBannerErrorVisible = false;
            IsBannerSuccessVisible = false;

            if (string.IsNullOrWhiteSpace(InputName) || string.IsNullOrWhiteSpace(InputSource) || string.IsNullOrWhiteSpace(InputTarget))
            {
                LanguageViewModel langVM = (LanguageViewModel)System.Windows.Application.Current.MainWindow.FindResource("LangVM");
                BannerErrorText = langVM["err_fields_missing"];
                IsBannerErrorVisible = true;
                return;
            }

            if (_backupService.CanCreateJob() == false)
            {
                LanguageViewModel langVM = (LanguageViewModel)System.Windows.Application.Current.MainWindow.FindResource("LangVM");
                BannerErrorText = langVM["err_max_jobs"];
                IsBannerErrorVisible = true;
                return;
            }

            Backup newJob = new Backup { Name = InputName.Trim(), FileSource = InputSource.Trim(), FileDestination = InputTarget.Trim(), Type = InputType };
            
            try
            {
                _backupService.CreateJob(newJob);

                RefreshGrid();
                LanguageViewModel langVM = (LanguageViewModel)System.Windows.Application.Current.MainWindow.FindResource("LangVM");
                BannerSuccessText = langVM["succ_job_created"];
                IsBannerSuccessVisible = true;

                InputName = string.Empty;
                InputSource = string.Empty;
                InputTarget = string.Empty;
            }
            catch (Exception ex)
            {
                BannerErrorText = ex.Message;
                IsBannerErrorVisible = true;
            }
        }

        private void DeleteJob(object parameter)
        {
            List<SelectableBackup> toDelete = new List<SelectableBackup>();
            foreach (SelectableBackup item in Jobs)
            {
                if (item.IsSelected) toDelete.Add(item);
            }

            try
            {
                foreach (SelectableBackup item in toDelete)
                {
                    _backupService.DeleteJob(item.Job);
                }

                RefreshGrid();
                LanguageViewModel langVM = (LanguageViewModel)System.Windows.Application.Current.MainWindow.FindResource("LangVM");
                BannerSuccessText = langVM["succ_job_deleted"];
                IsBannerSuccessVisible = true;
            }
            catch (Exception ex)
            {
                BannerErrorText = ex.Message;
                IsBannerErrorVisible = true;
            }
        }

        private void ExecuteSelected(object parameter)
        {
            List<Backup> selectedJobs = new List<Backup>();
            foreach (SelectableBackup item in Jobs)
            {
                if (item.IsSelected) selectedJobs.Add(item.Job);
            }

            if (selectedJobs.Count > 0) RunJobsInParallel(selectedJobs);
        }

        private void ExecuteAll(object parameter)
        {
            List<Backup> allJobs = _backupService.GetAllJobs();
            if (allJobs.Count > 0) RunJobsInParallel(allJobs);
        }

        private async void RunJobsInParallel(List<Backup> jobsToRun)
        {
            ActiveJobs.Clear();
            IsButtonsEnabled = false;

            long maxFileSizeBytes = 52428800;
            long parsedKb;
            if (long.TryParse(MaxFileSize, out parsedKb)) maxFileSizeBytes = parsedKb * 1024;

            List<Task> tasks = new List<Task>();

            foreach (Backup job in jobsToRun)
            {
                JobProgressInfo jobUI = new JobProgressInfo();
                jobUI.Name = job.Name;
                LanguageViewModel langVM = (LanguageViewModel)System.Windows.Application.Current.MainWindow.FindResource("LangVM");
                jobUI.Status = langVM["status_running"];
                jobUI.Progress = 0;
                jobUI.CurrentFile = "Starting...";
                jobUI.PlayPauseIcon = "⏸";
                jobUI.IsPlayPauseEnabled = true;
                jobUI.IsRemoveVisible = false;
                jobUI.IsActionVisible = true;

                ActiveJobs.Add(jobUI);

                IProgress<int> progressObj = new Progress<int>(p => jobUI.Progress = p);
                IProgress<string> textProgress = new Progress<string>(text =>
                {
                    if (ActiveJobs.Contains(jobUI) == false) return;
                    jobUI.CurrentFile = text;

                    LanguageViewModel langVM = (LanguageViewModel)System.Windows.Application.Current.MainWindow.FindResource("LangVM");
                    
                    if (text.StartsWith("⏸"))
                    {
                        jobUI.Status = langVM["status_blocked"];
                        jobUI.IsPlayPauseEnabled = false;
                    }
                    else if (jobUI.IsPlayPauseEnabled == false && jobUI.PlayPauseIcon == "⏸" && text.StartsWith("⏸") == false)
                    {
                        jobUI.Status = langVM["status_running"];
                        jobUI.IsPlayPauseEnabled = true;
                    }

                    if (text == "Job stopped.")
                    {
                        jobUI.Status = langVM["status_stopped"];
                        jobUI.IsPlayPauseEnabled = false;
                        jobUI.IsRemoveVisible = true;
                        jobUI.IsActionVisible = false;
                    }
                    else if (text.StartsWith("Error"))
                    {
                        jobUI.Status = langVM["status_error"];
                        jobUI.IsPlayPauseEnabled = false;
                        jobUI.IsRemoveVisible = true;
                        jobUI.IsActionVisible = false;
                    }
                    else if (text == "Finished")
                    {
                        jobUI.Status = langVM["status_finished"];
                        jobUI.Progress = 100;
                        jobUI.IsPlayPauseEnabled = false;
                        jobUI.IsRemoveVisible = true;
                        jobUI.IsActionVisible = false;
                    }
                });

                Action<string> reportTextAction = new Action<string>(textProgress.Report);

                Task task = Task.Run(async () =>
                {
                    string error = await _backupService.PerformJobsAsync(job, BusinessSoft, EncryptedExt, PriorityExt, maxFileSizeBytes, progressObj, reportTextAction);
                    
                    if (string.IsNullOrEmpty(error) == false)
                    {
                        reportTextAction(error);
                    }
                    else
                    {
                        reportTextAction("Finished");
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            IsButtonsEnabled = true;
        }

        private void PlayPauseJob(object parameter)
        {
            string jobName = parameter as string;
            if (string.IsNullOrEmpty(jobName)) return;

            foreach (JobProgressInfo job in ActiveJobs)
            {
                if (job.Name == jobName)
                {
                    if (job.PlayPauseIcon == "⏸")
                    {
                        _backupService.PauseJob(jobName);
                        job.Status = "Paused";
                        job.PlayPauseIcon = "▶";
                    }
                    else
                    {
                        _backupService.ResumeJob(jobName);
                        job.Status = "Running";
                        job.PlayPauseIcon = "⏸";
                    }
                    break;
                }
            }
        }

        private void StopJob(object parameter)
        {
            string jobName = parameter as string;
            if (string.IsNullOrEmpty(jobName) == false) _backupService.StopJob(jobName);
        }

        private void CloseBanners(object parameter)
        {
            IsBannerErrorVisible = false;
            IsBannerSuccessVisible = false;
        }

        private void RemoveActiveJob(object parameter)
        {
            string jobName = parameter as string;
            if (!string.IsNullOrEmpty(jobName))
            {
                JobProgressInfo jobUI = ActiveJobs.FirstOrDefault(j => j.Name == jobName);
                if (jobUI != null)
                {
                    ActiveJobs.Remove(jobUI);
                    _backupService.RemoveFromStateLog(jobName);
                }
            }
        }

        private void OpenStateLogs(object parameter)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dataDir = System.IO.Path.Combine(appData, "EasySave", "data");
            if (System.IO.Directory.Exists(dataDir))
            {
                System.Diagnostics.Process.Start("explorer.exe", dataDir);
            }
        }

        private void OpenDailyLogs(object parameter)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string logDir = System.IO.Path.Combine(appData, "EasySave", "logs");
            if (System.IO.Directory.Exists(logDir))
            {
                System.Diagnostics.Process.Start("explorer.exe", logDir);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}