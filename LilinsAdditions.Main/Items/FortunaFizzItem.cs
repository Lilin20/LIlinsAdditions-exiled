using System.ComponentModel;
using Exiled.CustomItems.API.Features;

namespace LilinsAdditions.Main.Items;

public abstract class FortunaFizzItem : CustomItem
{
    [Description("Whether this drink can be selected randomly")]
    public bool Buyable { get; set; } = true;
}