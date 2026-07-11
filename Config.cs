using System.Collections.Generic;
using MapGeneration;

namespace ItemManager
{
    public class AutoSpawnRule
    {
        /// <summary>Zone whose rooms are used for this rule.</summary>
        public FacilityZone Zone { get; set; } = FacilityZone.LightContainment;

        /// <summary>Optional room filter. Empty = any room in the zone.</summary>
        public List<RoomName> Rooms { get; set; } = new List<RoomName>();

        /// <summary>Item pool; each spawn picks a random entry.</summary>
        public List<ItemType> Items { get; set; } = new List<ItemType> { ItemType.Medkit };

        /// <summary>How many items this rule spawns per round.</summary>
        public int Count { get; set; } = 3;

        /// <summary>Attach the colored glow light to these items.</summary>
        public bool Glow { get; set; } = true;
    }

    public class Config
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool CleanerEnabled { get; set; } = true;
        public float CleanerStartMinutes { get; set; } = 20f;
        public float CleanerIntervalSeconds { get; set; } = 60f;
        public bool CleanerRemoveRagdolls { get; set; } = true;
        public bool CleanerAllLczItems { get; set; } = true;
        public List<ItemType> CleanerLowTierItems { get; set; } = new List<ItemType>
        {
            ItemType.KeycardJanitor,
            ItemType.KeycardScientist,
            ItemType.KeycardGuard,
            ItemType.GunCOM15,
            ItemType.GunCOM18,
            ItemType.Flashlight,
            ItemType.Radio,
            ItemType.Coin,
            ItemType.Painkillers,
            ItemType.Ammo9x19,
            ItemType.Ammo12gauge,
        };

        public bool MoverEnabled { get; set; } = true;
        public float MoverTargetX { get; set; } = 0.007f;
        public float MoverTargetY { get; set; } = 301.96f;
        public float MoverTargetZ { get; set; } = 0.125f;
        public float MoverScatterRadius { get; set; } = 3.5f;
        public float MoverDropHeight { get; set; } = 1.2f;
        public float MoverCollectRadius { get; set; } = 8f;
        public float MoverDelaySeconds { get; set; } = 0.4f;
        /// <summary>Automatically spawn glowing items in random rooms at round start.</summary>
        public bool AutoSpawnerEnabled { get; set; } = true;

        /// <summary>Delay (seconds) after round start before the automatic spawns happen.</summary>
        public float AutoSpawnDelaySeconds { get; set; } = 5f;

        public List<AutoSpawnRule> AutoSpawnRules { get; set; } = new List<AutoSpawnRule>
        {
            // medical
            new AutoSpawnRule
            {
                Zone = FacilityZone.LightContainment,
                Items = new List<ItemType> { ItemType.Medkit, ItemType.Painkillers, ItemType.Adrenaline, ItemType.SCP500 },
                Count = 6,
            },
            new AutoSpawnRule
            {
                Zone = FacilityZone.HeavyContainment,
                Items = new List<ItemType> { ItemType.Medkit, ItemType.Adrenaline, ItemType.Painkillers },
                Count = 5,
            },
            new AutoSpawnRule
            {
                Zone = FacilityZone.Entrance,
                Items = new List<ItemType> { ItemType.Medkit, ItemType.Painkillers },
                Count = 4,
            },

            // keycards: low tier in LCZ, better ones deeper in the facility
            new AutoSpawnRule
            {
                Zone = FacilityZone.LightContainment,
                Items = new List<ItemType> { ItemType.KeycardJanitor, ItemType.KeycardScientist, ItemType.KeycardZoneManager },
                Count = 4,
            },
            new AutoSpawnRule
            {
                Zone = FacilityZone.HeavyContainment,
                Items = new List<ItemType> { ItemType.KeycardGuard, ItemType.KeycardMTFPrivate, ItemType.KeycardResearchCoordinator },
                Count = 3,
            },
            new AutoSpawnRule
            {
                Zone = FacilityZone.Entrance,
                Items = new List<ItemType> { ItemType.KeycardMTFOperative, ItemType.KeycardFacilityManager },
                Count = 2,
            },

            // guns: pistols in LCZ, stronger guns in HCZ/EZ, always with matching ammo in the pool
            new AutoSpawnRule
            {
                Zone = FacilityZone.LightContainment,
                Items = new List<ItemType> { ItemType.GunCOM15, ItemType.GunCOM18, ItemType.Ammo9x19 },
                Count = 4,
            },
            new AutoSpawnRule
            {
                Zone = FacilityZone.HeavyContainment,
                Items = new List<ItemType> { ItemType.GunFSP9, ItemType.GunCrossvec, ItemType.GunShotgun, ItemType.GunRevolver, ItemType.Ammo9x19, ItemType.Ammo12gauge, ItemType.Ammo44cal },
                Count = 4,
            },
            new AutoSpawnRule
            {
                Zone = FacilityZone.Entrance,
                Items = new List<ItemType> { ItemType.GunCrossvec, ItemType.GunE11SR, ItemType.Ammo9x19, ItemType.Ammo556x45 },
                Count = 3,
            },

            // armor
            new AutoSpawnRule
            {
                Zone = FacilityZone.HeavyContainment,
                Items = new List<ItemType> { ItemType.ArmorLight, ItemType.ArmorCombat },
                Count = 2,
            },

            // scp stuff (rare)
            new AutoSpawnRule
            {
                Zone = FacilityZone.HeavyContainment,
                Items = new List<ItemType> { ItemType.SCP207, ItemType.AntiSCP207, ItemType.SCP018, ItemType.SCP2176, ItemType.SCP244a },
                Count = 2,
            },
            new AutoSpawnRule
            {
                Zone = FacilityZone.Entrance,
                Items = new List<ItemType> { ItemType.SCP268, ItemType.SCP1853, ItemType.SCP1576 },
                Count = 1,
            },
        };

        public float SpawnerDefaultRadius { get; set; } = 4f;
        public int SpawnerMaxItems { get; set; } = 40;
        public bool SpawnerGlowEnabled { get; set; } = true;
        public float SpawnerGlowRange { get; set; } = 3f;
        public float SpawnerGlowIntensity { get; set; } = 0.7f;
        public Dictionary<ItemType, string> GlowColors { get; set; } = new Dictionary<ItemType, string>
        {
            // medical
            [ItemType.Medkit] = "#2ECC40",
            [ItemType.Painkillers] = "#A3E4D7",
            [ItemType.Adrenaline] = "#00E5FF",
            [ItemType.SCP500] = "#FF4136",

            // scp items
            [ItemType.SCP207] = "#8E44AD",
            [ItemType.AntiSCP207] = "#F1C40F",
            [ItemType.SCP268] = "#5D6D7E",
            [ItemType.SCP018] = "#FF3B30",
            [ItemType.SCP2176] = "#7FFF6E",
            [ItemType.SCP244a] = "#66CCFF",
            [ItemType.SCP244b] = "#66CCFF",
            [ItemType.SCP1853] = "#E67E22",
            [ItemType.SCP1576] = "#F5DEB3",
            [ItemType.SCP330] = "#FF69B4",

            // keycards
            [ItemType.KeycardJanitor] = "#A2B8C4",
            [ItemType.KeycardScientist] = "#F4D03F",
            [ItemType.KeycardResearchCoordinator] = "#F5B041",
            [ItemType.KeycardZoneManager] = "#16A085",
            [ItemType.KeycardGuard] = "#95A5A6",
            [ItemType.KeycardMTFPrivate] = "#5DADE2",
            [ItemType.KeycardMTFOperative] = "#3498DB",
            [ItemType.KeycardMTFCaptain] = "#1F618D",
            [ItemType.KeycardFacilityManager] = "#C0392B",
            [ItemType.KeycardChaosInsurgency] = "#186A3B",
            [ItemType.KeycardO5] = "#FDFEFE",

            // explosives
            [ItemType.GrenadeHE] = "#E74C3C",
            [ItemType.GrenadeFlash] = "#F8F9F9",

            // gear / utility
            [ItemType.ArmorLight] = "#7F8C8D",
            [ItemType.ArmorCombat] = "#707B7C",
            [ItemType.ArmorHeavy] = "#616A6B",
            [ItemType.Radio] = "#2E86C1",
            [ItemType.Flashlight] = "#F7DC6F",
            [ItemType.Lantern] = "#F5B041",
            [ItemType.Coin] = "#F1C40F",

            // special weapons
            [ItemType.MicroHID] = "#3498DB",
            [ItemType.Jailbird] = "#F39C12",
            [ItemType.ParticleDisruptor] = "#8E44AD",
        };
        public string GlowFallbackColor { get; set; } = "#FFFFFF";
}   }