using IWshRuntimeLibrary;
using Microsoft.AspNetCore.Components;
using AppInfos = Appotara.Models.AppInfos;


namespace Appotara.Components.Pages
{
    public partial class Home
    {
        string? shortcutName = "";
        bool isSelectorVisible = false;

        List<AppInfos> selectedApps = new List<AppInfos>();
        List<string> createdShortchuts = new List<string>();


        string basePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string dirPath = @"\bats";

        private void OpenAppSelector()
        {
            isSelectorVisible = true;
        }

        private void GetCreatedShortchuts()
        {
            string path = basePath + dirPath;

            //check if "bats" directory is existing
            if (Directory.Exists(path))
            {
                //get all files with .bat extension in directory
                var ext = new List<string> { "bat" };
                createdShortchuts = Directory
                    .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant())).ToList();
            }
        }

        private void ReadFile(string filePath)
        {
            //Read the content of the file 
            //TODO: check if the shortcut not already exist in the list
            string readText = System.IO.File.ReadAllText(filePath);
        }

        private void HandleSelectedApps(List<AppInfos> apps)
        {
            isSelectorVisible = false;
            apps.ForEach(app => { 
                if (!selectedApps.Contains(app))
                {
                    selectedApps.Add(app);
                }
            });
        }

        private void CreateShortcut()
        {
            //create content for bat script
            string batchScript = "";

            foreach (AppInfos app in selectedApps)
            {
                batchScript += @$"START """" ""{app.Path}"" {Environment.NewLine}";
            }

            //create directory and bat file
            CreateDir(basePath + dirPath);
            CreateBatFile(basePath + dirPath + @$"\{ shortcutName}.bat", batchScript);

            //Create shortcut on desktop to bat file
            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @$"\{shortcutName}.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "";
            shortcut.TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + dirPath + @$"\{shortcutName}.bat";
            shortcut.Save();
        }

        //remove path from list
        private void RemovePath(AppInfos appToRemove)
        {
            selectedApps.Remove(appToRemove);
        }

        //create directory
        private void CreateDir(string path)
        {
            try
            {
                // Determine whether the directory exists.
                if (!Directory.Exists(path))
                { 
                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }
        }

        //create bat file
        private void CreateBatFile(string path, string content)
        {
            if (!System.IO.File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = System.IO.File.CreateText(path))
                {
                    sw.WriteLine(content);
                }
            }
        }

    }
}
