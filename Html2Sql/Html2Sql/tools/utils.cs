namespace Html2Sql.tools
{
    public class utils
    {
        public  static string GetImageAsBase64Url(string url)
        { 

            using (var client = new HttpClient())
            {
                var bytes =  client.GetByteArrayAsync(url).Result;
                return "image/jpeg;base64," + Convert.ToBase64String(bytes);
            }
        }
    }
}
