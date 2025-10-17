namespace ProjetoFinal.Aplication.Services.Utils;

public static class DirectoryUtils
{
    public static void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}