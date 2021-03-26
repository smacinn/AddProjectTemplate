using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMacinnis.AddProjectTemplate.Models;

namespace SMacinnis.AddProjectTemplate
{
    public class ProcessTemplateEventArgs : EventArgs
    {
        public string Filename { get; set; }
        public string TemplateName { get; set; }
    }
}
