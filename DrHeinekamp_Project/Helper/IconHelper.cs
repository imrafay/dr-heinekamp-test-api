namespace DrHeinekamp_Project.Helper
{
    public static class IconHelper
    {
        public static string GetIconUrl(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".pdf" => "pdf",
                ".docx" => "docx",
                ".xlsx" => "xlsx",
                ".jpg" or ".jpeg" or ".png" => "jpg",
                _ => "png"
            };
        }
    }
}
