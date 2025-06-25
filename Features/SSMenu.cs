using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using GockelsAIO_exiled.Roles.NTF;
using UserSettings.ServerSpecific;
using Yassa.Features;
using Yassa.Features.Builders;
using Yassa.Interfaces;
using Yassa.Models;

namespace GockelsAIO_exiled.Features
{
    public class SSMenu
    {
        public void InitSSMenu()
        {
            OptionNode node = new OptionNodeBuilder()
                .SetHeader("Gockel's Banger Plugin")
                .SetHint("Hier findest du alle Einstellungen zu meinem Plugin :3")
                .SetPadding(true)
                .AddTextAreaOption(o => o 
                    .SetCustomId("CustomRoleTextArea")
                    .SetLabel("Custom Role - Einstellungen")
                    .SetFoldoutMode(SSTextArea.FoldoutMode.NotCollapsable))
                .AddKeybindOption(o => o
                    .SetCustomId("RiotShieldAbilityButton")
                    .SetLabel("Riot Shield - Ability")
                    .SetHint("Keybind um das Riot Shield zu benutzen.")
                    .SetKeyCode(UnityEngine.KeyCode.B)
                    .SetIsInteractionPreventedOnGui(true)
                    .OnPressed(OnKeybindRiotShield))
                .Build();

            OptionsService.Register(node);
        }

        private void OnKeybindRiotShield(Player player, IOption clickable)
        {
            if (player.GetCustomRoles().Contains(CustomRole.Get(100)))
            {
                ActiveAbility? selected = player.GetSelectedAbility();
                selected.UseAbility(player);
            }
        }
    }
}
