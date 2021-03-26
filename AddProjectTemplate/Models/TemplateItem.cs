using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMacinnis.AddProjectTemplate.Enums;

namespace SMacinnis.AddProjectTemplate.Models
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
