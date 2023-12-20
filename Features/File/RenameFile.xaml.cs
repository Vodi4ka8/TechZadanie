using Npgsql;
using System;
using System.Collections.Generic;
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
using FileProc = System.IO.File;

namespace TechZadanie.Features.File
{
    /// <summary>
    /// Логика взаимодействия для RenameFile.xaml
    /// </summary>
    public partial class RenameFile : Window
    {

        private string _connectionStr = Config.ConnectionStr;

        private const string defaultFolderName = "Имя Файла";

        public RenameFile()
        {
            InitializeComponent();
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FileNameBox.Text != defaultFolderName)
                {
                    var filesName = FileNameBox.Text;

                    var newFilesName = NewFileNameBox.Text;

                    int parentFolder = 0;
                    string parentFolderName = string.Empty;

                    int extension = 0;
                    string extensionName = string.Empty;
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
                            parentFolder = (int)reader[2];
                        }
                    }
                    await using (var cmd = dataSource.CreateCommand($"UPDATE public.\"Files\" SET  \"FilesName\"='{newFilesName}' WHERE \"FilesName\"='{filesName}';"))
                        cmd.ExecuteNonQuery();

                    await using (var cmd = dataSource.CreateCommand($"SELECT \"Name\" FROM public.\"FileExtension\" where \"Id\" = {extension} "))
                    await using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            extensionName = (string)reader[0];
                            extensionName = extensionName.Trim();
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

                    string path = filesName + extensionName;
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

                    var newPath = path.Replace(filesName, newFilesName);

                    var dirRecord = new DirectoryRecord();
                    var project = dirRecord.GetProjectPath();

                    FileProc.Move(project.FullName + $@"\\{path}", project.FullName + $@"\\{newPath}");
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
