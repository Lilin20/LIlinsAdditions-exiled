using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GockelsAIO_exiled.Items.Keycards
{
    [CustomItem(ItemType.KeycardCustomSite02)]
    public class CreditCard : CustomKeycard
    {
        public override uint Id { get; set; } = 5555;
        public override string Name { get; set; } = "Credit Card";
        public override string Description { get; set; } = "---";
        public override float Weight { get; set; } = 1f;
        public override SpawnProperties SpawnProperties { get; set; }
        public override string KeycardLabel { get; set; } = "Credit Card";
        public override Color32? KeycardLabelColor { get; set; } = Color.black;
        public override string KeycardName { get; set; } = "---";
        public override Color32? TintColor { get; set; } = Color.white;
        public override Color32? KeycardPermissionsColor { get; set; } = Color.white;
    }
}
