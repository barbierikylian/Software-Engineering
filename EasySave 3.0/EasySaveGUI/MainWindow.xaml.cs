using EasyLog;
using EasySave.Model;
using EasySave.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EasySaveGUI
{
    public class JobProgressInfo : INotifyPropertyChanged
    {
        private string _name;
        private int _progress;
        private string _currentFile;
        private string _status;
        private string _playPauseIcon = "⏸";
        private string _playPauseToolTip;
        private string _stopToolTip;
        private bool _isPlayPauseEnabled = true;

        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }
        public int Progress
        {
            get { return _progress; }
            set { _progress = value; OnPropertyChanged("Progress"); }
        }
        public string CurrentFile
        {
            get { return _currentFile; }
            set { _currentFile = value; OnPropertyChanged("CurrentFile"); }
        }
        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); }
        }
        public string PlayPauseIcon
        {
            get { return _playPauseIcon; }
            set { _playPauseIcon = value; OnPropertyChanged("PlayPauseIcon"); }
        }
        public string PlayPauseToolTip
        {
            get { return _playPauseToolTip; }
            set { _playPauseToolTip = value; OnPropertyChanged("PlayPauseToolTip"); }
        }
        public string StopToolTip
        {
            get { return _stopToolTip; }
            set { _stopToolTip = value; OnPropertyChanged("StopToolTip"); }
        }
        public bool IsPlayPauseEnabled
        {
            get { return _isPlayPauseEnabled; }
            set { _isPlayPauseEnabled = value; OnPropertyChanged("IsPlayPauseEnabled"); }
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

            UpdateUserInfo();
            if (_saveVM != null && TxtServerUrl != null)
            {
                _saveVM.SetServerUrl(TxtServerUrl.Text);
            }

            CmbLanguage.SelectedIndex = 0;
            RefreshGrid();
            UpdateLogPath();
        }

        private void UpdateUserInfo()
        {
            string finalName = TxtUserName.Text;
            if (string.IsNullOrWhiteSpace(finalName))
            {
                finalName = Environment.UserName;
            }
            _saveVM.SetLogUserName(finalName);
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
                LblDataLocation.Text = _langVM.GetString("label_data_location");
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
                LblPriorityExt.Text = _langVM.GetString("label_priority_ext");

                if (LblUserName != null) LblUserName.Text = _langVM.GetString("label_username");
                if (TxtUserName != null) TxtUserName.ToolTip = _langVM.GetString("tooltip_username");
                if (LblMaxFileSize != null) LblMaxFileSize.Text = _langVM.GetString("label_max_file_size");
                if (LblLogDestination != null) LblLogDestination.Text = _langVM.GetString("label_log_destination");
                if (LblServerUrl != null) LblServerUrl.Text = _langVM.GetString("label_server_url");

                if (RbLogLocal != null) RbLogLocal.Content = _langVM.GetString("radio_local");
                if (RbLogServer != null) RbLogServer.Content = _langVM.GetString("radio_server");
                if (RbLogBoth != null) RbLogBoth.Content = _langVM.GetString("radio_both");

                if (CbiTypeFull != null) CbiTypeFull.Content = _langVM.GetString("type_full");
                if (CbiTypeDiff != null) CbiTypeDiff.Content = _langVM.GetString("type_diff");

                string closeToolTip = _langVM.GetString("tooltip_close");
                if (BtnCloseBannerError != null) BtnCloseBannerError.ToolTip = closeToolTip;
                if (BtnCloseBannerSuccess != null) BtnCloseBannerSuccess.ToolTip = closeToolTip;
                if (BtnCloseBannerExecError != null) BtnCloseBannerExecError.ToolTip = closeToolTip;
                if (BtnCloseBannerExecSuccess != null) BtnCloseBannerExecSuccess.ToolTip = closeToolTip;
                if (BtnCloseBannerHint != null) BtnCloseBannerHint.ToolTip = closeToolTip;
            }
            catch { }
        }

        private void CmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = CmbLanguage.SelectedItem as ComboBoxItem;
            if (selectedItem != null && _langVM != null)
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
            string format = "json";
            if (RbXml.IsChecked == true) format = "xml";
            _saveVM.SetLogFormat(format);
        }

        private void OpenLogsFolder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "Logs");
            if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            Process.Start("explorer.exe", path);
        }

        private void OpenDataFolder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "data");
            if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
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

        private void TxtUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUserInfo();
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
            if (valid == false) return;

            if (_saveVM.CanCreateNewJob() == false)
            {
                ShowFormError(_langVM.GetString("error_max_jobs"));
                return;
            }

            string name = TxtName.Text.Trim();
            ComboBoxItem item = (ComboBoxItem)CmbType.SelectedItem;
            string selectedType = item.Tag.ToString();

            _saveVM.CreateJob(name, TxtSource.Text.Trim(), TxtTarget.Text.Trim(), selectedType);
            RefreshGrid();

            string successMsg = _langVM.GetString("success_create");
            if (successMsg != null) successMsg = successMsg.Replace("{name}", name);
            ShowFormSuccess(successMsg);

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
                Backup job = item as Backup;
                if (job != null) selectedJobs.Add(job);
            }

            foreach (Backup job in selectedJobs)
            {
                _saveVM.DeleteJob(job.Name);
            }

            RefreshGrid();

            string successMsg = _langVM.GetString("success_delete");
            if (successMsg != null) successMsg = successMsg.Replace("{name}", selectedJobs.Count.ToString() + " job(s)");
            ShowExecSuccess(successMsg);
        }

        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                string jobName = btn.CommandParameter as string;
                if (jobName != null)
                {
                    foreach (JobProgressInfo job in ActiveJobs)
                    {
                        if (job.Name == jobName)
                        {
                            if (job.PlayPauseIcon == "⏸")
                            {
                                _saveVM.PauseJob(jobName);
                                job.Status = _langVM.GetString("status_paused");
                                job.PlayPauseIcon = "▶";
                                job.PlayPauseToolTip = _langVM.GetString("tooltip_play");
                            }
                            else
                            {
                                _saveVM.ResumeJob(jobName);
                                job.Status = _langVM.GetString("status_running");
                                job.PlayPauseIcon = "⏸";
                                job.PlayPauseToolTip = _langVM.GetString("tooltip_pause");
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                string jobName = btn.CommandParameter as string;
                if (jobName != null)
                {
                    _saveVM.StopJob(jobName);
                    JobProgressInfo jobToRemove = null;
                    foreach (JobProgressInfo job in ActiveJobs)
                    {
                        if (job.Name == jobName)
                        {
                            jobToRemove = job;
                            break;
                        }
                    }
                    if (jobToRemove != null)
                    {
                        ActiveJobs.Remove(jobToRemove);
                    }
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
            string priorityExt = TxtPriorityExt.Text.Trim();

            long maxFileSizeBytes = 52428800;
            long parsedKb;
            if (long.TryParse(TxtMaxFileSize.Text.Trim(), out parsedKb))
            {
                maxFileSizeBytes = parsedKb * 1024;
            }

            List<Task> tasks = new List<Task>();

            foreach (Backup job in jobsToRun)
            {
                JobProgressInfo jobUI = new JobProgressInfo();
                jobUI.Name = job.Name;
                jobUI.Status = _langVM.GetString("status_running");
                jobUI.Progress = 0;
                jobUI.CurrentFile = _langVM.GetString("file_starting");
                jobUI.PlayPauseIcon = "⏸";
                jobUI.PlayPauseToolTip = _langVM.GetString("tooltip_pause");
                jobUI.StopToolTip = _langVM.GetString("tooltip_stop");
                jobUI.IsPlayPauseEnabled = true;

                ActiveJobs.Add(jobUI);

                Progress<int> progressObj = new Progress<int>(p => jobUI.Progress = p);

                Action<string> updateTextObj = text => Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (ActiveJobs.Contains(jobUI) == false) return;

                    string translatedText = text;
                    translatedText = translatedText.Replace("Copying:", _langVM.GetString("text_copying"));
                    translatedText = translatedText.Replace("Updating:", _langVM.GetString("text_updating"));
                    translatedText = translatedText.Replace("⏸ Blocked by process:", _langVM.GetString("text_blocked_by"));

                    jobUI.CurrentFile = translatedText;

                    if (text.StartsWith("⏸"))
                    {
                        jobUI.Status = _langVM.GetString("status_blocked");
                        jobUI.IsPlayPauseEnabled = false;
                    }
                    else if (jobUI.IsPlayPauseEnabled == false && jobUI.PlayPauseIcon == "⏸" && text.StartsWith("⏸") == false)
                    {
                        jobUI.Status = _langVM.GetString("status_running");
                        jobUI.IsPlayPauseEnabled = true;
                    }
                }));

                Task task = Task.Run(async () =>
                {
                    string error = await _saveVM.PerformJobsAsync(job.Name, businessSoft, encryptedExt, priorityExt, maxFileSizeBytes, progressObj, updateTextObj);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (ActiveJobs.Contains(jobUI) == false) return;

                        if (string.IsNullOrEmpty(error) == false)
                        {
                            if (error == "Job stopped.")
                            {
                                ActiveJobs.Remove(jobUI);
                            }
                            else
                            {
                                if (error.StartsWith("Error: Source directory does not exist."))
                                {
                                    error = _langVM.GetString("error_source_missing");
                                }

                                jobUI.Status = _langVM.GetString("status_error");
                                jobUI.CurrentFile = error;
                                jobUI.IsPlayPauseEnabled = false;

                                if (BannerExecError.Visibility == Visibility.Visible)
                                {
                                    TxtBannerExecError.Text += "\n[" + job.Name + "] " + error;
                                }
                                else
                                {
                                    ShowExecError("[" + job.Name + "] " + error);
                                }
                            }
                        }
                        else
                        {
                            jobUI.Status = _langVM.GetString("status_finished");
                            jobUI.CurrentFile = _langVM.GetString("status_finished");
                            jobUI.Progress = 100;
                            jobUI.IsPlayPauseEnabled = false;
                        }
                    });
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            SetButtonsEnabled(true);

            if (BannerExecError.Visibility != Visibility.Visible)
            {
                ShowExecSuccess(_langVM.GetString("success_all_tasks"));
            }
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
                Backup job = item as Backup;
                if (job != null) selectedJobs.Add(job);
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
            {
                ClearExecBanners();
            }
        }

        private void RbLogDestination_Checked(object sender, RoutedEventArgs e)
        {
            if (_saveVM == null) return;
            string destination = "Both";
            if (RbLogLocal.IsChecked == true) destination = "Local";
            else if (RbLogServer.IsChecked == true) destination = "Centralized";

            _saveVM.SetLogDestination(destination);
        }

        private void TxtServerUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_saveVM != null && TxtServerUrl != null)
            {
                _saveVM.SetServerUrl(TxtServerUrl.Text);
            }
        }
    }
}