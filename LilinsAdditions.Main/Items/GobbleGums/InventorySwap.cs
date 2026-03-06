using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using UnityEngine;

namespace LilinsAdditions.Items.GobbleGums;

[CustomItem(ItemType.AntiSCP207)]
public class InventorySwap : FortunaFizzItem
{
    public InventorySwap()
    {
        Buyable = true;
    }

    public override uint Id { get; set; } = 817;
    public override string Name { get; set; } = "Backpack Bandito";
    public override string Description { get; set; } = "Swap your entire inventory with a random player.";
    public override float Weight { get; set; } = 0.5f;
    public string SwapMessageFormat { get; set; } = "Inventory swapped with {0}!";
    public override SpawnProperties SpawnProperties { get; set; }

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
        base.UnsubscribeEvents();
    }

    private void OnUsingItem(UsingItemEventArgs ev)
    {
        if (!Check(ev.Player.CurrentItem))
            return;

        ev.IsAllowed = false;

        ExecuteInventorySwap(ev);
    }

    private void ExecuteInventorySwap(UsingItemEventArgs ev)
    {
        if (ev.Player == null || !ev.Player.IsAlive || ev.Player.CurrentItem != ev.Item)
            return;

        var target = GetRandomValidTarget(ev.Player);
        if (target == null)
        {
            Log.Warn($"[InventorySwapper] Could not find valid target for {ev.Player.Nickname}");
            return;
        }

        ev.Item?.Destroy();

        SwapInventories(ev.Player, target);
        Log.Debug($"[InventorySwapper] {ev.Player.Nickname} swapped inventory with {target.Nickname}");
    }

    private Player GetRandomValidTarget(Player user)
    {
        var validTargets = Player.List.Where(p =>
            p.IsAlive &&
            p != user &&
            !p.IsScp &&
            p.Items.Any()).ToList();

        if (validTargets.Count == 0)
            return null;

        return validTargets[Random.Range(0, validTargets.Count)];
    }

    private void SwapInventories(Player player1, Player player2)
    {
        var player1Items = player1.Items.ToList();
        var player2Items = player2.Items.ToList();

        var player1CustomItems = new Dictionary<Item, CustomItem>();
        var player2CustomItems = new Dictionary<Item, CustomItem>();

        foreach (var item in player1Items)
            if (TryGet(item, out var customItem))
                player1CustomItems[item] = customItem;

        foreach (var item in player2Items)
            if (TryGet(item, out var customItem))
                player2CustomItems[item] = customItem;

        player1.ClearItems();
        player2.ClearItems();

        foreach (var kvp in player1CustomItems) kvp.Value.TrackedSerials.Remove(kvp.Key.Serial);

        foreach (var kvp in player2CustomItems) kvp.Value.TrackedSerials.Remove(kvp.Key.Serial);

        foreach (var item in player2Items)
        {
            var newItem = Item.Create(item.Type);

            if (item is Firearm originalFirearm && newItem is Firearm newFirearm)
                newFirearm.MagazineAmmo = originalFirearm.MagazineAmmo;

            player1.AddItem(newItem);

            if (player2CustomItems.TryGetValue(item, out var customItem)) customItem.TrackedSerials.Add(newItem.Serial);
        }

        foreach (var item in player1Items)
        {
            var newItem = Item.Create(item.Type);

            if (item is Firearm originalFirearm && newItem is Firearm newFirearm)
                newFirearm.MagazineAmmo = originalFirearm.MagazineAmmo;

            player2.AddItem(newItem);

            if (player1CustomItems.TryGetValue(item, out var customItem)) customItem.TrackedSerials.Add(newItem.Serial);
        }

        var message1 = string.Format(SwapMessageFormat, player2.Nickname);
        var message2 = string.Format(SwapMessageFormat, player1.Nickname);

        player1.ShowHint(message1, 5f);
        player2.ShowHint(message2, 5f);
    }
}