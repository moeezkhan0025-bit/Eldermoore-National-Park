using System;
using UnityEngine;

// Plain serializable data for a single save slot.
// JsonUtility serializes public fields (and [SerializeField] private ones).
[Serializable]
public class SaveData
{
    // --- Metadata shown on the Continue / Load screens ---
    public string characterName;      // empty => treat slot as unnamed
    public int characterClassId;      // maps to your class enum / list index
    public string sceneName;          // which scene to resume into
    public float playtimeSeconds;     // for a "playtime" readout
    public long lastSavedUnixTime;    // used to find the "most recent" save

    // --- Example gameplay state: expand this with your own fields ---
    public int level;
    public Vector3 playerPosition;    // JsonUtility handles Vector3 natively
    // public int gold;
    // public string[] inventory;
    // ...add whatever your game needs...

    public bool IsEmpty => string.IsNullOrEmpty(characterName);
}
