using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;
using Random = UnityEngine.Random;

namespace ItemManager.Features
{
    internal sealed class DeathItemMover
    {
        private static Config Cfg => ItemManagerPlugin.Cfg;
        public void Start() => PlayerEvents.Death += OnDeath;
        public void Stop() => PlayerEvents.Death -= OnDeath;
        private static void OnDeath(PlayerDeathEventArgs ev)
        {
            if (!Cfg.MoverEnabled || ev.Player == null)
                return;

            ReferenceHub owner = ev.Player.ReferenceHub;
            Vector3 deathPosition = ev.OldPosition;
            Timing.CallDelayed(Mathf.Max(0.1f, Cfg.MoverDelaySeconds), () => MoveDrops(owner, deathPosition));
        }

        private static void MoveDrops(ReferenceHub owner, Vector3 deathPosition)
        {
            try
            {
                float collectSqr = Cfg.MoverCollectRadius * Cfg.MoverCollectRadius;
                List<Pickup> drops = Pickup.List
                    .Where(p => p != null && !p.IsDestroyed && p.IsSpawned
                                && p.LastOwner?.ReferenceHub == owner
                                && (p.Position - deathPosition).sqrMagnitude <= collectSqr)
                    .ToList();

                if (drops.Count == 0)
                    return;
                Vector3 anchor = new Vector3(Cfg.MoverTargetX, Cfg.MoverTargetY, Cfg.MoverTargetZ);
                foreach (Pickup pickup in drops)
                    ScatterTeleport(pickup, anchor);

                Logger.Debug($"Moved {drops.Count} death drop(s) to the surface drop zone.", Cfg.Debug);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to move death drops: {ex}");
            }
        }

        private static void ScatterTeleport(Pickup pickup, Vector3 anchor)
        {
            Vector2 offset = Random.insideUnitCircle * Cfg.MoverScatterRadius;
            Vector3 column = anchor + new Vector3(offset.x, 0f, offset.y);
            float groundY = FindGroundY(column, anchor.y);
            pickup.Position = new Vector3(
                column.x,
                groundY + Cfg.MoverDropHeight + Random.Range(0f, 0.5f),
                column.z);
            pickup.Rotation = Random.rotation;

            Rigidbody rb = pickup.Rigidbody;
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Random.insideUnitSphere * 2f;
                rb.WakeUp();
            }
        }

        private static float FindGroundY(Vector3 column, float fallbackY)
        {
            Vector3 origin = new Vector3(column.x, column.y + 2.5f, column.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 30f, -1, QueryTriggerInteraction.Ignore))
                return hit.point.y;
            return fallbackY - 0.9f;
}   }   }