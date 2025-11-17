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
                        sb.SetWidth(3000, MeasurementUnit.Pixels);
                        sb.SetAlignment(RueI.Utils.Enums.AlignStyle.Center);

                        // Add word wrapping before appending  
                        string wrappedText = WrapText(textHint.Text, maxLineLength: 80);
                        sb.Append("\n" + wrappedText + "\n");

                        string content = sb.ToString();
                        float duration = textHint.DurationScalar;

                        RueDisplay display = RueDisplay.Get(player);
                        display.Remove(new Tag("test"));

                        BasicElement be = new BasicElement(300, content)
                        {
                            ResolutionBasedAlign = true,
                            VerticalAlign = RueI.API.Elements.Enums.VerticalAlign.Up,
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

            private static string WrapText(string text, int maxLineLength)
            {
                if (string.IsNullOrEmpty(text) || text.Length <= maxLineLength)
                    return text;

                StringBuilder result = new StringBuilder();
                int currentLineLength = 0;

                foreach (string word in text.Split(' '))
                {
                    if (currentLineLength + word.Length + 1 > maxLineLength)
                    {
                        result.Append('\n');
                        currentLineLength = 0;
                    }
                    else if (currentLineLength > 0)
                    {
                        result.Append(' ');
                        currentLineLength++;
                    }

                    result.Append(word);
                    currentLineLength += word.Length;
                }

                return result.ToString();
            }
        }
    }
}
