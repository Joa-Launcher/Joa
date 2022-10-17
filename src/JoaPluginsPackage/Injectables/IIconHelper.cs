﻿namespace JoaPluginsPackage.Injectables;

public interface IIconHelper
{
    public string GetIconsDirectory(Type pluginType);

    public string CreateIconFromFileIfNotExists<T>(string fileLocation);
}