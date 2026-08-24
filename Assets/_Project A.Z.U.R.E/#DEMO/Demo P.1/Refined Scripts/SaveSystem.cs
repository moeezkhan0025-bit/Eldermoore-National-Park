using System;
using System.IO;
using UnityEngine;

// Reads and writes save files. Three slots, JSON, in Application.persistentDataPath.
// Static class — never attached to a GameObject.
public static class SaveSystem
{
    public const int SlotCount = 3;

    static string PathFor(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

    public static bool SlotExists(int slot) => File.Exists(PathFor(slot));

    public static bool HasAnySave()
    {
        for (int i = 0; i < SlotCount; i++)
            if (SlotExists(i)) return true;
        return false;
    }

    public static int GetFirstEmptySlot()
    {
        for (int i = 0; i < SlotCount; i++)
            if (!SlotExists(i)) return i;
        return -1;
    }

    // Most recently saved slot, or -1 if there are no saves. Used by Continue().
    public static int GetMostRecentSlot()
    {
        int best = -1;
        long bestTime = long.MinValue;
        for (int i = 0; i < SlotCount; i++)
        {
            SaveData d = Load(i);
            if (d != null && d.lastSavedUnixTime > bestTime)
            {
                bestTime = d.lastSavedUnixTime;
                best = i;
            }
        }
        return best;
    }

    // Two-arg signature to match GameManager.SaveCurrent(): SaveSystem.Save(slot, data).
    public static void Save(int slot, SaveData data)
    {
        if (data == null) return;
        data.lastSavedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // stamp the time
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(PathFor(slot), json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to save slot {slot}: {e.Message}");
        }
    }

    public static SaveData Load(int slot)
    {
        if (!SlotExists(slot)) return null;
        try
        {
            string json = File.ReadAllText(PathFor(slot));
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to load slot {slot}: {e.Message}");
            return null;
        }
    }

    public static void Delete(int slot)
    {
        try { if (SlotExists(slot)) File.Delete(PathFor(slot)); }
        catch (Exception e) { Debug.LogError($"[SaveSystem] Failed to delete slot {slot}: {e.Message}"); }
    }
}