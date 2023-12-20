using Npgsql;
using System;
using System.IO;
using System.Windows;

namespace TechZadanie.Features.Folder
{
    /// <summary>
    /// Логика взаимодействия для AddFolder.xaml
    /// </summary>
    public partial class AddFolder : Window
    {
        private string _connectionStr = Config.ConnectionStr;

        private const string defaultFolderName = "Имя Папки";
        private const string defaultParentFolderName = "Родительская папка";

        public AddFolder()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FolderNameBox.Text != defaultFolderName || FolderNameBox.Text.Contains(string.Empty))
                {
                    var folderName = FolderNameBox.Text;
                    var parentFolderName = ParentFolderNameBox.Text == defaultParentFolderName ? null : ParentFolderNameBox.Text;

                    await using var dataSource = NpgsqlDataSource.Create(_connectionStr);


                    await using (var cmd = dataSource.CreateCommand($"SELECT * FROM public.\"Folders\" where \"FolderName\" = '{folderName}' "))
                    await using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            MessageBox.Show("Ошибка : Папка с таким именем уже существует");
                            return;
                        }
                    }

                    if (parentFolderName != null)
                        await using (var cmd = dataSource.CreateCommand($"SELECT * FROM public.\"Folders\" where \"FolderName\" = '{parentFolderName}' "))
                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows)
                            {
                                MessageBox.Show("Ошибка : Родительской папки с таким именем не существует");
                                return;
                            }
                        }

                    await using (var cmd = dataSource.CreateCommand("INSERT INTO public."
                                    + $"\"Folders\"(\"FolderName\", \"ParentFolderName\")VALUES('{folderName}','{parentFolderName}')"))

                        await cmd.ExecuteNonQueryAsync();

                    var dirRecord = new DirectoryRecord();
                    var project = dirRecord.GetProjectPath();

                    if (parentFolderName != null)
                        Directory.CreateDirectory(project.FullName + @$"\\{parentFolderName}\\{folderName}");
                    else
                        Directory.CreateDirectory(project.FullName + @$"\\{folderName}");

                    MessageBox.Show("Успешно");
                    Window.GetWindow(this).Close();

                }
                else
                {
                    MessageBox.Show("Ошибка : укажите корректное имя папки");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
