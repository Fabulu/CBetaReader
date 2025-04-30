using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;

namespace CbetaZenReader
{
    public partial class MainWindow : Window
    {
        private string cbetaRootPath = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select CBETA XML-P5 Root Folder"
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                cbetaRootPath = dlg.FileName;

                if (Directory.Exists(cbetaRootPath))
                {
                    PopulateTreeView();
                    FolderPickerPanel.Visibility = Visibility.Collapsed;
                    MainViewerPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("Selected folder does not exist.");
                }
            }
        }

        private void PopulateTreeView()
        {
            CanonTreeView.Items.Clear();

            var canonFolders = Directory.GetDirectories(cbetaRootPath);

            foreach (var canonPath in canonFolders)
            {
                string canonCode = Path.GetFileName(canonPath);
                var canonNode = new TreeViewItem
                {
                    Header = canonCode,
                    Tag = canonCode
                };

                foreach (var subfolder in Directory.GetDirectories(canonPath))
                {
                    foreach (var file in Directory.GetFiles(subfolder, "*.xml"))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        canonNode.Items.Add(new TreeViewItem
                        {
                            Header = fileName,
                            Tag = file
                        });
                    }
                }

                if (canonNode.Items.Count > 0)
                    CanonTreeView.Items.Add(canonNode);
            }
        }

        private void CanonTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CanonTreeView.SelectedItem is TreeViewItem selectedItem &&
                selectedItem.Tag is string filePath &&
                File.Exists(filePath))
            {
                DisplayXmlText(filePath);
            }
        }

        public class TocEntry
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public int Level { get; set; }
            public int Offset { get; set; }

            public override string ToString()
            {
                string prefix = Level switch
                {
                    1 => "📘 ",
                    2 => "📗 ",
                    3 => "📙 ",
                    _ => new string('•', Level) + " "
                };
                return prefix + Title;
            }
        }

        private void TocListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TocListBox.SelectedItem is TocEntry entry)
            {
                TextDisplay.Focus();
                TextDisplay.ScrollToLine(TextDisplay.GetLineIndexFromCharacterIndex(entry.Offset));
            }
        }

        private void TocItem_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.Tag is TocEntry entry)
            {
                var menu = new ContextMenu();
                var copyItem = new MenuItem { Header = "Copy Title" };
                copyItem.Click += (s, args) => Clipboard.SetText(entry.Title);
                menu.Items.Add(copyItem);
                tb.ContextMenu = menu;
                menu.IsOpen = true;
            }
        }

        private void TocItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void DisplayXmlText(string xmlPath)
        {
            try
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Ignore
                };

                using var reader = XmlReader.Create(xmlPath, settings);
                XDocument doc = XDocument.Load(reader);

                XNamespace tei = "http://www.tei-c.org/ns/1.0";
                XNamespace cb = "http://www.cbeta.org/ns/1.0";
                XNamespace xml = "http://www.w3.org/XML/1998/namespace";

                var sb = new System.Text.StringBuilder();
                var tocEntries = new List<TocEntry>();
                var body = doc.Root?.Element(tei + "text")?.Element(tei + "body");

                if (body == null)
                {
                    TextDisplay.Text = "No body content found.";
                    return;
                }

                int charOffset = 0;

                void RenderElement(XElement el, int indent = 0)
                {
                    string prefix = new string(' ', indent * 2);

                    if (el.Name == tei + "head")
                    {
                        sb.AppendLine($"\n\n🧵 {el.Value.Trim()}\n");
                    }
                    else if (el.Name == tei + "p")
                    {
                        sb.AppendLine(prefix + el.Value.Trim());
                    }
                    else if (el.Name == tei + "item")
                    {
                        sb.Append(prefix + "• ");
                        foreach (var child in el.Elements())
                            RenderElement(child, indent + 1);
                    }
                    else if (el.Name == tei + "list")
                    {
                        foreach (var item in el.Elements())
                            RenderElement(item, indent + 1);
                    }
                    else if (el.Name == cb + "mulu")
                    {
                        int level = int.TryParse(el.Attribute("level")?.Value, out var lvl) ? lvl : 1;
                        string id = el.Attribute(xml + "id")?.Value ?? Guid.NewGuid().ToString();
                        string title = el.Value.Trim();
                        tocEntries.Add(new TocEntry
                        {
                            Id = id,
                            Title = title,
                            Level = level,
                            Offset = charOffset
                        });

                        sb.AppendLine($"\n📖 {title}");
                    }
                    else if (el.Name.LocalName == "pb" && el.Attribute("n") != null)
                    {
                        sb.AppendLine($"\n📄 Page {el.Attribute("n")?.Value}\n");
                    }
                    else if (el.Name.LocalName == "lb")
                    {
                        sb.AppendLine();
                    }
                    else if (el.Name.LocalName == "milestone" && el.Attribute("unit")?.Value == "juan")
                    {
                        sb.AppendLine($"\n📜 卷 {el.Attribute("n")?.Value}\n");
                    }
                    else
                    {
                        foreach (var child in el.Elements())
                            RenderElement(child, indent);
                    }

                    charOffset = sb.Length;
                }

                RenderElement(body);
                TextDisplay.Text = sb.ToString().Trim();
                TocListBox.ItemsSource = tocEntries;
            }
            catch (Exception ex)
            {
                TextDisplay.Text = $"Error loading text: {ex.Message}";
            }
        }

        private async void TranslateButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedText = TextDisplay.SelectedText;

            if (string.IsNullOrWhiteSpace(selectedText))
            {
                MessageBox.Show("Please select some text first.");
                return;
            }

            LoadingIndicator.Visibility = Visibility.Visible;
            TranslationDisplay.Text = ""; // Clear previous output

            try
            {
                string result = await Task.Run(() =>
                {
                    string pythonDir = Path.Combine(AppContext.BaseDirectory, "Python");
                    string pythonExe = Path.Combine(pythonDir, "python.exe");
                    string scriptPath = Path.Combine(pythonDir, "translate_io.py");
                    string inputPath = Path.Combine(pythonDir, "input.txt");
                    string outputPath = Path.Combine(pythonDir, "output.txt");

                    File.WriteAllText(inputPath, selectedText, System.Text.Encoding.UTF8);

                    var psi = new ProcessStartInfo
                    {
                        FileName = pythonExe,
                        Arguments = $"\"{scriptPath}\" \"{inputPath}\" \"{outputPath}\"",
                        WorkingDirectory = pythonDir,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    psi.Environment["PYTHONHOME"] = pythonDir;
                    psi.Environment["PYTHONPATH"] = Path.Combine(pythonDir, "Lib", "site-packages");

                    using var process = Process.Start(psi);
                    if (process == null) return "[Process failed to start]";

                    process.WaitForExit();

                    string error = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(error))
                        return $"[Translation error]\n{error}";

                    if (File.Exists(outputPath))
                        return File.ReadAllText(outputPath).Trim();

                    return "[Translation failed: Output file missing]";
                });

                TranslationDisplay.Text = result;
            }
            catch (Exception ex)
            {
                TranslationDisplay.Text = $"[Translation failed: {ex.Message}]";
            }
            finally
            {
                LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }




    }
}
