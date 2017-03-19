using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace OpenFileWithApps
{
    public sealed partial class MainPage : Page
    {
        StorageFile file;
        ObservableCollection<AppInfo> associatedAppsCollection { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
    
            associatedAppsCollection = new ObservableCollection<AppInfo>();
            AssociatedAppsComboBox.DataContext = associatedAppsCollection;
        }

        public async Task PickFile()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;

            openPicker.FileTypeFilter.Add("*");

            try
            {
                StorageFile tempFile = await openPicker.PickSingleFileAsync();
                if (tempFile != null)
                {
                    file = tempFile;
                    List<AppInfo> associatedApps = await GetAssociatedApps(file);
                    PopulateLocalCollection(associatedApps);
                }
            }
            catch(Exception ex)
            {
                //Something went wrong
            }
        }

        public void PopulateLocalCollection(List<AppInfo> associatedApps)
        {
            associatedAppsCollection.Clear();

            if (associatedApps.Count != 0)
            {
                foreach (AppInfo appInfo in associatedApps)
                {
                    associatedAppsCollection.Add(appInfo);
                }

                AssociatedAppsComboBox.SelectedIndex = 0;
                AssociatedAppsComboBox.IsEnabled = true;
                DefaultAppCheckBox.IsEnabled = true;
            }
            else
            {
                DefaultAppCheckBox.IsEnabled = false;
                AssociatedAppsComboBox.IsEnabled = false;
            }

            OpenFileButton.IsEnabled = true;
        }

        /// <summary>
        /// Gets the list of apps which can be used in order to launch a file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<List<AppInfo>> GetAssociatedApps(StorageFile file)
        {
            IReadOnlyList<AppInfo> associatedApps = await Launcher.FindFileHandlersAsync(file.FileType);
            return associatedApps.ToList();
        }

        /// <summary>
        /// A method which sets the options which are used to launch a file
        /// </summary>
        /// <returns></returns>
        public async Task OpenFile()
        {
            LauncherOptions options = new LauncherOptions();
            options.TreatAsUntrusted = false;//Won't show untrusted dialog
            options.DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseNone;

            //Shows Open With dialog where you can pick app which will launch the file; delete TargetApplicationPackageFamilyName if you'd like to use it
            //options.DisplayApplicationPicker = true;

            if (associatedAppsCollection.Count != 0)
            {
                if (!DefaultAppCheckBox.IsChecked.Value)
                {
                    AppInfo associatedAppInfo = AssociatedAppsComboBox.SelectedItem as AppInfo;
                    options.TargetApplicationPackageFamilyName = associatedAppInfo.PackageFamilyName;
                }
            }

            await Launcher.LaunchFileAsync(file, options);
        }

        private async void OpenFileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await OpenFile();
        }

        private async void PickFileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await PickFile();
        }
    }
}
