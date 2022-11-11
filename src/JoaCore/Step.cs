﻿using JoaCore.SearchEngine;
using JoaPluginsPackage;

namespace JoaCore;

public class Step
{
    public required List<ProviderWrapper> Providers { get; set; }

    public Guid StepId { get; set; } = Guid.NewGuid();
    
    private List<PluginSearchResult> _lastSearchResults;

    public Step()
    {
        StringMatcher.Instance = new StringMatcher();
        _lastSearchResults = new List<PluginSearchResult>();
    }

    public ISearchResult GetSearchResultFromId(Guid id)
    {
        return _lastSearchResults.FirstOrDefault(x => x.CommandId == id)?.SearchResult 
               ?? throw new Exception("Could not find Search Result");
    }
    
    public List<PluginSearchResult> GetSearchResults(string searchString)
    {
        foreach (var providerWrapper in Providers.Where(x => x.Condition is not null))
        {
            providerWrapper.Condition.
        }
        
        _lastSearchResults = Providers
            .SelectMany(x => x.Provider.GetSearchResults(searchString)).ToList().ToPluginSerachResults();

        return SortSearchResults(_lastSearchResults, searchString);
    }
    
    private List<PluginSearchResult> SortSearchResults(List<PluginSearchResult> input, string searchString)
    {
        var sortValues = input.Select(x => 
            (x, StringMatcher.FuzzySearch(searchString, x.SearchResult.Caption).Score)).ToList();
        
        sortValues.Sort((x, y) =>
        {
            if (x.Item2 > y.Item2)
                return -1;
            return x.Item2 < y.Item2 ? 1 : 0;
        });

        return sortValues.Select(x => x.x).ToList();
    }
}