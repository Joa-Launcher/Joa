﻿using System.Text.Json.Serialization;
using JoaPluginsPackage;
using JoaPluginsPackage.Attributes;
using JoaPluginsPackage.Plugin;

namespace JoaCore.PluginCore;

public class PluginDefinition
{
    public Guid Id { get; }


    [JsonIgnore]
    public IPlugin Plugin { get; set; }
    public PluginAttribute PluginInfo { get; set; }
    public List<SearchResultProviderWrapper> SearchResultProviders { get; set; }
    public List<ISetting> Settings { get; set; }
    public List<ISearchResult> SearchResults { get; set; }
    
    public PluginDefinition()
    {
        Id = Guid.NewGuid();
    }
}