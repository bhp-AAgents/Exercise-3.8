using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace Exercise_3._8
{
    class CrawlerAgent
    {
        // shared by all crawlers

        // needed to check for whether an url has already been crawled
        private static ConcurrentDictionary<string, bool> visitedUrls =
            new ConcurrentDictionary<string, bool>();

        // the found urls containing the search string
        private static BlockingCollection<Uri> resultUrls =
            new BlockingCollection<Uri>(new ConcurrentQueue<Uri>());

        // all urls still to be crawled
        private static BlockingCollection<KeyValuePair<Uri, int>> frontier =
            new BlockingCollection<KeyValuePair<Uri, int>>(new ConcurrentQueue<KeyValuePair<Uri, int>>());

        // cache of all parsed robotstxt pages, so they can be reused for urls pointing to the same
        // web-site (domain)
        private static ConcurrentDictionary<string, RobotsTxtParser> RobotsTxtParsers =
            new ConcurrentDictionary<string, RobotsTxtParser>();
        
        // all agents crawl at most to this level into a web-site 
        private static int maxLevel;

        // all agents look for this searchTerm at the visited pages
        private static string searchTerm;

        // local for each crawler

        // the crawlerAgemt is threaded out to work simultaneously in parallel.
        private Thread crawler;

        // the Webclient used to extract pages.
        private WebClient wc;

        // used to signal termination of the crawlAgent
        private bool done;

        // initial webcrawler. Must be setup first.
        // instantiates the frontier for ALL crawlers.
        public CrawlerAgent(string aSearchTerm, Queue<Uri> seedingUrls, int maximumLevel)
           : this()  // call to default constructor (below)
        {
            searchTerm = aSearchTerm;
            maxLevel = maximumLevel;
            foreach (Uri url in seedingUrls)
                frontier.Add(new KeyValuePair<Uri, int>(url, 0));
        }

        // worker crawlerAgent fetching urls from the frontier
        public CrawlerAgent()
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            wc = new WebClient();            
            wc.Headers.Add(HttpRequestHeader.UserAgent, "bhp-bot; CS student practice crawler; Developer: Bent H. Pedersen (bhp@easv.dk)");
            wc.Headers.Add(HttpRequestHeader.From, "bhp@easv.dk");
            
            crawler = new Thread(() => Crawl());
            crawler.Start();
        }

        private void Crawl()
        {
            done = false;
            while (!done)
            {
                try
                {
                    // fetch an url to be crawled from the frontier
                    KeyValuePair<Uri, int> keyValue = frontier.Take();
                    Uri url = keyValue.Key;
                    int level = keyValue.Value;

                    // create url for the robots.txt page
                    Uri robotsTxtUrl = new UriBuilder($"{url.Host}/robots.txt").Uri;

                    // just a string version of the above Uri for convenience
                    string robotsTxtUrlStr = robotsTxtUrl.ToString().ToLower();

                    // RobotsTxtParser not in the cache?
                    if (!RobotsTxtParsers.ContainsKey(robotsTxtUrlStr))
                    {
                        // then create it and add to the cache
                        RobotsTxtParsers.TryAdd(robotsTxtUrlStr, new RobotsTxtParser(robotsTxtUrl));
                    }

                    // fetch the RobotsTxtParser from the cache
                    var robotsTxtParser = RobotsTxtParsers[robotsTxtUrlStr];

                    // if the fetched url in allowed to be crawled...
                    if (robotsTxtParser.IsUrlAllowed(url))
                    {
                        // then scrape the page
                        ResolvePage(url, level);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    // nothing special to do, just wake up !
                }
            }

        }

        private void ResolvePage(Uri url, int level)
        {
            var urlStr = url.ToString();

            if (visitedUrls.ContainsKey(urlStr)) return;

            visitedUrls[urlStr] = true;

            try
            {
                // try to download the webpage. Throws exception if the url is bad.
                string webPage = wc.DownloadString(urlStr);

                // If the webpage does not contain the search term, just skip it.
                if (!webPage.ToLower().Contains(searchTerm.ToLower())) return;

                // Page contains search term, so add it to the results.
                resultUrls.Add(url);

                if (level < maxLevel)
                {
                    // look for links in the webpage
                    var urlTagPattern = new Regex(@"<a.*?href\s*=\s*[""'](?<url>.*?)[""'].*?</a>", RegexOptions.IgnoreCase);

                    var links = urlTagPattern.Matches(webPage);

                    Uri baseUrl = new UriBuilder(url.Host).Uri;

                    foreach (Match link in links)
                    {
                        try
                        {
                            string newUrl = link.Groups["url"].Value;
                            Uri absoluteUrl = NormalizedUrl(baseUrl, newUrl);

                            // if the url is for a webpage, add it to the frontier for crawling
                            if (absoluteUrl != null && absoluteUrl.HostNameType == UriHostNameType.Dns && !visitedUrls.ContainsKey(absoluteUrl.ToString()))
                            {
                                frontier.Add(new KeyValuePair<Uri, int>(absoluteUrl, level + 1));
                            }
                        }
                        catch
                        {
                            //just continue with the next found link...
                        }
                    }
                }
            }
            catch
            {
                // Unable to load page
                visitedUrls[urlStr] = false;
            }
        }

        private Uri NormalizedUrl(Uri baseUrl, string newUrl)
        {
            newUrl = newUrl.ToLower();
            if (Uri.TryCreate(newUrl, UriKind.RelativeOrAbsolute, out var url))
            {
                return (Uri.TryCreate(baseUrl, url, out Uri absoluteUrl) ? absoluteUrl : null);
            }
            return null;
        }

        public void Stop()
        {
            done = true;
            if (crawler.ThreadState == ThreadState.WaitSleepJoin)
            { 
                crawler.Interrupt();
            }
        }

        public static int GetFrontierSize()
        {
            return frontier.Count;
        }

        public static Queue<Uri> GetResultUrls()
        {
            return new Queue<Uri>(resultUrls);
        }
    }
}
