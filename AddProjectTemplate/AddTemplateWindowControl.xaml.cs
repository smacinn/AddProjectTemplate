namespace Steven.Macinnis.AddProjectTemplate
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Collections.Generic;
    using Steven.Macinnis.AddProjectTemplate.Models;

    

    /// <summary>
    /// Interaction logic for AddTemplateWindowControl.
    /// </summary>
    public partial class AddTemplateWindowControl : UserControl
    {
        public event AddTemplateEventHandler AddTemplate;
        public delegate void AddTemplateEventHandler(object sender, ProcessTemplateEventArgs e);

        /// <summary>
        /// Initializes a new instance of the <see cref="AddTemplateWindowControl"/> class.
        /// </summary>
        public AddTemplateWindowControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]

        public void AddItems(Dictionary<string, TemplateItem> templates)
        {
            lstTemplates.Items.Clear();
            foreach(var tpl in templates)
            {
                lstTemplates.Items.Add(tpl.Key);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var evt = new ProcessTemplateEventArgs();
            evt.TemplateName = lstTemplates.SelectedItem.ToString();
            evt.Filename = txtFilename.Text;

            AddTemplateEventHandler handler = AddTemplate;
            handler?.Invoke(sender, evt);
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "Cancelled");
        }

        private void lstTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }
    }
}