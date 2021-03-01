using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace Exercise_3._8
{
    public class RobotsTxtParser
    {
        public List<string> AllowedUrls { get; private set; }
        public List<string> DisallowedUrls { get; private set; }
        public string SitemapUrl { get; private set; }

        public RobotsTxtParser(Uri domainUrl)
        {
            AllowedUrls = new List<string>();
            DisallowedUrls = new List<string>();

            string host = domainUrl.Host;
            Uri robotsTxtUrl = new UriBuilder(host + "/robots.txt").Uri;

            string robotstxt = null;
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add(HttpRequestHeader.UserAgent, "bhp-bot; CS student practice crawler; Developer: Bent H. Pedersen (bhp@easv.dk)");
                wc.Headers.Add(HttpRequestHeader.From, "bhp@easv.dk");
                try
                {
                    robotstxt = wc.DownloadString(robotsTxtUrl.ToString());
                    Parse(robotstxt);
                }
                catch
                {
                    // no robotstxt found for website
                    // use Default empty robotstxt => everything is allowed!
                }
            }


        }

        private void Parse(string robotstxt)
        {
            string[] lines = robotstxt.ToLower().Split("\n");
            int i = 0;
            while (i < lines.Length && !lines[i].Contains("user-agent: *"))
            {
                if (lines[i].Contains("sitemap:"))
                {
                    var lineSegments = lines[i].Split(":");
                    SitemapUrl = lineSegments[1].Trim();
                }
                i++;
            }

            for (int line = i + 1; line < lines.Length && !lines[line].Contains("user-agent"); line++)
            {
                string[] lineSegment = lines[line].Split(':');
                if (lineSegment.Length == 2)
                {
                    string command = lineSegment[0].Trim();
                    string path = lineSegment[1].Trim();
                    if (path.Length > 0)
                    {
                        if (command == "allow")
                        {
                            AllowedUrls.Add(path);
                        }
                        else if (command == "disallow")
                        {
                            DisallowedUrls.Add(path);
                        }
                    }
                }
            }
        }

        public bool IsUrlAllowed(Uri url)
        {
            if (url.ToString().EndsWith("/robots.txt"))
            {
                return false;
            }

            bool allowed = true;
            int count = 0;
            string absolutePath = url.AbsolutePath;

            foreach (var path in DisallowedUrls)
            {
                try
                {
                    Regex reg = new Regex("^" + path.Replace("*", "\\S*"));
                    if (reg.IsMatch(absolutePath))
                    {
                        allowed = false;
                        count = path.Length;
                        break;
                    }
                }
                catch
                {
                    // Unresolvable urlPath => bypass this
                }
            }

            foreach (var path in AllowedUrls)
            {
                try
                {
                    Regex reg = new Regex("^" + path.Replace("*", "\\S*"));
                    if (reg.IsMatch(absolutePath) && path.Length >= count)
                    {
                        allowed = true;
                        break;
                    }
                }
                catch
                {
                    // Unresolvable urlPath => bypass this
                }
            }
            return allowed;
        }
    }
}
