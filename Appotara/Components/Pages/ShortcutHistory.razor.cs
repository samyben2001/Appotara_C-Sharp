using Appotara.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Appotara.Components.Pages
{
    public partial class ShortcutHistory
    {
        string basePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string dirPath = @"\bats";
        List<ShortcutCreated> allShortcuts = new List<ShortcutCreated>();

        [Parameter]
        public EventCallback<ShortcutCreated> OnSeeShortcutDetail { get; set; }

        protected override void OnInitialized()
        {
            if (File.Exists($"{basePath}{dirPath}\\shortcutHistory.json"))
            {
                string fileName = $"{basePath}{dirPath}\\shortcutHistory.json";
                string jsonString = File.ReadAllText(fileName);

                allShortcuts = JsonSerializer.Deserialize<List<ShortcutCreated>>(jsonString)!;
            }
        }

        private void SeeShortcutDetail(ShortcutCreated shortcut)
        {
            OnSeeShortcutDetail.InvokeAsync(shortcut);
        }
    }
}
