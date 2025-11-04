using Exiled.API.Features;
using Exiled.API.Features.Pools;
using HarmonyLib;
using Hints;
using InventorySystem.Items.Pickups;
using JetBrains.Annotations;
using RueI.API;
using RueI.API.Elements;
using RueI.Utils;
using RueI.Utils.Enums;
using System.Text;

namespace GockelsAIO_exiled.Patches
{
    public class PickupPatches
    {
        [HarmonyPatch(typeof(PickupStandardPhysics), nameof(PickupStandardPhysics.ServerSendFreeze), MethodType.Getter)]
        internal static class PickupIsKinematicSyncPatch
        {
            [UsedImplicitly]
            static void Postfix(PickupStandardPhysics __instance, ref bool __result)
            {
                __result = __result || __instance.Rb.isKinematic;
            }
        }

        [HarmonyPatch(typeof(HintDisplay), nameof(HintDisplay.Show))]
        [HarmonyPriority(Priority.First)]
        internal static class CustomHintFrameworkPatch
        {
            private static bool Prefix(HintDisplay __instance, Hints.Hint hint)
            {
                if (__instance?.gameObject == null)
                    return true;

                Player player = Player.Get(__instance.gameObject);
                if (player == null)
                    return true;

                if (hint is TextHint textHint)
                {
                    StringBuilder sb = StringBuilderPool.Pool.Get();

                    try
                    {
                        sb.SetLineHeight(20, MeasurementUnit.Pixels);
                        sb.SetAlignment(RueI.Utils.Enums.AlignStyle.Center);
                        sb.Append("\n" + textHint.Text + "\n");

                        string content = sb.ToString();
                        float duration = textHint.DurationScalar;

                        RueDisplay display = RueDisplay.Get(player);

                        display.Remove(new Tag("test"));

                        BasicElement be = new BasicElement(300, content)
                        {
                            ResolutionBasedAlign = true,
                            VerticalAlign = RueI.API.Elements.Enums.VerticalAlign.Down,
                        };

                        display.Show(new Tag("test"), be, duration);
                    }
                    finally
                    {
                        StringBuilderPool.Pool.Return(sb);
                    }

                    return false;
                }

                return true;
            }
        }
    }
}
