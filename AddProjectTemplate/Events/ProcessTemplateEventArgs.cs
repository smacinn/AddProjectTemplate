using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steven.Macinnis.AddProjectTemplate.Models;

namespace Steven.Macinnis.AddProjectTemplate
{
    public class ProcessTemplateEventArgs : EventArgs
    {
        public string Filename { get; set; }
        public string TemplateName { get; set; }
    }
}
