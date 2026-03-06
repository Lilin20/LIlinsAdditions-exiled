using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using UnityEngine;

namespace LilinsAdditions.Items.Keycards
{
    [CustomItem(ItemType.KeycardCustomSite02)]
    public class CreditCard : CustomKeycard
    {
        private const string CARD_LABEL = "Credit Card";
        private const string PLACEHOLDER_TEXT = "---";

        public override uint Id { get; set; } = 5555;
        public override string Name { get; set; } = CARD_LABEL;
        public override string Description { get; set; } = PLACEHOLDER_TEXT;
        public override float Weight { get; set; } = 1f;
        public override SpawnProperties SpawnProperties { get; set; }
        
        public override string KeycardLabel { get; set; } = CARD_LABEL;
        public override Color32? KeycardLabelColor { get; set; } = Color.black;
        public override string KeycardName { get; set; } = PLACEHOLDER_TEXT;
        public override Color32? TintColor { get; set; } = Color.white;
        public override Color32? KeycardPermissionsColor { get; set; } = Color.white;
    }
}
