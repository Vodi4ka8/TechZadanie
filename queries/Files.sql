CREATE TABLE IF NOT EXISTS public."Files"
(
    "Id" Serial primary key,
    "FilesName" "char"(100) NOT NULL,
    "Description" "char"(100),
	"FileTypeId" int REFERENCES public."FileExtension"("Id"),
	"FolderId" int REFERENCES public."Folders"("Id")
)