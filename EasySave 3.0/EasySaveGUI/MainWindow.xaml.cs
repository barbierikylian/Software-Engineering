using EasySave.Model;
using EasySave.ViewModel;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.TextFormatting;
using System.Xml.Linq;

namespace EasySaveGUI
{
    public class JobProgressInfo : INotifyPropertyChanged
    {
        private string _name;
        private int _progress;
        private string _currentFile;
        private string _status;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }
        public int Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(nameof(Progress)); }
        }
        public string CurrentFile
        {
            get => _currentFile;
            set { _currentFile = value; OnPropertyChanged(nameof(CurrentFile)); }
        }
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public partial class MainWindow : Window
    {
        private SaveViewModel _saveVM;
        private LanguageViewModel _langVM;

        public ObservableCollection<JobProgressInfo> ActiveJobs { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _saveVM = new SaveViewModel();
            _langVM = new LanguageViewModel();
            ActiveJobs = new ObservableCollection<JobProgressInfo>();

            ActiveJobsControl.ItemsSource = ActiveJobs;

            CmbLanguage.SelectedIndex = 0;
            RefreshGrid();
            UpdateLogPath();
        }

        private void UpdateLanguageUI()
        {
            if (_langVM == null) return;
            try
            {
                LblCreateTitle.Text = _langVM.GetString("create_title");
                LblListTitle.Text = _langVM.GetString("list_title");
                LblName.Text = _langVM.GetString("label_name");
                LblSource.Text = _langVM.GetString("label_source");
                LblTarget.Text = _langVM.GetString("label_dest");
                LblType.Text = _langVM.GetString("label_type");
                LblSettings.Text = _langVM.GetString("label_settings");
                LblBusinessSoft.Text = _langVM.GetString("label_business_soft");
                LblLogsTitle.Text = _langVM.GetString("label_logs_title");
                LblLogFormat.Text = _langVM.GetString("label_log_format");
                LblLogLocation.Text = _langVM.GetString("label_log_location");
                BtnAdd.Content = _langVM.GetString("menu_create");
                BtnDelete.Content = _langVM.GetString("menu_delete");
                BtnExecute.Content = _langVM.GetString("menu_execute");
                BtnExecuteAll.Content = _langVM.GetString("hint_all");
                TxtBannerHint.Text = _langVM.GetString("hint_ctrl_click");
                LblHintMultipleApps.Text = _langVM.GetString("hint_multiple_apps");
                ColName.Header = _langVM.GetString("label_name");
                ColSource.Header = _langVM.GetString("label_source");
                ColTarget.Header = _langVM.GetString("label_dest");
                ColType.Header = _langVM.GetString("label_type");
                LblPermanentHint.Text = _langVM.GetString("hint_ctrl_click");
                LblEncryptedExt.Text = _langVM.GetString("label_encrypted_ext");
            }
            catch { }
        }

        private void CmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbLanguage.SelectedItem is ComboBoxItem selectedItem && _langVM != null)
            {
                _langVM.UpdateLanguage(selectedItem.Tag.ToString());
                UpdateLanguageUI();
            }
        }

        private void RefreshGrid()
        {
            GridJobs.ItemsSource = null;
            GridJobs.ItemsSource = _saveVM.GetAllJobs();
        }

        private void UpdateLogPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            TxtLogPath.Text = Path.Combine(appData, "EasySave", "Logs");
            TxtDataPath.Text = Path.Combine(appData, "EasySave", "data");
        }

        private void RbLogFormat_Checked(object sender, RoutedEventArgs e)
        {
            if (_saveVM == null) return;
            string format = RbJson.IsChecked == true ? "json" : "xml";
            _saveVM.SetLogFormat(format);
        }

        private void OpenLogsFolder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "Logs");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            Process.Start("explorer.exe", path);
        }

        private void OpenDataFolder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "data");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            Process.Start("explorer.exe", path);
        }

        private void CloseBanners_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ClearExecBanners();
            ClearFormBanners();
        }

        private void ClearFormBanners()
        {
            BannerError.Visibility = Visibility.Collapsed;
            BannerSuccess.Visibility = Visibility.Collapsed;
            ErrName.Visibility = Visibility.Collapsed;
            ErrSource.Visibility = Visibility.Collapsed;
            ErrTarget.Visibility = Visibility.Collapsed;
        }

        private void ClearExecBanners()
        {
            BannerExecError.Visibility = Visibility.Collapsed;
            BannerExecSuccess.Visibility = Visibility.Collapsed;
            BannerHint.Visibility = Visibility.Collapsed;
        }

        private void ShowFormError(string message)
        {
            ClearFormBanners();
            TxtBannerError.Text = message;
            BannerError.Visibility = Visibility.Visible;
        }

        private void ShowFormSuccess(string message)
        {
            ClearFormBanners();
            TxtBannerSuccess.Text = message;
            BannerSuccess.Visibility = Visibility.Visible;
        }

        private void ShowFieldError(TextBlock errLabel, string message)
        {
            errLabel.Text = message;
            errLabel.Visibility = Visibility.Visible;
        }

        private void ShowExecError(string message)
        {
            ClearExecBanners();
            TxtBannerExecError.Text = message;
            BannerExecError.Visibility = Visibility.Visible;
        }

        private void ShowExecSuccess(string message)
        {
            ClearExecBanners();
            TxtBannerExecSuccess.Text = message;
            BannerExecSuccess.Visibility = Visibility.Visible;
        }

        private void ShowHintBanner()
        {
            ClearExecBanners();
            TxtBannerHint.Text = _langVM.GetString("hint_ctrl_click");
            BannerHint.Visibility = Visibility.Visible;
        }

        private void SetButtonsEnabled(bool enabled)
        {
            BtnExecute.IsEnabled = enabled;
            BtnExecuteAll.IsEnabled = enabled;
            BtnAdd.IsEnabled = enabled;
            BtnDelete.IsEnabled = enabled;
            RbJson.IsEnabled = enabled;
            RbXml.IsEnabled = enabled;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ClearFormBanners();
            bool valid = true;

            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                ShowFieldError(ErrName, _langVM.GetString("error_name_required"));
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(TxtSource.Text))
            {
                ShowFieldError(ErrSource, _langVM.GetString("error_source_required"));
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(TxtTarget.Text))
            {
                ShowFieldError(ErrTarget, _langVM.GetString("error_dest_required"));
                valid = false;
            }
            if (!valid) return;

            if (!_saveVM.CanCreateNewJob())
            {
                ShowFormError(_langVM.GetString("error_max_jobs"));
                return;
            }

            string name = TxtName.Text.Trim();
            string selectedType = ((ComboBoxItem)CmbType.SelectedItem).Tag.ToString();

            _saveVM.CreateJob(name, TxtSource.Text.Trim(), TxtTarget.Text.Trim(), selectedType);
            RefreshGrid();
            ShowFormSuccess(_langVM.GetString("success_create")?.Replace("{name}", name));
            TxtName.Clear();
            TxtSource.Clear();
            TxtTarget.Clear();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (GridJobs.SelectedItems.Count == 0)
            {
                ShowExecError(_langVM.GetString("error_select_job"));
                return;
            }

            List<Backup> selectedJobs = new List<Backup>();
            foreach (object item in GridJobs.SelectedItems)
            {
                if (item is Backup job) selectedJobs.Add(job);
            }

            foreach (Backup job in selectedJobs)
            {
                _saveVM.DeleteJob(job.Name);
            }

            RefreshGrid();
            ShowExecSuccess(_langVM.GetString("success_delete")?.Replace("{name}", $"{selectedJobs.Count} job(s)"));
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string jobName)
            {
                _saveVM.PauseJob(jobName);
                UpdateJobStatus(jobName, "Paused");
            }
        }

        private void BtnResume_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string jobName)
            {
                _saveVM.ResumeJob(jobName);
                UpdateJobStatus(jobName, "Running");
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string jobName)
            {
                _saveVM.StopJob(jobName);
                UpdateJobStatus(jobName, "Stopping...");
            }
        }

        private void UpdateJobStatus(string jobName, string status)
        {
            foreach (var job in ActiveJobs)
            {
                if (job.Name == jobName)
                {
                    job.Status = status;
                    break;
                }
            }
        }

        private async void RunJobsInParallel(List<Backup> jobsToRun)
        {
            ActiveJobs.Clear();
            ClearExecBanners();
            SetButtonsEnabled(false);

            string businessSoft = TxtBusinessSoft.Text.Trim();
            string encryptedExt = TxtEncryptedExt.Text.Trim();
            List<Task> tasks = new List<Task>();
            List<string> errorMessages = new List<string>();

            foreach (Backup job in jobsToRun)
            {
                JobProgressInfo jobUI = new JobProgressInfo
                {
                    Name = job.Name,
                    Status = "Running",
                    Progress = 0,
                    CurrentFile = "Starting backup..."
                };
                ActiveJobs.Add(jobUI);

                Progress<int> progressObj = new Progress<int>(p => jobUI.Progress = p);
                Action<string> updateTextObj = text => Application.Current.Dispatcher.Invoke(() => jobUI.CurrentFile = text);

                Task task = Task.Run(async () =>
                {
                    string error = await _saveVM.PerformJobsAsync(job.Name, businessSoft, encryptedExt, progressObj, updateTextObj);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            if (error == "Job stopped.") jobUI.Status = "Stopped";
                            else
                            {
                                jobUI.Status = "Error";
                                errorMessages.Add($"[{job.Name}] {error}");
                            }
                        }
                        else
                        {
                            jobUI.Status = "Finished";
                            jobUI.Progress = 100;
                        }
                    });
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            if (errorMessages.Count > 0)
            {
                ShowExecError(string.Join("\n", errorMessages));
            }
            else
            {
                ShowExecSuccess("All selected tasks have finished processing.");
            }

            SetButtonsEnabled(true);
        }

        private void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            if (GridJobs.SelectedItems.Count == 0)
            {
                ShowHintBanner();
                return;
            }

            List<Backup> selectedJobs = new List<Backup>();
            foreach (object item in GridJobs.SelectedItems)
            {
                if (item is Backup job) selectedJobs.Add(job);
            }

            RunJobsInParallel(selectedJobs);
        }

        private void BtnExecuteAll_Click(object sender, RoutedEventArgs e)
        {
            if (GridJobs.Items.Count == 0)
            {
                ShowExecError(_langVM.GetString("error_no_jobs_exec"));
                return;
            }

            List<Backup> allJobs = _saveVM.GetAllJobs();
            RunJobsInParallel(allJobs);
        }

        private void GridJobs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridJobs.SelectedItems.Count > 0)
                ClearExecBanners();
        }
    }
}