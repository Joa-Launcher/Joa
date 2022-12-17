﻿using JoaLauncher.Api;
using JoaLauncher.Api.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Joa.Step;

public class StepBuilder : IStepBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchResult _pluginSearchResult;

    public StepBuilder(IServiceProvider serviceProvider, ISearchResult pluginSearchResult)
    {
        _serviceProvider = serviceProvider;
        _pluginSearchResult = pluginSearchResult;
    }
    
    private List<IGenericProvider> Providers { get; set; } = new();
    

    public Step Build()
    {
        return new Step
        {
            Providers = Providers.Select(x => new ProviderWrapper
            {
                Provider = x
            }).ToList(),
            Name = _pluginSearchResult.Title
        };
    }

    public IStepBuilder AddProvider<T>() where T : IProvider
    {
        Providers.Add(ActivatorUtilities.CreateInstance<T>(_serviceProvider));
        return this;
    }

    public IStepBuilder AddProvider<TProvider, TContext>(TContext providerContext) where TProvider : IProvider<TContext> where TContext : IProviderContext
    {
        Providers.Add(ActivatorUtilities.CreateInstance<TProvider>(_serviceProvider, providerContext));
        return this;
    }
}