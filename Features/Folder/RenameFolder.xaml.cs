using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TechZadanie.Features.Folder
{
    /// <summary>
    /// Логика взаимодействия для RenameFolder.xaml
    /// </summary>
    public partial class RenameFolder : Window
    {
        private  string _connectionStr = Config.ConnectionStr;

        private const string defaultFolderName = "Имя Папки";
        public RenameFolder()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FolderNameBox.Text != defaultFolderName)
                {
                    var folderName = FolderNameBox.Text;

                    var newFolderName = NewFolderNameBox.Text;

                    await using var dataSource = NpgsqlDataSource.Create(_connectionStr);

                    string parentFolder = string.Empty;
                    await using (var cmd = dataSource.CreateCommand($"SELECT * FROM public.\"Folders\" where \"FolderName\" = '{folderName}' "))
                    await using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            MessageBox.Show("Ошибка : Папка с таким именем не существует");
                            return;
                        }
                        while (reader.Read())
                        {
                            parentFolder = (string)reader[2];
                            parentFolder = parentFolder.Trim();
                        }
                    }

                    await using (var cmd = dataSource.CreateCommand($"Update public.\"Folders\" set \"ParentFolderName\" ='{newFolderName}' where \"ParentFolderName\" = '{folderName}'"))
                        await cmd.ExecuteNonQueryAsync();

                    await using (var cmd = dataSource.CreateCommand($"Update public.\"Folders\" set \"FolderName\" ='{newFolderName}' where \"FolderName\" = '{folderName}'"))
                        await cmd.ExecuteNonQueryAsync();

                    string oldPath = folderName;
                    while (!string.IsNullOrEmpty(parentFolder))
                    {
                        oldPath = oldPath.Insert(0, $@"{parentFolder}\");
                        await using (var cmd = dataSource.CreateCommand($"SELECT * FROM public.\"Folders\" where \"FolderName\" = '{parentFolder}' "))
                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                parentFolder = (string)reader[2];
                                parentFolder = parentFolder.Trim();
                            }

                        }
                    }
                    var dirRecord = new DirectoryRecord();
                    var project = dirRecord.GetProjectPath();

                    var newPath = oldPath.Replace(folderName, newFolderName);

                    Directory.Move(project.FullName + $@"\\{oldPath}" , project.FullName + $@"\\{newPath}");

                    MessageBox.Show("Успешно");
                    Window.GetWindow(this).Close();
                }
                else
                {
                    MessageBox.Show("Ошибка : укажите имя папки");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
