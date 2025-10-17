namespace ProjetoFinal.Aplication.Services.Utils;

public static class FileCopyUtil
{
    public static async Task Copy(string filePath, byte[]? content)
    {
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await stream.WriteAsync(content);
            stream.Close();
        }
    }
}