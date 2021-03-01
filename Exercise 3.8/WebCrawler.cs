using System;
using System.Collections.Generic;

namespace Exercise_3._8
{
    class WebCrawler
    {
        private List<CrawlerAgent> Crawlers = null;

        public WebCrawler(string searchString, int levelsToCrawl, int maxCrawlerAgents)
        {
            GoogleCustomSearchEngine gse = new GoogleCustomSearchEngine(searchString);
            Queue<Uri> seedingUrls = new Queue<Uri>(gse.GetResultLinks());
            
            Crawlers = new List<CrawlerAgent>();
            Crawlers.Add(new CrawlerAgent(searchString, seedingUrls, levelsToCrawl));
            for (int i = 1; i< maxCrawlerAgents; i++)
            { 
                Crawlers.Add(new CrawlerAgent());
            }
        }

        public int GetFrontierSize()
        {
            return CrawlerAgent.GetFrontierSize();
        }

        public void Stop()
        {
            foreach(CrawlerAgent crawler in Crawlers)
            {
                crawler.Stop();
            }
        }

        public Queue<Uri> GetResultUrls()
        {
            return CrawlerAgent.GetResultUrls();
        }

    }
}
