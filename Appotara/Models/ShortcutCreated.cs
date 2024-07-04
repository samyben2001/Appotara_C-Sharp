using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Appotara.Models
{
    public class ShortcutCreated
    {
       public string Name { get; set; } = "";
       public List<AppInfos> Apps { get; set; } = new List<AppInfos>();
       public string BatchScript { get; set; } = "";
    }
}
