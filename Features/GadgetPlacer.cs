using System.Collections.Generic;
using Exiled.API.Features;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using UnityEngine;
using MEC;

namespace GockelsAIO_exiled.Features
{
    public class GadgetPlacer
    {
        private static readonly Dictionary<Player, GameObject> activePreviews = new();
        private static readonly HashSet<Player> previewActive = new();
        private static readonly Dictionary<Player, List<GameObject>> placedGadgets = new();
        private static readonly Dictionary<Player, CoroutineHandle> previewCoroutines = new();

        public static void TogglePreviewOrPlace(Player player)
        {
            if (previewActive.Contains(player))
            {
                // Vorschau platzieren und "ADS" spawnen
                if (activePreviews.TryGetValue(player, out var previewObj))
                {
                    Timing.KillCoroutines(previewCoroutines[player]);
                    previewCoroutines.Remove(player);

                    // Position & Rotation merken
                    Vector3 placePos = previewObj.transform.position;
                    Quaternion placeRot = previewObj.transform.rotation;

                    // Vorschau entfernen
                    previewObj.transform.parent = null;
                    activePreviews.Remove(player);
                    previewActive.Remove(player);

                    // Vorschau-Objekt nicht behalten (optional: Destroy)
                    Object.Destroy(previewObj);

                    // "ADS"-Schematic dauerhaft spawnen an der gleichen Stelle
                    if (ObjectSpawner.TrySpawnSchematic("ADS", placePos, placeRot, out var adsSchematic))
                    {
                        if (!placedGadgets.ContainsKey(player))
                            placedGadgets[player] = new List<GameObject>();

                        placedGadgets[player].Add(adsSchematic.gameObject);
                        ADSManager.RegisterADS(adsSchematic.gameObject);
                    }
                }
            }
            else
            {
                // Alte Vorschau ggf. löschen
                if (activePreviews.TryGetValue(player, out var oldPreview))
                {
                    Timing.KillCoroutines(previewCoroutines[player]);
                    previewCoroutines.Remove(player);

                    Object.Destroy(oldPreview);
                    activePreviews.Remove(player);
                }

                Vector3 forwardOffset = player.CameraTransform.forward * 2f;
                Vector3 start = player.CameraTransform.position + forwardOffset;

                if (Physics.Raycast(start, Vector3.down, out RaycastHit hitInfo, 10f, LayerMask.GetMask("Default")))
                {
                    Vector3 groundPos = hitInfo.point;

                    if (ObjectSpawner.TrySpawnSchematic("PlacementShower", groundPos, player.Rotation, out var schematic))
                    {
                        schematic.transform.rotation = Quaternion.Euler(0f, player.Rotation.eulerAngles.y, 0f);

                        activePreviews[player] = schematic.gameObject;
                        previewActive.Add(player);

                        // Coroutine starten, die die Preview-Position updated
                        CoroutineHandle handle = Timing.RunCoroutine(UpdatePreviewPosition(player, schematic.gameObject));
                        previewCoroutines[player] = handle;
                    }
                }
            }
        }

        private static IEnumerator<float> UpdatePreviewPosition(Player player, GameObject preview)
        {
            // Y-Achse einmalig festhalten
            float fixedY = preview.transform.position.y;

            while (previewActive.Contains(player) && preview != null)
            {
                Vector3 forwardOffset = player.CameraTransform.forward * 2f;
                Vector3 targetPos = player.Position + forwardOffset;

                // Nur X und Z anpassen, Y bleibt fix
                preview.transform.position = new Vector3(targetPos.x, fixedY, targetPos.z);

                preview.transform.rotation = Quaternion.Euler(0f, player.Rotation.eulerAngles.y, 0f);

                yield return Timing.WaitForSeconds(0.05f);
            }
        }
    }
}
