using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{

  [System.Serializable]
  public class BossEntry
  {
    public string bossName;         // The text that appears
    public KeyCode spawnKey;        // Press this key to spawn the boss
    public GameObject bossPrefab;   // The boss prefab to spawn
    public Vector2 spawnPosition;   // Position in world space to spawn
  }

  [Header("Boss Spawn List")]
  public List<BossEntry> bossList = new List<BossEntry>();

  void Update()
  {
    // Loop through the list and check if any spawn key was pressed
    foreach (var boss in bossList)
    {
      if (Input.GetKeyDown(boss.spawnKey))
      {
        SpawnBoss(boss);
      }
    }
  }

  void SpawnBoss(BossEntry boss)
  {
    if (boss.bossPrefab == null)
    {
      Debug.LogWarning($"Boss prefab for '{boss.bossName}' is missing!");
      return;
    }

    Instantiate(boss.bossPrefab, new Vector3(boss.spawnPosition.x, boss.spawnPosition.y, -1), Quaternion.identity);
    BillboardService.Instance.ShowText(boss.bossName);
    Debug.Log($"Spawned boss: {boss.bossName} at {boss.spawnPosition}");
  }

}
