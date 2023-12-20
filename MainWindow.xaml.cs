using Microsoft.Win32;
using Npgsql;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using TechZadanie.Features.File;
using TechZadanie.Features.Folder;

namespace TechZadanie;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string _connectionStr = Config.ConnectionStr;
    public MainWindow()
    {
        InitializeComponent();
        trvStructure.Items.Add(CreateTreeItem("TechZadanie", null,null));
    }

    private void AddFile_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "All files (*.*)|*.*";
        var dirRecord = new DirectoryRecord();
        var project = dirRecord.GetProjectPath();
        ofd.InitialDirectory = project.FullName;

        if (ofd.ShowDialog() == true)
        {
            TextRange doc = new TextRange(docBox.Document.ContentStart, docBox.Document.ContentEnd);
            using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open))
            {
                if (Path.GetExtension(ofd.FileName).ToLower() == ".rtf")
                    doc.Load(fs, DataFormats.Rtf);
                else if (Path.GetExtension(ofd.FileName).ToLower() == ".txt")
                    doc.Load(fs, DataFormats.Text);
                else
                    doc.Load(fs, DataFormats.Xaml);
            }
        }
    }
    private void DeleteFile_Click(object sender, RoutedEventArgs e)
    {
        var window = new DeleteFile();
        window.Show();
    }
    private void SaveFile_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog sfd = new SaveFileDialog();
        sfd.Filter = "Text Files (*.txt)|*.txt|RichText Files (*.rtf)|*.rtf|XAML Files (*.xaml)|*.xaml|All files (*.*)|*.*";
        var dirRecord = new DirectoryRecord();
        var project = dirRecord.GetProjectPath();
        sfd.InitialDirectory = project.FullName;

        if (sfd.ShowDialog() == true)
        {
            TextRange doc = new TextRange(docBox.Document.ContentStart, docBox.Document.ContentEnd);
            using (FileStream fs = File.Create(sfd.FileName))
            {
                if (Path.GetExtension(sfd.FileName).ToLower() == ".rtf")
                    doc.Save(fs, DataFormats.Rtf);
                else if (Path.GetExtension(sfd.FileName).ToLower() == ".txt")
                    doc.Save(fs, DataFormats.Text);
                else
                    doc.Save(fs, DataFormats.Xaml);

            }
            var window = new Description();
            window.ShowDialog();
            var saver = new SaveFile(sfd.FileName, Path.GetExtension(sfd.FileName).ToLower(), window.Text);
            saver.Save();
        }
    }
    private void RenameFile_Click(object sender, RoutedEventArgs e)
    {
        var window = new AddFolder();
        window.Show();
    }
    private void AddFolder_Click(object sender, RoutedEventArgs e)
    {
        var window = new AddFolder();
        window.Show();
    }
    private void DeleteFolder_Click(object sender, RoutedEventArgs e)
    {
        var window = new DeleteFolder();
        window.Show();
    }
    private void RenameFolder_Click(object sender, RoutedEventArgs e)
    {
        var window = new RenameFolder();
        window.Show();
    }

    private TreeViewItem CreateTreeItem(string treeItem , string icon , string descr)
    {
        TreeViewItem item = new TreeViewItem();
        item.Header = treeItem;
        
        item.Tag = item.Header.ToString() == "TechZadanie"? string.Empty : treeItem;
        item.Items.Add("Loading...");

        if(icon != null) 
        {
            var dirRecord = new DirectoryRecord();
            var project = dirRecord.GetProjectPath();
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;
            // create Image
            Image image = new Image();
            image.Source = new BitmapImage
                (new Uri(project.FullName + icon));
            image.Width = 16;
            image.Height = 16;
            // Label
            Label lbl = new Label();
            lbl.Content = treeItem;
            ToolTip toolTip = new ToolTip();
            toolTip.Content = descr;
            // Add into stack
            stack.Children.Add(image);
            stack.Children.Add(lbl);
            stack.ToolTip = toolTip;
            item.Header = stack;
        }
        return item;
    }

    public async void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
    {
        TreeViewItem item = e.Source as TreeViewItem;
        if ((item.Items.Count == 1) && (item.Items[0] is string))
        {
            item.Items.Clear();

            await using var dataSource = NpgsqlDataSource.Create(_connectionStr); 

            await using (var cmd = dataSource.CreateCommand($"SELECT \"FolderName\" FROM public.\"Folders\" WHERE \"ParentFolderName\" = '{item.Tag.ToString()}'"))
            await using (var reader = await cmd.ExecuteReaderAsync()) 
            {
                if(reader.HasRows)
                while(reader.Read()) 
                {
                    item.Items.Add(CreateTreeItem((string)reader[0],null,null));
                }
            }
            await using (var Files = dataSource.CreateCommand($"SELECT F.\"FilesName\",F.\"Description\",H.\"Name\",h.\"Icon\" FROM public.\"Files\" as F JOIN \"Folders\" as G on G.\"Id\" = F.\"FolderId\" join \"FileExtension\" as H on H.\"Id\" = F.\"FileTypeId\"where G.\"FolderName\" ='{item.Tag.ToString()}'"))
            await using (var readerFile = await Files.ExecuteReaderAsync()) 
            {
                while (readerFile.Read())
                {
                    var name = (string)readerFile[0];
                    name = name.Trim();
                    var description = (string)readerFile[1];
                    description = description.Trim();
                    var ext = (string)readerFile[2];
                    ext = ext.Trim();
                    var treeItem = name + ext;
                    item.Items.Add(CreateTreeItem(treeItem, (string)readerFile[3] , description));
                }
            }

        }
    }

    public async void TreeViewItem_Selected(object sender, RoutedEventArgs e)
    {
        TreeViewItem item = e.Source as TreeViewItem;
        if ((item.Items.Count == 1) && (item.Items[0] is string) && (item.Tag.ToString().Contains('.')))
        {
            int parentFolder = 0;
            string parentFolderName = string.Empty;
            var filesName = item.Tag.ToString().Split('.')[0];
            await using var dataSource = NpgsqlDataSource.Create(_connectionStr);

            await using (var cmd = dataSource.CreateCommand($"SELECT * FROM public.\"Files\" where \"FilesName\" = '{filesName}' "))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (!reader.HasRows)
                {
                    MessageBox.Show("Ошибка : Папка с таким именем не существует");
                    return;
                }
                else
                {
                    while(reader.Read())
                        parentFolder = (int)reader[2];
                }
            }


            await using (var cmd = dataSource.CreateCommand($"SELECT \"FolderName\" FROM public.\"Folders\" where \"Id\" = {parentFolder} "))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    parentFolderName = (string)reader[0];
                    parentFolderName = parentFolderName.Trim();
                }
            }

            string path = item.Tag.ToString();
            while (!string.IsNullOrEmpty(parentFolderName))
            {
                path = path.Insert(0, $@"{parentFolderName}\");
                await using (var cmd = dataSource.CreateCommand($"SELECT \"ParentFolderName\" FROM public.\"Folders\" where \"FolderName\" = '{parentFolderName}' "))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        parentFolderName = (string)reader[0];
                        parentFolderName = parentFolderName.Trim();
                    }

                }
            }

            var dirRecord = new DirectoryRecord();
            var project = dirRecord.GetProjectPath();

            path = path.Insert(0, $@"{project.FullName}\");

            TextRange doc = new TextRange(docBox.Document.ContentStart, docBox.Document.ContentEnd);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                if (Path.GetExtension(path).ToLower() == ".rtf")
                    doc.Load(fs, DataFormats.Rtf);
                else if (Path.GetExtension(path).ToLower() == ".txt")
                    doc.Load(fs, DataFormats.Text);
                else
                    doc.Load(fs, DataFormats.Xaml);
            }
        }
    }
}

