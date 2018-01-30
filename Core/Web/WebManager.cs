using System.IO;
using System.Net;

namespace Plus.Core.Net
{
    internal static class WebManager
    {
        /// <summary>
        ///     HTTPs the post json.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="json">The json.</param>
        /// <returns>System.String.</returns>
        public static string HttpPostJson(string uri, string json)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);

            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";

            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                return streamReader.ReadToEnd();
        }
    }
}