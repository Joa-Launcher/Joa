﻿using JoaKit;

#pragma warning disable CS0657
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(HotReloadManager))]
#pragma warning restore CS0657

namespace JoaKit;

public static class HotReloadManager
{
    public static void ClearCache(Type[]? updatedTypes)
    {
        Console.WriteLine("HotReloadManager.ClearCache");
    }

    public static void UpdateApplication(Type[]? updatedTypes)
    {
        foreach (var manager in JoaKitApp.WindowManagers)
        {
            manager.Builder.ShouldRebuild(manager.RootComponent);
        }

        Console.WriteLine("HotReloadManager.UpdateApplication");
    }
}
