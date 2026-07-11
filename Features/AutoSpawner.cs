using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;
using Random = UnityEngine.Random;

namespace ItemManager.Features
{
    /// <summary>
    /// At round start, automatically spawns glowing items scattered on the floor of random
    /// rooms, driven by the auto_spawn_rules config (zone, optional room filter, item pool,
    /// count). Points are validated with raycasts so items always land on the ground inside
    /// the chosen room.
    /// </summary>
    internal sealed class AutoSpawner
    {
        private static Config Cfg => ItemManagerPlugin.Cfg;

        private CoroutineHandle _routine;

        public void Start()
        {
            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundRestarted += OnRoundRestarted;
        }

        public void Stop()
        {
            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundRestarted -= OnRoundRestarted;
            Timing.KillCoroutines(_routine);
        }

        private void OnRoundStarted()
        {
            if (!Cfg.AutoSpawnerEnabled || Cfg.AutoSpawnRules == null || Cfg.AutoSpawnRules.Count == 0)
                return;

            Timing.KillCoroutines(_routine);
            _routine = Timing.RunCoroutine(SpawnRoutine());
        }

        private void OnRoundRestarted() => Timing.KillCoroutines(_routine);

        private static IEnumerator<float> SpawnRoutine()
        {
            yield return Timing.WaitForSeconds(Mathf.Max(0.5f, Cfg.AutoSpawnDelaySeconds));

            int total = 0;
            foreach (AutoSpawnRule rule in Cfg.AutoSpawnRules)
            {
                try
                {
                    total += SpawnRule(rule);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Auto spawn rule ({rule.Zone}) failed: {ex}");
                }

                yield return Timing.WaitForOneFrame;
            }

            if (total > 0)
                Logger.Info($"Auto spawner placed {total} glowing item(s) around the facility.");
        }

        private static int SpawnRule(AutoSpawnRule rule)
        {
            if (rule.Items == null || rule.Items.Count == 0 || rule.Count < 1)
                return 0;

            List<Room> candidates = Room.List
                .Where(r => r != null && !r.IsDestroyed && r.Zone == rule.Zone
                            && (rule.Rooms == null || rule.Rooms.Count == 0 || rule.Rooms.Contains(r.Name)))
                .ToList();

            if (candidates.Count == 0)
            {
                Logger.Warn($"Auto spawner found no rooms for zone {rule.Zone}.");
                return 0;
            }

            int spawned = 0;
            for (int i = 0; i < rule.Count; i++)
            {
                Room room = candidates[Random.Range(0, candidates.Count)];
                if (!TryGetFloorPoint(room, out Vector3 point))
                    continue;

                ItemType type = rule.Items[Random.Range(0, rule.Items.Count)];
                Pickup pickup = Pickup.Create(type, point, Random.rotation);
                if (pickup == null)
                    continue;

                if (rule.Glow)
                    ItemGlow.Attach(pickup);

                spawned++;
            }

            return spawned;
        }

        /// <summary>Finds a random spot on the floor of the room, retrying until the point is
        /// really inside that room and has ground under it.</summary>
        private static bool TryGetFloorPoint(Room room, out Vector3 point)
        {
            for (int attempt = 0; attempt < 8; attempt++)
            {
                Vector2 offset = Random.insideUnitCircle * 5.5f;
                Vector3 probe = room.Position + new Vector3(offset.x, 1.5f, offset.y);

                if (Room.GetRoomAtPosition(probe) != room)
                    continue;

                if (!Physics.Raycast(probe, Vector3.down, out RaycastHit hit, 5f, -1, QueryTriggerInteraction.Ignore))
                    continue;

                point = hit.point + Vector3.up * 0.35f;
                return true;
            }

            point = default;
            return false;
        }
    }
}
