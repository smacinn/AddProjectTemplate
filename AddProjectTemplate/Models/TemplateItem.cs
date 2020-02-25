using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steven.Macinnis.AddProjectTemplate.Enums;

namespace Steven.Macinnis.AddProjectTemplate.Models
{
    public class TemplateItem
    {
        public TemplateItem()
        {
            TemplateType = TemplateType.FILE;
        }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public TemplateType TemplateType { get; set; }
    }
}
