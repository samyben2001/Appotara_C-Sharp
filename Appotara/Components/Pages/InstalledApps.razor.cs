using System.Drawing;
using Microsoft.AspNetCore.Components;
using Microsoft.WindowsAPICodePack.Shell;
using AppInfos = Appotara.Models.AppInfos;
using Microsoft.JSInterop;

namespace Appotara.Components.Pages
{
    public partial class InstalledApps
    {
        [Inject]
        IJSRuntime JSRuntime { get; set; }

        List<AppInfos> installedApps = new List<AppInfos>();
        List<AppInfos> selectedApps = new List<AppInfos>();

        private bool isVisible;
        [Parameter]
        public bool IsVisible { 
            get { return isVisible; } 
            set {
                if (value)
                {
                    isVisible = true;
                }
                else
                {
                    CallJsMethodToUncheckCheckboxes();
                    selectedApps = new List<AppInfos>();
                    isVisible = false;
                }
            }
        }

        [Parameter]
        public EventCallback<List<AppInfos>> OnSelectedAppsValidate { get; set; }

        protected override void OnInitialized()
        {
            IsVisible = false;
            GetInstalledApps();
        }

        private async Task CallJsMethodToUncheckCheckboxes()
        {
            await JSRuntime.InvokeVoidAsync("disableCheckBoxes");
        }

        //Get the applications insalled on Windows from shell:appsFolder
        private void GetInstalledApps()
        {
            // folder GUID taken from https://learn.microsoft.com/en-us/windows/win32/shell/knownfolderid
            var FODLERID_AppsFolder = new Guid("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");
            ShellObject appsFolder = (ShellObject)KnownFolderHelper.FromKnownFolderId(FODLERID_AppsFolder);

            foreach (var app in (IKnownFolder)appsFolder)
            {
                AppInfos appInfo = new AppInfos();

                // The friendly app name
                appInfo.Name = app.Name;
                // The ParsingName property is the AppUserModelID
                string appUserModelID = app.ParsingName; // or app.Properties.System.AppUserModel.ID
                                                         // You can even get the Jumbo icon in one shot
                //The path of the app
                appInfo.Path = app.Properties.System.Link.TargetParsingPath.Value;
                installedApps.Add(appInfo);

                Icon icon = app.Thumbnail.Icon; //OR ImageSource icon = app.Thumbnail.BitmapSource;
            }
        }

        private void AddOrRemovePath(AppInfos app)
        {
            if (selectedApps.Contains(app))
            {
                selectedApps.Remove(app);
            }else
            {
                selectedApps.Add(app);
            }
        }

        //Open Windows file ficker
        private async void BrowseApp()
        {
            var result = await FilePicker.PickAsync();
            //check if user have selected a file
            if (result is not null)
            {
                AppInfos app = new AppInfos();
                app.Path = result.FullPath; //TODO: check if fullpath and filename are same than shell
                app.Name= result.FileName;
                //check if path not already selected
                if (!selectedApps.Contains(app))
                {
                    selectedApps.Add(app);
                }
                else
                {
                   //TODO: make an alert: app already selected
                }
            }
        }

        private void ValidatePath()
        {
            OnSelectedAppsValidate.InvokeAsync(selectedApps);
        }

        private void CloseSelector()
        {
            IsVisible = false;
        }
    }
}
