using Npgsql;
using System;
using FileProc = System.IO.File;
using System.Windows;
namespace TechZadanie.Features.File
{
    /// <summary>
    /// Логика взаимодействия для DeleteFile.xaml
    /// </summary>
    public partial class DeleteFile : Window
    {
        private string _connectionStr = Config.ConnectionStr;

        private const string defaultFileName = "Имя Файла";
        public DeleteFile()
        {
            InitializeComponent();
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var fileName = FileNameBox.Text.Trim();
                if (fileName != defaultFileName)
                {
                    await using var dataSource = NpgsqlDataSource.Create(_connectionStr);


                    int parentFolder = 0;
                    string parentFolderName = string.Empty;

                    int extension = 0;
                    string extensionName = string.Empty;

                    await using (var cmd = dataSource.CreateCommand($"SELECT * FROM public.\"Files\" where \"FilesName\" = '{fileName}' "))
                    await using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            MessageBox.Show("Ошибка : Папка с таким именем не существует");
                            return;
                        }
                        while (reader.Read())
                        {
                            parentFolder = (int)reader[2];
                            extension = (int)reader[1];
                        }
                    }

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

                    await using (var cmd = dataSource.CreateCommand($"Delete  FROM public.\"Files\" where \"FilesName\" = '{fileName}' "))
                        await cmd.ExecuteNonQueryAsync();

                    string deletedPath = fileName + extensionName;
                    while (!string.IsNullOrEmpty(parentFolderName))
                    {
                        deletedPath = deletedPath.Insert(0, $@"{parentFolderName}\");
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

                    FileProc.Delete(project.FullName + $@"\\{deletedPath}");

                    MessageBox.Show("Успешно");
                    Window.GetWindow(this).Close();
                }
                else
                {
                    MessageBox.Show("Ошибка : укажите имя файла");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
