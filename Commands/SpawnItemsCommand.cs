using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommandSystem;
using ItemManager.Features;
using LabApi.Features.Wrappers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ItemManager.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public sealed class SpawnItemsCommand : ICommand
    {
        private static Config Cfg => ItemManagerPlugin.Cfg;

        private static readonly Dictionary<string, ItemType[]> Presets = new Dictionary<string, ItemType[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["medical"] = new[]
            {
                ItemType.Medkit, ItemType.Painkillers, ItemType.Adrenaline, ItemType.SCP500,
            },
            ["weapons"] = new[]
            {
                ItemType.GunCOM15, ItemType.GunCOM18, ItemType.GunFSP9, ItemType.GunCrossvec,
                ItemType.GunE11SR, ItemType.GunAK, ItemType.GunShotgun, ItemType.GunRevolver,
            },
            ["keycards"] = new[]
            {
                ItemType.KeycardJanitor, ItemType.KeycardScientist, ItemType.KeycardResearchCoordinator,
                ItemType.KeycardZoneManager, ItemType.KeycardGuard, ItemType.KeycardMTFPrivate,
                ItemType.KeycardMTFOperative, ItemType.KeycardMTFCaptain, ItemType.KeycardFacilityManager,
                ItemType.KeycardChaosInsurgency, ItemType.KeycardO5,
            },
            ["grenades"] = new[]
            {
                ItemType.GrenadeHE, ItemType.GrenadeFlash, ItemType.SCP018, ItemType.SCP2176,
            },
            ["scp"] = new[]
            {
                ItemType.SCP500, ItemType.SCP207, ItemType.AntiSCP207, ItemType.SCP268,
                ItemType.SCP1853, ItemType.SCP244a, ItemType.SCP244b, ItemType.SCP018,
                ItemType.SCP2176, ItemType.SCP1576, ItemType.SCP330,
            },
            ["ammo"] = new[]
            {
                ItemType.Ammo9x19, ItemType.Ammo556x45, ItemType.Ammo762x39,
                ItemType.Ammo12gauge, ItemType.Ammo44cal,
            },
            ["armor"] = new[]
            {
                ItemType.ArmorLight, ItemType.ArmorCombat, ItemType.ArmorHeavy,
            },
        };

        public string Command => "itemspawn";
        public string[] Aliases => new[] { "ispawn", "si" };
        public string Description => "Spawns glowing items scattered around you. Usage: itemspawn <item|preset|list> [count] [radius]";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission(PlayerPermissions.GivingItems, out response))
                return false;

            Player player = Player.Get(sender);
            if (player == null)
            {
                response = "This command can only be used by a player connected to the server.";
                return false;
            }

            string[] args = arguments.ToArray();
            if (args.Length == 0)
            {
                response = $"Usage: itemspawn <item|preset|list> [count] [radius]. Presets: {string.Join(", ", Presets.Keys)}, random.";
                return false;
            }

            if (string.Equals(args[0], "list", StringComparison.OrdinalIgnoreCase))
            {
                response = "Presets: " + string.Join(", ", Presets.Keys) + ", random.\n"
                         + "Any ItemType name also works (e.g. Medkit, KeycardO5, GunAK, SCP500). Partial names match too (e.g. 'med', 'o5').";
                return true;
            }

            int count = 5;
            if (args.Length >= 2 && (!int.TryParse(args[1], out count) || count < 1))
            {
                response = "Count must be a positive whole number.";
                return false;
            }

            count = Mathf.Clamp(count, 1, Cfg.SpawnerMaxItems);
            float radius = Cfg.SpawnerDefaultRadius;
            if (args.Length >= 3 && !float.TryParse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture, out radius))
            {
                response = "Radius must be a number (meters).";
                return false;
            }

            radius = Mathf.Clamp(radius, 1f, 20f);
            ItemType[] pool = ResolvePool(args[0], out string poolName, out string error);
            if (pool == null)
            {
                response = error;
                return false;
            }

            int spawned = 0;
            for (int i = 0; i < count; i++)
            {
                ItemType type = pool.Length == 1 ? pool[0] : pool[Random.Range(0, pool.Length)];
                if (SpawnScattered(type, player.Position, radius))
                    spawned++;
            }

            response = $"Spawned {spawned}/{count} {poolName} item(s) scattered within {radius:0.#}m around you.";
            return true;
        }

        private static ItemType[] ResolvePool(string token, out string poolName, out string error)
        {
            error = null;

            if (Presets.TryGetValue(token, out ItemType[] preset))
            {
                poolName = token.ToLowerInvariant();
                return preset;
            }

            if (string.Equals(token, "random", StringComparison.OrdinalIgnoreCase))
            {
                poolName = "random";
                return Presets.Values.SelectMany(x => x).Distinct().ToArray();
            }

            if (Enum.TryParse(token, true, out ItemType exact) && exact != ItemType.None)
            {
                poolName = exact.ToString();
                return new[] { exact };
            }

            List<ItemType> matches = new List<ItemType>();
            foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            {
                if (type == ItemType.None)
                    continue;

                if (type.ToString().IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    matches.Add(type);
            }

            if (matches.Count == 1)
            {
                poolName = matches[0].ToString();
                return new[] { matches[0] };
            }

            poolName = null;
            error = matches.Count == 0
                ? $"Unknown item or preset '{token}'. Use 'itemspawn list'."
                : $"'{token}' is ambiguous: {string.Join(", ", matches.Take(8))}{(matches.Count > 8 ? ", ..." : string.Empty)}";
            return null;
        }

        private static bool SpawnScattered(ItemType type, Vector3 origin, float radius)
        {
            Vector2 offset = Random.insideUnitCircle * radius;
            if (offset.sqrMagnitude < 0.5f)
                offset = Random.insideUnitCircle.normalized * 0.75f;
            Vector3 column = origin + new Vector3(offset.x, 0f, offset.y);
            Vector3 eye = origin + Vector3.up * 0.5f;
            Vector3 target = column + Vector3.up * 0.5f;
            if (Physics.Linecast(eye, target, out RaycastHit wallHit, -1, QueryTriggerInteraction.Ignore))
                column = wallHit.point + (eye - target).normalized * 0.4f;

            float y;
            if (Physics.Raycast(column + Vector3.up * 0.6f, Vector3.down, out RaycastHit groundHit, 6f, -1, QueryTriggerInteraction.Ignore))
                y = groundHit.point.y + 0.35f;
            else
                y = origin.y + 0.2f;

            Pickup pickup = Pickup.Create(type, new Vector3(column.x, y + Random.Range(0f, 0.4f), column.z), Random.rotation);
            if (pickup == null)
                return false;

            ItemGlow.Attach(pickup);
            return true;
}   }   }