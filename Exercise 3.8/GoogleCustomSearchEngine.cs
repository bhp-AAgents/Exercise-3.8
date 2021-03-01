using Google.Apis.Customsearch.v1;
using Google.Apis.Customsearch.v1.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;

namespace Exercise_3._8
{
    class GoogleCustomSearchEngine
    {
        private readonly IList<Result> Results;

        public GoogleCustomSearchEngine(string searchString)
        {
            const string apiKey = "AIzaSyC3PlCIrvhCYZhidmEUi7z4oVOrOIrw8EI";
            const string searchEngineId = "015351139142946479508:3lihax8uevs";
            var customSearchService = new CustomsearchService(new BaseClientService.Initializer { ApiKey = apiKey });
            var listRequest = customSearchService.Cse.List();
            listRequest.Cx = searchEngineId;
            listRequest.Q = searchString;

            Results = listRequest.Execute().Items;
        }

        public List<Uri> GetResultLinks()
        {
            List<Uri> links = new List<Uri>();
            foreach (var result in Results)
            {
                links.Add(new UriBuilder(result.Link).Uri);
            }
            return links;
        }

    }
}
