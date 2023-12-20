CREATE TABLE IF NOT EXISTS public."Folders"
(
    "Id" Serial Primary KEY,
    "FolderName" "char"(100) NOT NULL,
    "ParentFolderName" "char"(100)
)