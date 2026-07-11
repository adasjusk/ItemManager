using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace ItemManager.Features
{
    /// <summary>
    /// Once the round is old enough (LCZ is decontaminated anyway), periodically deletes
    /// every leftover item pickup and dead body inside the Light Containment Zone so the
    /// server does not keep simulating junk nobody can reach.
    /// </summary>
    internal sealed class LczCleaner
    {
        private static Config Cfg => ItemManagerPlugin.Cfg;

        private CoroutineHandle _routine;

        public void Start()
        {
            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundRestarted += OnRoundRestarted;

            // Covers plugin reloads that happen mid-round.
            if (Round.IsRoundInProgress)
                OnRoundStarted();
        }

        public void Stop()
        {
            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundRestarted -= OnRoundRestarted;
            Timing.KillCoroutines(_routine);
        }

        private void OnRoundStarted()
        {
            if (!Cfg.CleanerEnabled)
                return;

            Timing.KillCoroutines(_routine);
            _routine = Timing.RunCoroutine(CleanupRoutine());
        }

        private void OnRoundRestarted() => Timing.KillCoroutines(_routine);

        private IEnumerator<float> CleanupRoutine()
        {
            float startDelay = Cfg.CleanerStartMinutes * 60f - (float)Round.Duration.TotalSeconds;
            if (startDelay > 0f)
                yield return Timing.WaitForSeconds(startDelay);

            Logger.Info($"LCZ cleanup is now active (round older than {Cfg.CleanerStartMinutes} minutes).");

            while (Round.IsRoundInProgress)
            {
                try
                {
                    Sweep();
                }
                catch (Exception ex)
                {
                    Logger.Error($"LCZ cleanup sweep failed: {ex}");
                }

                yield return Timing.WaitForSeconds(Mathf.Max(5f, Cfg.CleanerIntervalSeconds));
            }
        }

        private static void Sweep()
        {
            int items = 0;
            int corpses = 0;

            foreach (Pickup pickup in Pickup.List.ToList())
            {
                if (pickup == null || pickup.IsDestroyed || pickup.IsInUse)
                    continue;

                Room room = pickup.Room;
                if (room == null || room.Zone != FacilityZone.LightContainment)
                    continue;

                if (!Cfg.CleanerAllLczItems && !Cfg.CleanerLowTierItems.Contains(pickup.Type))
                    continue;

                pickup.Destroy();
                items++;
            }

            if (Cfg.CleanerRemoveRagdolls)
            {
                foreach (Ragdoll ragdoll in Ragdoll.List.ToList())
                {
                    if (ragdoll == null || ragdoll.IsDestroyed)
                        continue;

                    Room room = Room.GetRoomAtPosition(ragdoll.Position);
                    if (room == null || room.Zone != FacilityZone.LightContainment)
                        continue;

                    ragdoll.Destroy();
                    corpses++;
                }
            }

            if (items > 0 || corpses > 0)
                Logger.Info($"LCZ cleanup: removed {items} item(s) and {corpses} corpse(s).");
        }
    }
}
