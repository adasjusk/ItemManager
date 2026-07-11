using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace ItemManager.Features
{
    /// <summary>
    /// Attaches a colored point light to spawned pickups so they glow in a color matching the
    /// item (green medkit, cyan adrenaline, red SCP-500, ...). The light is parented to the
    /// pickup, so it follows the item and is automatically destroyed with it; picking the item
    /// up removes the glow.
    /// </summary>
    internal static class ItemGlow
    {
        private static readonly Dictionary<ushort, LightSourceToy> ActiveLights = new Dictionary<ushort, LightSourceToy>();

        private static Config Cfg => ItemManagerPlugin.Cfg;

        public static void Start()
        {
            PlayerEvents.PickedUpItem += OnPickedUpItem;
            ServerEvents.PickupDestroyed += OnPickupDestroyed;
            ServerEvents.RoundRestarted += OnRoundRestarted;
        }

        public static void Stop()
        {
            PlayerEvents.PickedUpItem -= OnPickedUpItem;
            ServerEvents.PickupDestroyed -= OnPickupDestroyed;
            ServerEvents.RoundRestarted -= OnRoundRestarted;
            DestroyAll();
        }

        public static void Attach(Pickup pickup)
        {
            if (!Cfg.SpawnerGlowEnabled || pickup == null || pickup.IsDestroyed)
                return;

            LightSourceToy light = LightSourceToy.Create(new Vector3(0f, 0.15f, 0f), pickup.Transform);
            light.Type = LightType.Point;
            light.Color = ResolveColor(pickup.Type, pickup.Category);
            light.Range = Cfg.SpawnerGlowRange;
            light.Intensity = Cfg.SpawnerGlowIntensity;
            light.ShadowType = LightShadows.None;
            light.MovementSmoothing = 0;

            if (pickup.Serial != 0)
                ActiveLights[pickup.Serial] = light;
        }

        private static void OnPickedUpItem(PlayerPickedUpItemEventArgs ev)
        {
            if (ev.Item != null)
                Remove(ev.Item.Serial);
        }

        private static void OnPickupDestroyed(PickupDestroyedEventArgs ev)
        {
            if (ev.Pickup != null)
                Remove(ev.Pickup.Serial);
        }

        private static void OnRoundRestarted()
        {
            // The map (including all toys) is wiped on restart; just forget the references.
            ActiveLights.Clear();
        }

        private static void Remove(ushort serial)
        {
            if (serial == 0 || !ActiveLights.TryGetValue(serial, out LightSourceToy light))
                return;

            ActiveLights.Remove(serial);

            // Mirror already cascades destruction from the parent pickup; this is a safety net.
            if (light != null && !light.IsDestroyed)
                light.Destroy();
        }

        private static void DestroyAll()
        {
            foreach (LightSourceToy light in ActiveLights.Values)
            {
                if (light != null && !light.IsDestroyed)
                    light.Destroy();
            }

            ActiveLights.Clear();
        }

        private static Color ResolveColor(ItemType type, ItemCategory category)
        {
            if (Cfg.GlowColors != null
                && Cfg.GlowColors.TryGetValue(type, out string hex)
                && ColorUtility.TryParseHtmlString(hex, out Color configured))
                return configured;

            string fallback;
            switch (category)
            {
                case ItemCategory.Medical: fallback = "#2ECC40"; break;
                case ItemCategory.Firearm: fallback = "#E67E22"; break;
                case ItemCategory.Ammo: fallback = "#D5DBDB"; break;
                case ItemCategory.Keycard: fallback = "#5DADE2"; break;
                case ItemCategory.Grenade: fallback = "#E74C3C"; break;
                case ItemCategory.SCPItem: fallback = "#FF4136"; break;
                case ItemCategory.Armor: fallback = "#7F8C8D"; break;
                case ItemCategory.Radio: fallback = "#2E86C1"; break;
                default: fallback = Cfg.GlowFallbackColor; break;
            }

            return ColorUtility.TryParseHtmlString(fallback, out Color color) ? color : Color.white;
        }
    }
}
