using Orchestrate.Io;


public static class SearchHelper
{
    public static void WaitForConsistency(Collection collection, int searchCount)
    {
        int count = 0;
        SearchResults<dynamic> searchResults;
        do
        {
            searchResults =
                AsyncHelper.RunSync<SearchResults<dynamic>>(() => collection.SearchAsync<dynamic>("*"));
            count++;
        }
        while (searchResults.Count < searchCount);
    }
}

