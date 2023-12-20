using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TechZadanie.Features.File
{
    public class SaveFile
    {
        private readonly string _fileName;

        private readonly string _fileExtension;

        private readonly string _description;

        private bool isNew = true;

        private string _connectionStr = Config.ConnectionStr;

        public SaveFile(string fileName , string fileExtension , string description) 
        {
            _fileExtension = fileExtension;
            _fileName = fileName;
            _description = description;
        }

        public async void Save() 
        {
            await using var dataSource = NpgsqlDataSource.Create(_connectionStr);

            await using (var cmd = dataSource.CreateCommand($"SELECT * FROM public.\"Files\" where \"FilesName\" = '{_fileName}' "))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    isNew = false;
                }
            }

            
            string defaultIconPath = @"\icons\default.png";
            var list = _fileName.Split("\\");
            var fileDirName = list[list.Length - 2];
            var fileName = list[list.Length - 1].Split('.')[0];
            if (isNew) 
            {
                int selectFolderId = 0;
                int selectTypeId = 0;

                await using (var cmd = dataSource.CreateCommand($"SELECT * FROM public.\"FileExtension\" where \"Name\" = '{_fileExtension}'"))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                    {
                        await using (var insert = dataSource.CreateCommand($"INSERT INTO public.\"FileExtension\"(\"Name\", \"Icon\")VALUES ('{_fileExtension}', '{defaultIconPath}');"))
                            insert.ExecuteNonQuery();
                    }
                }


                await using (var cmd = dataSource.CreateCommand($"Select \"Id\" From public.\"Folders\" where \"FolderName\" = '{fileDirName}'"))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        selectFolderId = (int)reader[0];
                    }
                }

                await using (var cmd = dataSource.CreateCommand($"Select \"Id\" From public.\"FileExtension\" where \"Name\" = '{_fileExtension}'"))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        selectTypeId = (int)reader[0];
                    }
                }

                await using (var cmd = dataSource.CreateCommand($"INSERT INTO public.\"Files\"( \"FileTypeId\", \"FolderId\", \"FilesName\", \"Description\")VALUES ({selectTypeId},{selectFolderId},'{fileName}','{_description}');"))
                    cmd.ExecuteNonQuery();
            }
            
        }
    }
}
