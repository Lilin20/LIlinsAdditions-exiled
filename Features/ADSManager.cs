using System.Collections.Generic;
using AdminToys;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.API.Features.Toys;
using MEC;
using UnityEngine;

namespace GockelsAIO_exiled.Features
{
    public static class ADSManager
    {
        private static readonly HashSet<GameObject> activeADS = new();

        private const float detectionRadius = 2f;

        public static void RegisterADS(GameObject ads)
        {
            if (ads != null)
                activeADS.Add(ads);
        }

        public static void UnregisterADS(GameObject ads)
        {
            if (ads != null)
                activeADS.Remove(ads);
        }

        // Regelmäßig aufrufen (z.B. mit MEC oder einem Event)
        public static void CheckForGrenades()
        {
            foreach (var ads in activeADS)
            {
                if (ads == null)
                    continue;

                Vector3 origin = ads.transform.position + Vector3.up * 0.5f;
                RaycastHit[] hits = Physics.SphereCastAll(origin, detectionRadius, Vector3.up, 2f, (1 << 0) | (1 << 13) | (1 << 20));

                foreach (RaycastHit hit in hits)
                {
                    if (Player.TryGet(hit.collider, out Player playerHit))
                    {
                        Log.Info(playerHit);
                        Log.Info(playerHit.GameObject.layer);
                        break;
                    }

                    var pickup = Pickup.Get(hit.collider.gameObject);
                    if (pickup is TimeGrenadeProjectile grenade)
                    {
                        Log.Info("grenade gefunden");

                        SpawnLaserBetween(ads.transform.position, grenade.GameObject.transform.position);

                        // Granate zerstören
                        grenade.Destroy();

                        break; // keine weiteren Treffer für dieses ads nötig
                    }
                }
            }
        }

        private static void SpawnLaserBetween(Vector3 start, Vector3 end)
        {
            Vector3 direction = end - start;
            float distance = direction.magnitude;
            Vector3 midPoint = start + direction / 2f;

            // Basisrotation: zylinder zeigt nach oben (Y-Achse)
            // Wir wollen ihn aber entlang 'direction' ausrichten,
            // also erstellen wir LookRotation für Z-Achse und rotieren dann um 90 Grad um X
            Quaternion rotation = Quaternion.LookRotation(direction);
            rotation *= Quaternion.Euler(90f, 0f, 0f); // Drehung anpassen

            Primitive laser = Primitive.Create(PrimitiveType.Cylinder);
            laser.Flags = PrimitiveFlags.Visible;
            laser.Color = new Color(25f, 25f, 25f, 0.9f);
            laser.Position = midPoint;
            laser.Rotation = rotation;
            laser.Scale = new Vector3(0.025f, distance / 2f, 0.025f);

            Timing.RunCoroutine(DestroyAfterDelay(laser, 0.2f));
        }

        private static IEnumerator<float> DestroyAfterDelay(Primitive primitive, float delay)
        {
            yield return Timing.WaitForSeconds(delay);
            primitive.Destroy();
        }
    }

    public static class ADSMonitor
    {
        private static CoroutineHandle _adsCheck;

        public static void Start()
        {
            _adsCheck = MEC.Timing.RunCoroutine(CheckRoutine());
        }

        public static void Stop()
        {
            MEC.Timing.KillCoroutines(_adsCheck);
        }

        private static IEnumerator<float> CheckRoutine()
        {
            while (true)
            {
                ADSManager.CheckForGrenades();
                yield return MEC.Timing.WaitForSeconds(0.25f);
            }
        }
    }
}
