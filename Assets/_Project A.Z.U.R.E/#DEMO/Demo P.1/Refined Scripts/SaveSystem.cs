using System;
using System.IO;
using UnityEngine;

// Handles reading/writing the three save slots as JSON files on disk.
// Files live in Application.persistentDataPath (safe, writable on all platforms).
public static class SaveSystem
{
    public const int SlotCount = 3;

    private static string SlotPath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

    public static bool SlotExists(int slot) => File.Exists(SlotPath(slot));

    public static void Save(int slot, SaveData data)
    {
        data.lastSavedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SlotPath(slot), json);
    }

    public static SaveData Load(int slot)
    {
        if (!SlotExists(slot)) return null;
        string json = File.ReadAllText(SlotPath(slot));
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void Delete(int slot)
    {
        if (SlotExists(slot)) File.Delete(SlotPath(slot));
    }

    // Metadata for all three slots. A null entry = empty slot.
    // Use this to populate the Load Game screen.
    public static SaveData[] LoadAllSlots()
    {
        var slots = new SaveData[SlotCount];
        for (int i = 0; i < SlotCount; i++)
            slots[i] = Load(i);
        return slots;
    }

    // For "Continue": the most recently saved non-empty slot, or -1 if none.
    public static int GetMostRecentSlot()
    {
        int best = -1;
        long bestTime = long.MinValue;
        for (int i = 0; i < SlotCount; i++)
        {
            var data = Load(i);
            if (data != null && data.lastSavedUnixTime > bestTime)
            {
                bestTime = data.lastSavedUnixTime;
                best = i;
            }
        }
        return best;
    }

    // For "New Game": first empty slot, or -1 if all three are full.
    public static int GetFirstEmptySlot()
    {
        for (int i = 0; i < SlotCount; i++)
            if (!SlotExists(i)) return i;
        return -1;
    }

    public static bool HasAnySave()
    {
        for (int i = 0; i < SlotCount; i++)
            if (SlotExists(i)) return true;
        return false;
    }
}
