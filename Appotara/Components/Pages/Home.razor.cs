using Appotara.Models;
using IWshRuntimeLibrary;
using Microsoft.AspNetCore.Components;
using System.Text.Json;
using File = System.IO.File;
using AppInfos = Appotara.Models.AppInfos;
using Microsoft.JSInterop;
using Blazored.Toast.Services;


namespace Appotara.Components.Pages
{
    public partial class Home
    {
        [Inject]
        IJSRuntime? JSRuntime { get; set; }


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

        private void CloseAppSelector()
        {
            isSelectorVisible = false;
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
            apps.ForEach(app =>
            {
                if (!selectedApps.Contains(app))
                {
                    selectedApps.Add(app);
                }
            });
        }

        private void ShowShortcutDetail(ShortcutCreated shortcut)
        {
            shortcutName = shortcut.Name;
            selectedApps = shortcut.Apps;
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

        //create batch file
        private void CreateFile(string path, object content)
        {
            try
            {
                if (!File.Exists(path))
                {
                    if (path.Contains(".json"))
                    {
                        content = JsonSerializer.Serialize((List<ShortcutCreated>)content);
                    }
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(content);
                    }
                }
                else
                {
                    if (path.Contains(".json"))
                    {
                        string jsonString = File.ReadAllText(path);
                        List<ShortcutCreated> allShortcuts = JsonSerializer.Deserialize<List<ShortcutCreated>>(jsonString)!;
                        allShortcuts.AddRange((List<ShortcutCreated>)content);

                        using (StreamWriter sw = File.CreateText(path))
                        {
                            sw.WriteLine(JsonSerializer.Serialize(allShortcuts));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }
        }

        private void CreateShortcut()
        {
            //create content for batch script
            string batchScript = "@echo off" + Environment.NewLine;

            foreach (AppInfos app in selectedApps)
            {
                batchScript += @$"START """" ""{app.Path}"" {Environment.NewLine}";
            }

            //create directory and batch file
            CreateDir(basePath + dirPath);
            CreateFile(basePath + dirPath + @$"\{shortcutName}.bat", batchScript);
            try
            {
                //Create shortcut on desktop to batch file
                object shDesktop = (object)"Desktop";
                WshShell shell = new WshShell();
                string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @$"\{shortcutName}.lnk";
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                shortcut.Description = "";
                shortcut.TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + dirPath + @$"\{shortcutName}.bat";
                shortcut.Save();

                ShortcutCreated shortcutCreated = new ShortcutCreated();
                shortcutCreated.Name = shortcutName!;
                shortcutCreated.Apps = selectedApps;
                shortcutCreated.BatchScript = batchScript;

                List<ShortcutCreated> shortcutCreatedList = [shortcutCreated];
                CreateFile(basePath + dirPath + @$"\shortcutHistory.json", shortcutCreatedList);

                shortcutName = "";
                selectedApps = new List<AppInfos>();

                toastService.ShowSuccess("Shortcut created on Desktop");
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }
        }

    }
}
