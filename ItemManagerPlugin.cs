using System;
using ItemManager.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
namespace ItemManager
{
    public sealed class ItemManagerPlugin : Plugin<Config>
    {
        public static ItemManagerPlugin Instance { get; private set; }
        internal static Config Cfg => Instance.Config;
        public override string Name => "ItemManager";
        public override string Description => "LCZ item/corpse cleanup, death drops teleported to the surface, glowing item spawner.";
        public override string Author => "adasjusk";
        public override Version Version => new Version(1, 1, 0);
        public override Version RequiredApiVersion => new Version(1, 1, 7);
        private LczCleaner _cleaner;
        private DeathItemMover _mover;
        private AutoSpawner _autoSpawner;
        public override void Enable()
        {
            Instance = this;

            if (!Config.IsEnabled)
            {
                Logger.Warn("ItemManager is disabled via config.");
                return;
            }

            _cleaner = new LczCleaner();
            _mover = new DeathItemMover();
            _autoSpawner = new AutoSpawner();
            _cleaner.Start();
            _mover.Start();
            _autoSpawner.Start();
            ItemGlow.Start();
            Logger.Info($"ItemManager v{Version} enabled. Cleaner: {Config.CleanerEnabled} ({Config.CleanerStartMinutes} min), Mover: {Config.MoverEnabled}, Auto spawner: {Config.AutoSpawnerEnabled}, Spawner glow: {Config.SpawnerGlowEnabled}.");
        }

        public override void Disable()
        {
            _cleaner?.Stop();
            _mover?.Stop();
            _autoSpawner?.Stop();
            ItemGlow.Stop();
            _cleaner = null;
            _mover = null;
            _autoSpawner = null;
            Instance = null;
            Logger.Info("ItemManager disabled.");
}   }   }