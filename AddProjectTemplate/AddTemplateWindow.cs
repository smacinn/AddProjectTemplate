using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Steven.Macinnis.AddProjectTemplate.Models;

namespace Steven.Macinnis.AddProjectTemplate
{
    public delegate void ProcessTemplateEventHandler(object sender, ProcessTemplateEventArgs e);
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("17d164be-7d71-47fd-b057-a67e9215a968")]
    public class AddTemplateWindow : ToolWindowPane
    {
        Dictionary<string, TemplateItem> _templates = new Dictionary<string, TemplateItem>();
       // Dictionary<string, string> _replacements = new Dictionary<string, string>();

        public event ProcessTemplateEventHandler ProcessTemplate;

        protected virtual void OnProcessTemplate(TemplateItem template)
        {
            var e = new ProcessTemplateEventArgs();
            e.Template = template;
            //e.ReplacementDictionary = _replacements;

            ProcessTemplateEventHandler handler = ProcessTemplate;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddTemplateWindow"/> class.
        /// </summary>
        public AddTemplateWindow() : base(null)
        {
            this.Caption = "Choose a Template";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new AddTemplateWindowControl();
        }
        public Dictionary<string, TemplateItem> Templates
        {
            set
            {
                _templates = value;
            }
        }

        //public Dictionary<string, string> ReplacementValues
        //{
        //    set
        //    {
        //        _replacements = value;
        //    }
        //}
    }
}
