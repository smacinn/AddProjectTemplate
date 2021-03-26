using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TemplateWizard;
using EnvDTE;
using EnvDTE80;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SMacinnis.AddProjectTemplate.Abstract;

namespace SMacinnis.AddProjectTemplate.Wizard
{
    public class CustomWizard : WizardBase, IWizard
    {

        // This method is called before opening any item that
        // has the OpenInEditor attribute.
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        // This method is only called for item templates,
        // not for project templates.
        public void ProjectItemFinishedGenerating(ProjectItem
            projectItem)
        {
        }

        // This method is called after the project is created.
        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                /* https://www.mztools.com/articles/2006/MZ2006004.aspx */
                /* https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-use-wizards-with-project-templates?view=vs-2019 */
                /* replacement variables https://docs.microsoft.com/en-us/visualstudio/ide/template-parameters?view=vs-2019 */
                var dte = automationObject as DTE2;


                var selectedItems = dte.SelectedItems;
                if (!selectedItems.MultiSelect)
                {
                    var selectedItem = selectedItems.Item(1);
                    var item = selectedItem.ProjectItem;
                    var projectPath = item.ContainingProject.FileName;
                    var templateJsonPath = Path.Combine(Path.GetDirectoryName(projectPath), "template.json");
                    if (File.Exists(templateJsonPath))
                    {
                        var jsonStr = File.ReadAllText(templateJsonPath);
                        JObject json = JObject.Parse(jsonStr);
                        foreach( var rep in json["replacementValues"])
                        {
                            var key = rep["placeholder"].ToString();
                            var val = rep["value"].ToString();
                            replacementsDictionary.Add(key, val);
                        }
                    }

                    var parent = item.Collection.Parent as ProjectItem;

                    // Add custom parameters.
                    if (item.Kind == Constants.vsProjectItemKindPhysicalFolder && parent.Kind == Constants.vsProjectItemKindPhysicalFolder)
                    {
                        Dictionary<string, string> variants = new Dictionary<string, string>();
                        // Add folder variants
                        replacementsDictionary.Add("$currentFolderName$", item.Name);
                        replacementsDictionary.Add("$parentFolderName$", parent.Name);

                        // add replacementsDictionary camel, kebab and pascal variants for existing and above keys
                        foreach (var rep in replacementsDictionary)
                        {
                            var key = rep.Key.Replace("$", "");
                            var pascalKey = string.Format("${0}ToPascal$", key);
                            var kebabKey = string.Format("${0}ToKebab$", key);
                            var camelKey = string.Format("${0}ToCamel$", key);

                            if (!replacementsDictionary.ContainsKey(pascalKey))
                                variants.Add(pascalKey, ToPascalCase(rep.Value));
                            if (!replacementsDictionary.ContainsKey(kebabKey))
                                variants.Add(kebabKey, ToKebabCase(rep.Value));
                            if (!replacementsDictionary.ContainsKey(camelKey))
                                variants.Add(camelKey, ToCamelCase(rep.Value));
                        }
                        foreach(var variant in variants)
                        {
                            replacementsDictionary.Add(variant.Key, variant.Value);
                        }
                     }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // This method is only called for item templates,
        // not for project templates.
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
