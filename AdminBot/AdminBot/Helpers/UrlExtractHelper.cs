using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace AdminBot.Helpers
{
    public class UrlExtractHelper
    {
        //public method called from your application
        public List<string> RetrieveUrls(string webPage)
        {
            return GetAllUrls(RetrieveContent(webPage));
        }

        //get the content of the web page passed in
        private string RetrieveContent(string webPage)
        {
            HttpWebResponse response = null;//used to get response
            StreamReader respStream = null;//used to read response into string
            try
            {
                //create a request object using the url passed in
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webPage);
                request.Timeout = 10000;

                //go get a response from the page
                response = (HttpWebResponse)request.GetResponse();

                //create a streamreader object from the response
                respStream = new StreamReader(response.GetResponseStream());

                //get the contents of the page as a string and return it
                return respStream.ReadToEnd();
            }
            catch (Exception ex)//houston we have a problem!
            {
                throw ex;
            }
            finally
            {
                //close it down, we’re going home!
                response.Close();
                respStream.Close();
            }
        }

        //using a regular expression, find all of the href or urls
        //in the content of the page
        private List<string> GetAllUrls(string content)
        {
            List<string> urlList = new List<string>();
            //regular expression
            string pattern = @"(?:href\s*=)(?:[\s""']*)(?!#|mailto|location.|javascript|.*css|.*this\.)(?.*?)(?:[\s>""'])";
              
            //Set up regex object
              //Regex RegExpr = new Regex(pattern, RegexOptions.IgnoreCase);

            //get the first match
            //Match match = RegExpr.Match(content);

            Regex rr = new Regex("<a (.*\\s)?href=\"?'?(.*?)\"?'?(\\s.*)?>");
            Match m = rr.Match(content);

            //loop through matches
            //while (match.Success)
            while (m.Success)
            {
                urlList.Add(m.Groups[2].Value);
                //output the match info
                //Console.WriteLine("href match: " +match.Groups[0].Value);
                //WriteToLog("C:\\matchlog.txt", "href match: " +match.Groups[0].Value + "\r\n");
                //urlList.Add(match.Groups[0].Value);

                //Console.WriteLine("Url match: " +match.Groups[1].Value);
                //WriteToLog("C:\\matchlog.txt", "Url | Location | mailto match: " +match.Groups[1].Value + "\r\n");

                //get next match
                //match = match.NextMatch();
                m = m.NextMatch();
            }
            return urlList;
        }

        //Write to a log file
        private void WriteToLog(string file, string message)
        {
            using (StreamWriter w = File.AppendText(file))
            {
                w.WriteLine(DateTime.Now.ToString() + ": " +message); w.Close();
            }
        }
    }
}
