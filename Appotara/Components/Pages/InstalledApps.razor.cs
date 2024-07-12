using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.WindowsAPICodePack.Shell;
using System.Drawing;
using AppInfos = Appotara.Models.AppInfos;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

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
        public bool IsVisible
        {
            get { return isVisible; } 
            set {
                if (value)
                {
                    isVisible = true;
                }
                else
                {
                    if (selectedApps.Count > 0)
                        CallJsMethodToUncheckCheckboxes();
                    selectedApps = new List<AppInfos>();
                    isVisible = false;
                }
            }
        }

        [Parameter]
        public EventCallback<List<AppInfos>> OnSelectedAppsValidate { get; set; }

        [Parameter]
        public EventCallback<List<AppInfos>> OnClosed { get; set; }

        [CascadingParameter] public IModalService Modal { get; set; } = default!;

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
                string appPath = app.Properties.System.Link.TargetParsingPath.Value;
                if (appPath is not null && appPath.Contains(".exe") && !appPath.Contains("\\Windows\\"))
                {
                    AppInfos appInfo = new AppInfos();

                    // The friendly app name
                    appInfo.Name = app.Name;
                    // The ParsingName property is the AppUserModelID
                    string appUserModelID = app.ParsingName; // or app.Properties.System.AppUserModel.ID
                                                             // You can even get the Jumbo icon in one shot
                                                             //The path of the app
                    appInfo.Path = app.Properties.System.Link.TargetParsingPath.Value;
                    appInfo.Icon = turnImageToByteArray(app.Thumbnail.Icon); //OR ImageSource icon = app.Thumbnail.BitmapSource;

                    installedApps.Add(appInfo);
                }
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
                app.Path = result.FullPath;
                app.Name = Path.GetFileNameWithoutExtension(result.FileName);
                //check if path not already selected
                //TODO: check on app.Path
                if (!selectedApps.Contains(app))
                {
                    selectedApps.Add(app);
                    ValidatePath();
                }
                else
                {
                    toastService.ShowError("You have already select this application");
                }
            }
        }

        private void ValidatePath()
        {
            OnSelectedAppsValidate.InvokeAsync(selectedApps);
            CloseSelector();
        }

        private async Task ShowModal()
        {
            // call confirm modal if selectedApps not empty
            if (selectedApps.Count > 0)
            {
                var options = new ModalOptions
                {
                    UseCustomLayout = true
                };

                var parameters = new ModalParameters()
                    .Add(nameof(ConfirmDialog.Title), "You have some applications selected!")
                    .Add(nameof(ConfirmDialog.Message), "Exit anyway?");

                var mod = Modal.Show<ConfirmDialog>(parameters, options);
                var result = await mod.Result;

                if (result.Cancelled)
                {
                    Console.WriteLine("Modal was cancelled");
                }
                else if (result.Confirmed)
                {
                    CloseSelector();
                }
            }
            else
            {
                CloseSelector();
            }
        }

        private void CloseSelector()
        {
            OnClosed.InvokeAsync();
        }

        private string turnImageToByteArray(System.Drawing.Icon img)
        {
            // Convert the icon to a Bitmap object
            Bitmap bitmap = img.ToBitmap();
            bitmap.MakeTransparent(bitmap.GetPixel(0, 0));


            // Save the bitmap as a PNG file and convert in base64
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);

                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
}