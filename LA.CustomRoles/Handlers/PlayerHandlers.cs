using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.CustomRoles.API;
using Exiled.Events.EventArgs.Player;
using LilinsAdditions.CustomRoles.Abilities.Active;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LilinsAdditions.CustomRoles.Handlers
{
    public class PlayerHandlers
    {
        public void OnChangingItem(ChangingItemEventArgs ev)
        {
            if (ev.Player.GetCustomRoles().All(r => r.Name != "MTF Nu-7")) return;

            if (!RiotShield.activeShields.TryGetValue(ev.Player, out var shield)) return;

            Transform shieldTransform = shield.transform;

            if (ev.Item is Firearm)
            {
                // Spieler hat Waffe -> Schild zur Seite drehen
                shieldTransform.localPosition = new Vector3(-0.4f, -1f, 0.4f); // leicht links
                shieldTransform.localRotation = Quaternion.Euler(0f, -70f, 0f); // seitlich
                Log.Debug($"[{ev.Player.Nickname}] weapon detected – shield rotated to the left.");
            }
            else
            {
                // Standardausrichtung
                shieldTransform.localPosition = new Vector3(0f, -1f, 0.5f);
                shieldTransform.localRotation = Quaternion.identity;
                Log.Debug($"[{ev.Player.Nickname}] no weapon detected. – shield centered.");
            }
        }
    }
}
