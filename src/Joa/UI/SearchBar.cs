﻿using System.Runtime.InteropServices;
using Joa.Hotkey;
using Joa.PluginCore;
using Joa.Steps;
using JoaKit;
using JoaLauncher.Api;
using Modern.WindowKit;
using Modern.WindowKit.Input;
using Modern.WindowKit.Platform;
using Key = Modern.WindowKit.Input.Key;

namespace Joa.UI;

public class SearchBar : IComponent
{
    private readonly IWindowImpl _window;
    private string _input = string.Empty;
    private List<PluginSearchResult> _searchResults = new();
    private int _selectedResult;
    private Stack<Step> _steps = new();
    private readonly Search _search;
    public const int SearchBoxHeight = 60;
    private const int StepsHeight = 30;
    private const int SearchResultHeight = 60;
    public const int Width = 600;

    public SearchBar(IWindowImpl window, GlobalHotKey globalHotKey, Search search, PluginManager pluginManager)
    {
        _window = window;
        _search = search;

        window.LostFocus = HideWindow;

        SearchResultsHaveChanged();

        _steps.Push(new Step
        {
            Providers = pluginManager.GlobalProviders,
            Name = "Global Step",
            Options = new StepOptions()
        });

        globalHotKey.InitialHotKeyRegistration(() =>
        {
            _window.Show(true, false);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            External.SetFocus(_window.Handle.Handle);
            External.SetForegroundWindow(_window.Handle.Handle);
        });
    }

    public RenderObject Build()
    {
        return new Div
        {
            new Div
                {
                    new Img("./battery.svg"),
                    new InputComponent()
                }.Color(40, 40, 40)
                .XAlign(XAlign.Center)
                .Padding(10)
                .Gap(10)
                .Height(SearchBoxHeight)
                .Dir(Dir.Horizontal),
            new Div()
                .Items(_steps.Reverse().Select(x =>
                    new Div
                        {
                            new Txt(x.Name).VAlign(TextAlign.Center).HAlign(TextAlign.Center)
                        }.Width(100)
                        .MAlign(MAlign.Center)
                        .Color(60, 60, 60)
                        .Radius(5)
                )).Dir(Dir.Horizontal)
                .Padding(4)
                .Gap(8)
                .Height(StepsHeight)
                .Color(40, 40, 40),
            new Div()
                .Items(_searchResults.Select((x, i) =>
                    new SearchResultComponent(x, _selectedResult == i)
                        .Key(x.SearchResult.Title)
                ))
        };
    }

    private void TextChanged()
    {
        if (_input == string.Empty && _steps.Count == 1)
        {
            _searchResults.Clear();
        }
        else
        {
            _searchResults = _search.UpdateSearchResults(_steps.Peek(), _input);
        }

        SearchResultsHaveChanged();
    }

    private void SearchResultsHaveChanged()
    {
        _selectedResult = 0;
        _window.Resize(new Size(_window.ClientSize.Width,
            SearchBoxHeight + StepsHeight + _searchResults.Count * SearchResultHeight));
    }

    private void HideWindow()
    {
        _window.Hide();
        ClearSteps();
        _input = string.Empty;
        _searchResults.Clear();
        SearchResultsHaveChanged();
    }

    private void ClearSteps()
    {
        while (_steps.Count > 1)
        {
            _steps.Pop();
        }
    }
}