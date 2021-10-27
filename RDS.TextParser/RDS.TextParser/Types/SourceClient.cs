using System;
using System.Net;
using RDS.TextParser.Interfaces;

namespace RDS.TextParser.Types
{
    public class SourceClient : ISourceClient
    {
        private readonly WebClient _client;

        public SourceClient()
        {
            _client = new WebClient
            {
                Headers =
                {
                    ["User-Agent"] = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)",
                    ["Accept"] = "text/html, application/xhtml+xml, */*"
                }
            };
        }

        public string GetSourceAsString(Uri uri)
        {
            if (!VerifyInternetConnection(uri))
            {
                return string.Empty;
            }

            var html = _client.DownloadString(uri);

            return html;
        }

        private bool VerifyInternetConnection(Uri url)
        {
            try
            {
                var updateDomain = url.Host;
                var hostAddress = Dns.GetHostEntry(updateDomain);
                return true;
            }
            catch (Exception err)
            {
                HandleError(err);
            }

            return false;
        }

        private static void HandleError(Exception err)
        {
            Console.WriteLine(err.Message);
            throw err;
        }
    }
}
