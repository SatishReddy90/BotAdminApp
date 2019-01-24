using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace AdminBot.Helpers
{
    /// <summary>
    /// Logic to parse website and get urls
    /// </summary>
    public class UrlExtractor
    {
        /// <summary>
        /// Returns the urls in specified site address
        /// </summary>
        /// <param name="baseUrl">Base Url</param>
        /// <param name="recursive">If true, parses recursively through all links</param>
        /// <returns></returns>
        public IList<string> GetUrls(string url, bool recursive)
        {
            string absoluteBaseUrl = url;
            if (!absoluteBaseUrl.EndsWith("/"))
                absoluteBaseUrl += "/";

            return this.GetUrls(url, absoluteBaseUrl, recursive);
        }

        /// <summary>
        /// Returns the urls in specified site address
        /// </summary>
        /// <param name="url">Base Url</param>
        /// <param name="recursive">If true, parses recursively through all links</param>
        /// <returns></returns>
        public IList<string> GetUrls(string url, string baseUrl, bool recursive)
        {
            if (recursive)
            {
                _urls.Clear();
                RecursivelyGenerateUrls(url, baseUrl);

                return _urls;
            }
            else
                return InternalGetUrls(url, baseUrl);
        }

        /// <summary>
        /// Internal method that recursively generates urls
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="absoluteBaseUrl"></param>
        private void RecursivelyGenerateUrls(string baseUrl, string absoluteBaseUrl)
        {
            var urls = InternalGetUrls(baseUrl, absoluteBaseUrl);

            foreach (string url in urls)
            {
                if (!_urls.Contains(url))
                {
                    _urls.Add(url);

                    string newAbsoluteBaseUrl = GetBasePath(url);
                    RecursivelyGenerateUrls(url, newAbsoluteBaseUrl);
                }
            }
        }

        private string GetBasePath(string baseUrl)
        {
            if (baseUrl.EndsWith("/"))
                baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);

            if (baseUrl.Contains("/"))
            {
                int index = baseUrl.LastIndexOf("/");
                string basePath = baseUrl.Substring(0, index + 1);

                if (!basePath.EndsWith("/"))
                    basePath += "/";

                return basePath;
            }
            return baseUrl;
        }

        private IList<string> _urls = new List<string>();

        private IList<string> InternalGetUrls(string baseUrl, string absoluteBaseUrl)
        {
            IList<string> list = new List<string>();

            Uri uri = null;
            if (!Uri.TryCreate(baseUrl, UriKind.RelativeOrAbsolute, out uri))
                return list;

            // Get the http content
            string siteContent = GetHttpResponse(baseUrl);

            var allUrls = GetAllUrls(siteContent);

            foreach (string uriString in allUrls)
            {
                uri = null;
                if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
                {
                    string[] excludeUrls = new string[] { "#", ""};
                    string val = uri.ToString();
                    if (!excludeUrls.Contains(uri.ToString()))
                    {
                        if (uri.IsAbsoluteUri)
                        {
                            if (uri.OriginalString.StartsWith(absoluteBaseUrl)) // If different domain / javascript: urls needed exclude this check
                            {
                                list.Add(uriString);
                            }
                        }
                        else
                        {
                            string newUri = GetAbsoluteUri(uri, absoluteBaseUrl, uriString);
                            if (!string.IsNullOrEmpty(newUri))
                            {
                                if(newUri.EndsWith("/"))
                                    newUri = newUri.Substring(0, newUri.LastIndexOf("/"));
                                list.Add(newUri);
                            }
                        }
                    }
                }
                else
                {
                    if (!uriString.StartsWith(absoluteBaseUrl))
                    {
                        string newUri = GetAbsoluteUri(uri, absoluteBaseUrl, uriString);
                        if (!string.IsNullOrEmpty(newUri))
                        {
                            if (newUri.EndsWith("/"))
                                newUri = newUri.Substring(0, newUri.LastIndexOf("/"));
                            list.Add(newUri);
                        }
                    }
                }
            }

            return list;
        }

        private string GetAbsoluteUri(Uri uri, string basePath, string uriString)
        {
            if (!string.IsNullOrEmpty(uriString))
                if (uriString.Contains(":"))
                    if (!uriString.Contains("http:"))
                        return string.Empty;

            string newUriString = string.Empty;
            basePath = GetResolvedBasePath(basePath, uriString);

            Uri userUri = new Uri(basePath);
            if (uriString.StartsWith("//"))
            {
                Uri extractedUri = new Uri(userUri.Scheme + ":" + uriString);
                if (extractedUri.Host.ToLower().Replace("www.", "") != userUri.Host.ToLower().Replace("www.", ""))
                    return string.Empty;
                if ((extractedUri.Host != userUri.Host &&
                    extractedUri.Host.ToLower().Replace("www.", "") == userUri.Host.ToLower().Replace("www.", "")) ||
                    extractedUri.Host.ToLower() == userUri.Host.ToLower()
                    )
                {
                    newUriString = userUri.Scheme + ":";
                }
                else
                {
                    newUriString = basePath;
                }
            }
            else
            {
                newUriString = basePath;
            }

            uriString = uriString.Replace("../", string.Empty);

            uri = null;
            
            if (!newUriString.EndsWith("/"))
                newUriString += "/";

            newUriString += uriString;

            newUriString = newUriString.Replace("//", "/");
            newUriString = newUriString.Replace("http:/", "http://");
            newUriString = newUriString.Replace("///", "//");
            newUriString = newUriString.Replace("/#", "/");

            if (Uri.TryCreate(newUriString, UriKind.RelativeOrAbsolute, out uri))
                return newUriString;

            return string.Empty;
        }

        private string GetResolvedBasePath(string basePath, string uriString)
        {
            int count = GetCountOf("../", uriString);
            for (int i = 1; i <= count; i++)
            {
                basePath = GetBasePath(basePath);
            }

            return basePath;
        }

        private int GetCountOf(string pattern, string str)
        {
            int count = 0;
            int index = -1;

            while (true)
            {
                index = str.IndexOf(pattern, index + 1);
                if (index == -1)
                    break;

                count++;
            }

            return count;
        }

        /// <summary>
        /// Returns all urls in string content
        /// [Includes javascrip:, mailto:, other domains too]
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string[] GetAllUrls(string str)
        {
            //string pattern = @"<a.*?href=[""'](?<url>.*?)[""'].*?>(?<name>.*?)</a>";
            //string pattern = "<a (.*\\s)?href=\"?'?(.*?)\"?'?(\\s.*)?>";

            //System.Text.RegularExpressions.MatchCollection matches
            //    = System.Text.RegularExpressions.Regex.Matches(str, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            //string[] matchList = new string[matches.Count];

            //int c = 0;

            //foreach (System.Text.RegularExpressions.Match match in matches)
            //{
            //    //matchList[c++] = match.Groups["url"].Value;
            //    matchList[c++] = match.Groups[2].Value;
            //}
            //return matchList;

            Regex rr = new Regex("<a (.*\\s)?href=\"?'?(.*?)\"?'?(\\s.*)?>");
            Match m = rr.Match(str);
            List<string> urlList = new List<string>();
            while (m.Success)
            {
                urlList.Add(m.Groups[2].Value);

                m = m.NextMatch();
            }
            return urlList.ToArray();
        }

        /// <summary>
        /// Returns the response content as string for given url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetHttpResponse(string url)
        {
            try
            {
                ASCIIEncoding encoding = new ASCIIEncoding();

                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.Method = "GET";

                HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse();

                return GetResponseContent(response);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return String.Empty;
        }

        #region "Exception Handling"

        public delegate void OnExceptionDelegate(Exception ex);

        /// <summary>
        /// OnException delegate can be used to handle the exceptions inside this class
        /// </summary>
        public OnExceptionDelegate OnException;

        private void HandleException(Exception ex)
        {
            if (OnException != null)
                OnException(ex);
        }

        #endregion

        /// <summary>
        /// Returns the string content of HttpWebResponse
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private string GetResponseContent(HttpWebResponse response)
        {
            if (response == null)
                return String.Empty;

            StringBuilder builder = new StringBuilder();
            Stream stream = response.GetResponseStream();

            StreamReader streamReader = new StreamReader(stream);

            int data = 0;
            do
            {
                data = streamReader.Read();
                if (data > -1)
                    builder.Append((char)data);
            }
            while (data > -1);

            streamReader.Close();

            return builder.ToString();
        }
    }
}
