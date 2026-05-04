using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using EasySave.ViewModel;
using EasySave.Model;

namespace EasySaveGUI
{
    public partial class MainWindow : Window
    {
        private SaveViewModel _saveVM;
        private LanguageViewModel _langVM;

        public MainWindow()
        {
            InitializeComponent();
            _saveVM = new SaveViewModel();
            _langVM = new LanguageViewModel();
            CmbLanguage.SelectedIndex = 0;
            RefreshGrid();
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
                BtnAdd.Content = _langVM.GetString("menu_create");
                BtnDelete.Content = _langVM.GetString("menu_delete");
                BtnExecute.Content = _langVM.GetString("menu_execute");
                BtnExecuteAll.Content = _langVM.GetString("hint_all");
                TxtBannerHint.Text = _langVM.GetString("error_select_job");
                LblHintMultipleApps.Text = _langVM.GetString("hint_multiple_apps");

                ColName.Header = _langVM.GetString("label_name");
                ColSource.Header = _langVM.GetString("label_source");
                ColTarget.Header = _langVM.GetString("label_dest");
                ColType.Header = _langVM.GetString("label_type");
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
            BannerRunning.Visibility = Visibility.Collapsed;
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

        private void ShowExecRunning(string message)
        {
            ClearExecBanners();
            TxtBannerRunning.Text = message;
            BannerRunning.Visibility = Visibility.Visible;
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
            TxtBannerHint.Text = _langVM.GetString("error_select_job");
            BannerHint.Visibility = Visibility.Visible;
        }

        private void SetCurrentFileLabel(string text)
        {
            LblCurrentFile.Inlines.Clear();

            if (text == "...")
            {
                LblCurrentFile.Inlines.Add(new Run("...")
                {
                    Foreground = new SolidColorBrush(Color.FromRgb(0x7d, 0x85, 0x90))
                });
            }
            else
            {
                LblCurrentFile.Inlines.Add(new Run(text)
                {
                    Foreground = new SolidColorBrush(Color.FromRgb(0x56, 0xd3, 0x64))
                });
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            BtnExecute.IsEnabled = enabled;
            BtnExecuteAll.IsEnabled = enabled;
            BtnAdd.IsEnabled = enabled;
            BtnDelete.IsEnabled = enabled;
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
            _saveVM.CreateJob(name, TxtSource.Text.Trim(), TxtTarget.Text.Trim(), CmbType.Text);
            RefreshGrid();

            ShowFormSuccess(_langVM.GetString("success_create")?.Replace("{name}", name));

            TxtName.Clear();
            TxtSource.Clear();
            TxtTarget.Clear();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (GridJobs.SelectedItem is not Backup selectedJob)
            {
                ShowExecError(_langVM.GetString("error_select_job"));
                return;
            }

            _saveVM.DeleteJob(selectedJob.Name);
            RefreshGrid();
            ShowExecSuccess(_langVM.GetString("success_delete")?.Replace("{name}", selectedJob.Name));
        }

        private async void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            if (GridJobs.SelectedItem is not Backup selectedJob)
            {
                ShowHintBanner();
                return;
            }

            var progress = new Progress<int>(p => ProgBar.Value = p);

            Action<string> updateText = text =>
                Application.Current.Dispatcher.Invoke(() => SetCurrentFileLabel(text));

            ShowExecRunning(_langVM.GetString("executing_single")?.Replace("{name}", selectedJob.Name));
            SetButtonsEnabled(false);

            try
            {
                string errorMessage = await Task.Run(() => _saveVM.PerformJobs(selectedJob.Name, progress, updateText));

                if (string.IsNullOrEmpty(errorMessage))
                {
                    ShowExecSuccess(_langVM.GetString("execution_finished"));
                }
                else
                {
                    ShowExecError(errorMessage);
                }
            }
            catch (Exception ex)
            {
                ShowExecError(ex.Message);
            }
            finally
            {
                SetButtonsEnabled(true);
                SetCurrentFileLabel("...");
                ProgBar.Value = 0;
            }
        }

        private async void BtnExecuteAll_Click(object sender, RoutedEventArgs e)
        {
            if (GridJobs.Items.Count == 0)
            {
                ShowExecError(_langVM.GetString("error_no_jobs_exec"));
                return;
            }

            var progress = new Progress<int>(p => ProgBar.Value = p);

            Action<string> updateText = text =>
                Application.Current.Dispatcher.Invoke(() => SetCurrentFileLabel(text));

            ShowExecRunning(_langVM.GetString("executing_all"));
            SetButtonsEnabled(false);

            try
            {
                string errorMessage = await Task.Run(() => _saveVM.PerformJobs("", progress, updateText));

                if (string.IsNullOrEmpty(errorMessage))
                {
                    ShowExecSuccess(_langVM.GetString("success_execute_all"));
                }
                else
                {
                    ShowExecError(errorMessage);
                }
            }
            catch (Exception ex)
            {
                ShowExecError(ex.Message);
            }
            finally
            {
                SetButtonsEnabled(true);
                SetCurrentFileLabel("...");
                ProgBar.Value = 0;
            }
        }

        private void GridJobs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridJobs.SelectedItems.Count > 0)
                ClearExecBanners();
        }
    }
}