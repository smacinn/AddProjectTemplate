using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.TemplateWizard;
using EnvDTE;
using EnvDTE80;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Newtonsoft.Json.Linq;
using SMacinnis.AddProjectTemplate.Models;
using SMacinnis.AddProjectTemplate.Abstract;
using System.IO.Compression;

namespace SMacinnis.AddProjectTemplate
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AddTemplateWindowCommand : WizardBase
    {
        public static DTE2 _dte;

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("158bddc7-e55d-40b4-b0de-a4f205500d01");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddTemplateWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private AddTemplateWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AddTemplateWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in AddTemplateWindowCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var result = await package.GetServiceAsync(typeof(DTE));
            if (result == null)
            {
                throw new Exception("Erro initializing Package, DTE is null");
            }
            else
            {
                _dte = result as DTE2;
            }

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new AddTemplateWindowCommand(package, commandService);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            /* https://www.mztools.com/articles/2006/MZ2006004.aspx */
            Dictionary<string, TemplateItem> templateList = new Dictionary<string, TemplateItem>();
            Dictionary<string, string> replacementsDictionary = new Dictionary<string, string>();
            var selectedItems = _dte.SelectedItems;
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
                    foreach (var fldr in json["templateFolders"])
                    {
                        var folderName = fldr.ToString();
                        var folderPath = Path.Combine(Path.GetDirectoryName(projectPath), folderName);
                        var files = Directory.GetFiles(folderPath);
                        foreach (var file in files)
                        {
                            var templateItem = new TemplateItem()
                            {
                                FileName = Path.GetFileName(file),
                                FilePath = file
                            };
                            var ext = Path.GetExtension(file);
                            if (ext.ToLower() == ".zip")
                            {
                                templateItem.TemplateType = Enums.TemplateType.ITEMTEMPLATE;
                            }
                            templateList.Add(templateItem.FileName, templateItem);
                        }
                    }
                    foreach (var rep in json["replacementValues"])
                    {
                        var key = rep["placeholder"].ToString();
                        var val = rep["value"].ToString();
                        replacementsDictionary.Add(key, val);
                    }
                }

                var parent = item.Collection.Parent as ProjectItem;

                // Add custom parameters.
                if (item.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder && parent.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
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
                    foreach (var variant in variants)
                    {
                        replacementsDictionary.Add(variant.Key, variant.Value);
                    }
                }
                AddTemplateWindow window = null;
                ProcessTemplateEventHandler handler = null;

                handler = (eSender, evt) =>
                {
                    ThreadHelper.ThrowIfNotOnUIThread();

                    var selectedTemplate = templateList[evt.TemplateName];
                    if (selectedTemplate.TemplateType == Enums.TemplateType.ITEMTEMPLATE)
                    {
                        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                        
                        ZipFile.ExtractToDirectory(selectedTemplate.FilePath, temp);
                        //_dte.Solution.AddFromTemplate();
                        var installerFiles = Directory.GetFiles(temp, "*.vstemplate");
                        if (installerFiles.Length > 0)
                        {
                            item.ProjectItems.AddFromTemplate(installerFiles[0], evt.Filename);
                        }
                        //Directory.Delete(temp,true);
                    }
                    else
                    {
                        if (File.Exists(selectedTemplate.FilePath))
                        {
                            string folderPath = string.Empty;
                            
                            for(int x = 1; x <= item.Properties.Count; x++)
                            {
                                if(item.Properties.Item(x) != null && item.Properties.Item(x).Name == "FullPath")
                                {
                                    folderPath = item.Properties.Item(x).Value.ToString();
                                    break;
                                }
                            }

                            if (!string.IsNullOrEmpty(folderPath))
                            {
                                string fileContents = File.ReadAllText(selectedTemplate.FilePath);
                                string filename = string.Empty;
                                string extension = Path.GetExtension(evt.Filename);

                                if (string.IsNullOrEmpty(extension))
                                {
                                    extension = Path.GetExtension(selectedTemplate.FilePath);
                                    filename = evt.Filename + extension;
                                }
                                else
                                {
                                    filename = evt.Filename;
                                }
                                foreach (var pair in replacementsDictionary)
                                {
                                    fileContents = fileContents.Replace(pair.Key, pair.Value);
                                }
                                filename = Path.Combine(folderPath, filename);
                                File.WriteAllText(filename,fileContents);
                                item.ProjectItems.AddFromFile(filename);
                            }
                        }
                    }
                };

                this.package.JoinableTaskFactory.RunAsync(async delegate
                {
                    window = await this.package.ShowToolWindowAsync(typeof(AddTemplateWindow), 0, true, this.package.DisposalToken) as AddTemplateWindow;
                    if ((null == window) || (null == window.Frame))
                    {
                        throw new NotSupportedException("Cannot create tool window");
                    }
                    window.Templates = templateList;
                    //window.ReplacementValues = replacementsDictionary;

                    window.ProcessTemplate += handler;

                });
            }

        }
    }
}
