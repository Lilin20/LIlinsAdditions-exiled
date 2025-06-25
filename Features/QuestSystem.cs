using Exiled.API.Features;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;

namespace GockelsAIO_exiled.Features
{
    public class Quest
    {
        public string Id { get; set; }
        public QuestType Type { get; set; }
        public string Target { get; set; }
        public int RequiredAmount { get; set; }
        public int RewardPoints { get; set; }
    }

    public class PlayerQuestProgress
    {
        public Quest Quest { get; set; }
        public int Progress { get; set; }
        public HashSet<uint> UniqueItemPickups { get; set; } = new();

        public bool IsCompleted => Progress >= Quest.RequiredAmount;

        public void AddProgress(int amount = 1)
        {
            Progress += amount;
        }
    }

    public class QuestManager
    {
        // Singleton-Instanz
        public static QuestManager Instance { get; } = new QuestManager();

        private readonly Dictionary<Player, List<PlayerQuestProgress>> _progress = new();

        private QuestManager() { }

        public void AssignQuest(Player player, Quest quest)
        {
            if (!_progress.ContainsKey(player))
                _progress[player] = new List<PlayerQuestProgress>();

            _progress[player].Add(new PlayerQuestProgress { Quest = quest });

            Log.Info($"Quest '{quest.Id}' an {player.Nickname} vergeben.");
        }

        public IEnumerable<PlayerQuestProgress> GetPlayerProgress(Player player)
        {
            return _progress.TryGetValue(player, out var list) ? list : Enumerable.Empty<PlayerQuestProgress>();
        }

        public void GrantReward(Player player, PlayerQuestProgress progress)
        {
            player.ShowHint($"Quest '{progress.Quest.Id}' completed! +{progress.Quest.RewardPoints} Points", 5);
            Log.Info($"Spieler {player.Nickname} hat Quest '{progress.Quest.Id}' abgeschlossen.");

            // Punkte hinzufügen (hier ggf. durch SessionVariable ersetzen)
            // Beispiel: player.SessionVariables["QuestPoints"] = currentPoints + progress.Quest.RewardPoints;
        }

        public void ClearProgress(Player player)
        {
            _progress.Remove(player);
        }

        public void ShowActiveQuests(Player player)
        {
            Log.Info($"Aktive Quests für {player.Nickname} werden angezeigt.");

            var hint = new Hint
            {
                AutoText = display => GetQuestContent(player),
                Alignment = HintServiceMeow.Core.Enum.HintAlignment.Left,
                FontSize = 28,
                XCoordinate = GetLeftXPosition(player.ReferenceHub.aspectRatioSync.AspectRatio),
                YCoordinate = 100,
                SyncSpeed = HintServiceMeow.Core.Enum.HintSyncSpeed.Fastest
            };

            PlayerDisplay.Get(player).AddHint(hint);
        }

        public static string GetQuestContent(Player player)
        {
            var quests = QuestManager.Instance.GetPlayerProgress(player).ToList();
            if (quests.Count == 0)
                return "<color=grey>Keine aktiven Quests</color>";

            var sb = new StringBuilder();
            sb.AppendLine("<color=#ffaa00><b>Active Quests</b></color>");

            foreach (var q in quests)
            {
                var status = q.IsCompleted ? "<color=green>✔</color>" : "<color=red>✘</color>";
                sb.AppendLine($"{status} <b>{q.Quest.Id}</b>: {q.Progress}/{q.Quest.RequiredAmount}");
            }

            return sb.ToString();
        }

        public static float GetLeftXPosition(float aspectRatio)
        {
            return (622.27444f * Mathf.Pow(aspectRatio, 3f)) +
                   (-2869.08991f * Mathf.Pow(aspectRatio, 2f)) +
                   (3827.03102f * aspectRatio) - 1580.21554f;
        }
    }

    public enum QuestType
    {
        UseScpItem,
        UseMedicine,
        KillEnemy,
        PickupUniqueItem
    }
}
