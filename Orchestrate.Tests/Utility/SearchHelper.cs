﻿using System.Threading.Tasks;
using Orchestrate.Io;


public static class SearchHelper
{
    public async static Task WaitForConsistency(Collection collection, string query, int searchCount)
    {
        int count = 0;
        SearchResults<dynamic> searchResults;
        do
        {
            searchResults = await collection.SearchAsync<dynamic>(query);
            count++;
        }
        while (searchResults.Count < searchCount);

        return;
    }
}

