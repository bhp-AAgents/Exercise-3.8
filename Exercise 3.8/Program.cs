using System;
using System.Collections.Generic;

namespace Exercise_3._8
{
    class Program
    {
        static void Main(string[] args)
        {
            // what to search for ?
            Console.Write("Enter search string: ");
            string searchString = Console.ReadLine().Trim();

            // start a web crawler searching for the searchstring
            // at most 3 levels into a web-site
            // and using 20 crawlerAgents
            WebCrawler webCrawler = new WebCrawler(searchString, 3, 20);
 
            Console.WriteLine("\nCrawling...");

            bool done = false;
            while (!done)
            {
                Console.WriteLine("\nPress any key to see status (ESC to stop)...");

                // if ESC is pressed then stop
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    done = true;
                }

                // Prints out the size of the frontier queue
                Console.WriteLine("\nFrontier size = " + webCrawler.GetFrontierSize());
            }

            // Stop the crawlerAgents
            webCrawler.Stop();

            // print the found urls
            PrintResults(webCrawler.GetResultUrls());
        }

        private static void PrintResults(Queue<Uri> results)
        {
            // Print out the result urls containing the search string found by the web crawlers 

            foreach (Uri url in results)
            {
                Console.WriteLine(url);
            }

            // Print out the number of results found by the crawlers
            Console.WriteLine("\nNumber of found results = " + results.Count);
        }
    }
}
