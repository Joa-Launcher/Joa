﻿using Joa.Injectables;
using Joa.PluginCore;
using Joa.Settings;
using Joa.Steps;
using JoaLauncher.Api;
using JoaLauncher.Api.Enums;
using JoaLauncher.Api.Injectables;
using ExecutionContext = Joa.Steps.ExecutionContext;

namespace Joa;

public class Search
{
    private readonly IJoaLogger _logger;
    private readonly PluginServiceProvider _pluginServiceProvider;
    private readonly SettingsManager _settingsManager;

    public Search(IJoaLogger logger, PluginServiceProvider pluginServiceProvider, SettingsManager settingsManager)
    {
        _logger = logger;
        _pluginServiceProvider = pluginServiceProvider;
        _settingsManager = settingsManager;
    }

    public async Task<Step?> ExecuteCommand(SearchResult searchResult, ContextAction contextAction)
    {
        var executionContext = new ExecutionContext(searchResult)
        {
            ContextAction = contextAction,
            ServiceProvider = _pluginServiceProvider.ServiceProvider
        };

        await Task.Run(() => searchResult.Execute(executionContext));

        return executionContext.StepBuilder?.Build();
    }

    public List<PluginSearchResult> UpdateSearchResults(Step step, string searchString)
    {
        using var _ = _logger.TimedOperation(nameof(UpdateSearchResults));

        _logger.Info($"SearchString: ${searchString}");

        var results = step.GetSearchResults(searchString).Take(8).ToList();

        JoaLogger.GetInstance().Info(Environment.CurrentManagedThreadId.ToString());
        
        foreach (var result in results)
        {
            result.SearchResult.Actions ??= new List<ContextAction>();

            if (result.SearchResult.Actions.All(x => x.Key != Key.Enter))
            {
                result.SearchResult.Actions.Add(new ContextAction
                {
                    Id = "Enter",
                    Key = Key.Enter,
                    Name = "Execute"
                });
            }
        }

        return results;
    }
}