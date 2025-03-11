using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  [Header("Target Settings")]
  public GameObject targetPrefab;
  public Vector2 playBounds = new Vector2(2000f, 2000f); // Default play area size
  public Vector2 origin = Vector2.zero;

  [Header("Spawn Settings")]
  public float spawnInterval = 1f; // Spawns a target every second
  private float nextSpawnTime = 0f;

  void Update()
  {
    if (Time.time >= nextSpawnTime)
    {
      SpawnTarget();
      nextSpawnTime = Time.time + spawnInterval;
    }
  }

  private void SpawnTarget()
  {
    if (targetPrefab == null)
    {
      Debug.LogWarning("Target prefab is not assigned in the GameManager.");
      return;
    }

    // Generate a random position within the play bounds relative to the origin
    float randomX = Random.Range(origin.x - playBounds.x / 2, origin.x + playBounds.x / 2);
    float randomY = Random.Range(origin.y - playBounds.y / 2, origin.y + playBounds.y / 2);
    Vector3 spawnPosition = new Vector3(randomX, randomY, -1);

    Instantiate(targetPrefab, spawnPosition, Quaternion.identity);
  }

}
