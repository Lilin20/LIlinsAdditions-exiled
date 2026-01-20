using System.Reflection;
using System.Text;
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

namespace LilinsAdditions.Patches
{
    /// <summary>
    /// Harmony patches for pickup and hint system customizations.
    /// </summary>
    public static class PickupPatches
    {
        private const int HINT_WIDTH_PIXELS = 3000;
        private const float HINT_POSITION = 300f;
        private const int MAX_LINE_LENGTH = 80;
        private const string HINT_TAG = "test";

        #region Pickup Physics Patch

        /// <summary>
        /// Patches pickup freeze synchronization to respect kinematic state.
        /// </summary>
        [HarmonyPatch(typeof(PickupStandardPhysics), nameof(PickupStandardPhysics.ServerSendFreeze), MethodType.Getter)]
        internal static class PickupIsKinematicSyncPatch
        {
            [UsedImplicitly]
            private static void Postfix(PickupStandardPhysics __instance, ref bool __result)
            {
                __result = __result || __instance.Rb.isKinematic;
            }
        }

        #endregion

        #region Custom Hint Framework Patch

        /// <summary>
        /// Intercepts and customizes hint display using RueI framework.
        /// </summary>
        [HarmonyPatch(typeof(HintDisplay), nameof(HintDisplay.Show))]
        [HarmonyPriority(Priority.First)]
        internal static class CustomHintFrameworkPatch
        {
            [UsedImplicitly]
            private static bool Prefix(HintDisplay __instance, Hints.Hint hint)
            {
                if (!TryGetPlayerFromHintDisplay(__instance, out var player))
                    return true;

                if (hint is not TextHint textHint)
                    return true;

                DisplayCustomHint(player, textHint);
                return false;
            }

            private static bool TryGetPlayerFromHintDisplay(HintDisplay hintDisplay, out Player player)
            {
                player = null;

                if (hintDisplay?.gameObject == null)
                    return false;

                player = Player.Get(hintDisplay.gameObject);
                return player != null;
            }

            private static void DisplayCustomHint(Player player, TextHint textHint)
            {
                var sb = StringBuilderPool.Pool.Get();
                try
                {
                    var content = BuildHintContent(sb, GetTextHintContent(textHint));
                    ShowHintToPlayer(player, content, textHint.DurationScalar);
                }
                finally
                {
                    StringBuilderPool.Pool.Return(sb);
                }
            }

            private static string GetTextHintContent(TextHint textHint)
            {
                var textProperty = typeof(TextHint).GetProperty("Text", 
                    BindingFlags.Instance |
                    BindingFlags.Public | 
                    BindingFlags.NonPublic);

                return textProperty?.GetValue(textHint) as string ?? string.Empty;
            }

            private static string BuildHintContent(StringBuilder sb, string text)
            {
                sb.SetWidth(HINT_WIDTH_PIXELS, MeasurementUnit.Pixels);
                sb.SetAlignment(AlignStyle.Center);

                var wrappedText = WrapText(text, MAX_LINE_LENGTH);
                sb.Append($"\n{wrappedText}\n");

                return sb.ToString();
            }

            private static void ShowHintToPlayer(Player player, string content, float duration)
            {
                var display = RueDisplay.Get(player);
                display.Remove(new Tag(HINT_TAG));

                var element = new BasicElement(HINT_POSITION, content)
                {
                    ResolutionBasedAlign = true,
                    VerticalAlign = RueI.API.Elements.Enums.VerticalAlign.Up,
                };

                display.Show(new Tag(HINT_TAG), element, duration);
            }

            private static string WrapText(string text, int maxLineLength)
            {
                if (string.IsNullOrEmpty(text) || text.Length <= maxLineLength)
                    return text;

                var result = new StringBuilder(text.Length + (text.Length / maxLineLength) * 2);
                var words = text.Split(' ');
                var currentLineLength = 0;

                foreach (var word in words)
                {
                    var wordLength = word.Length;
                    
                    if (currentLineLength > 0 && currentLineLength + wordLength + 1 > maxLineLength)
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
                    currentLineLength += wordLength;
                }

                return result.ToString();
            }
        }

        #endregion
    }
}
