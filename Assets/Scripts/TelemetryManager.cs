using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TelemetryManager : MonoBehaviour
{

  private Dictionary<string, int> valuePairs = new Dictionary<string, int>();

  // 1) Singleton reference
  private static TelemetryManager instance;
  public static TelemetryManager Instance => instance;

  // 2) For periodic logging of ship stats (once per second).
  private float nextLogTime = 0f;
  private float logInterval = 1f;

  // 3) Track old accelerations so we can compute "jerk" (the change in acceleration).
  private float oldLinAccel = 0f;
  private float oldRotAccel = 0f;

  // 4) Track boss timing.
  private float bossStartTime = 0f;

  private void Awake()
  {
    // Set up a singleton so we can easily access TelemetryManager.Instance
    if (instance != null && instance != this)
    {
      Destroy(this.gameObject);
      return;
    }
    instance = this;
    DontDestroyOnLoad(this.gameObject);
  }

  private void Update()
  {
    if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
    {
      Save();
      Clear();
    }

    if (PlayerController.aiControlling && Time.time >= nextLogTime)
    {
      LogPeriodicData();
      nextLogTime = Time.time + logInterval;
    }
  }

  /// <summary>
  /// Logs velocity, acceleration, jerk, rotation speed, rotation acceleration, rotation jerk,
  /// and ship size. Called once per second while AI is controlling.
  /// </summary>
  private void LogPeriodicData()
  {
    // Find the PlayerController in the scene
    PlayerController player = FindObjectOfType<PlayerController>();
    if (player == null) return;

    // 1) Linear velocity
    float velocity = player.GetComponent<Rigidbody2D>().velocity.magnitude;
    AddFloat("ShipVelocity", velocity);

    // 2) Linear acceleration
    float currentLinAccel = player.GetCurrentAcceleration();  
    AddFloat("ShipAcceleration", currentLinAccel);

    // 3) Linear jerk = (Δacceleration / Δtime)
    float linJerk = (currentLinAccel - oldLinAccel) / Time.fixedDeltaTime;
    AddFloat("ShipLinearJerk", linJerk);
    oldLinAccel = currentLinAccel;

    // 4) Rotational speed
    float rotSpeed = player.GetCurrentRotSpeed(); 
    AddFloat("ShipRotSpeed", rotSpeed);

    // 5) Rotational acceleration
    float currentRotAccel = player.GetCurrentRotAcceleration();  
    AddFloat("ShipRotAcceleration", currentRotAccel);

    // 6) Rotational jerk
    float rotJerk = (currentRotAccel - oldRotAccel) / Time.fixedDeltaTime;
    AddFloat("ShipRotJerk", rotJerk);
    oldRotAccel = currentRotAccel;

    // 7) Current ship size (index)
    Add("ShipSizeIndex", PlayerController.sizeIndex);
  }

  /// <summary>
  /// Adds a float value (converted to int) to the telemetry if AI is controlling.
  /// </summary>
  public void AddFloat(string key, float value)
  {
    if (!PlayerController.aiControlling) return;
    int intVal = Mathf.RoundToInt(value);
    Add(key, intVal);
  }

  /// <summary>
  /// Logs how much damage the ship has taken.
  /// </summary>
  public void AddDamageTaken(float damage)
  {
    // Convert to int internally for simple CSV logging
    AddFloat("DamageTaken", damage);
  }

  /// <summary>
  /// Logs that the player ship was destroyed.
  /// </summary>
  public void AddDestruction()
  {
    Add("ShipDestructions");
  }

  /// <summary>
  /// Logs each player death.
  /// </summary>
  public void IncrementDeaths()
  {
    Add("Deaths");
  }

  /// <summary>
  /// Logs that a shot (or multiple shots) were fired.
  /// </summary>
  public void AddShotsFired(int count = 1)
  {
    Add("ShotsFired", count);
  }

  /// <summary>
  /// Logs that a shot (or multiple shots) were fired.
  /// </summary>
  public void AddMissilesFired(int count = 1)
  {
    Add("MissilesFired", count);
  }

  /// <summary>
  /// Called when a boss spawns or begins. Tracks start time.
  /// </summary>
  public void MarkBossSpawned()
  {
    bossStartTime = Time.time;
    Add("BossSpawned");
  }

  /// <summary>
  /// Called when a boss is defeated; logs time to defeat and how many deaths occurred.
  /// </summary>
  public void MarkBossDefeated()
  {
    float timeToDefeat = Time.time - bossStartTime;
    AddFloat("BossDefeatTime", timeToDefeat);

    // If we have a "Deaths" counter, also record that at boss defeat.
    if (valuePairs.ContainsKey("Deaths"))
    {
      Add("DeathsBeforeBossDefeat", valuePairs["Deaths"]);
    }
    else
    {
      Add("DeathsBeforeBossDefeat", 0);
    }

    Add("BossDefeated");
  }

  public void Add(string key, int value = 1)
  {
    // if (!PlayerController.aiControlling) return;

    if (valuePairs.ContainsKey(key))
    {
      valuePairs[key] += value;
      // Debug.Log(key + ": " + valuePairs[key]);
    }
    else
    {
      valuePairs.Add(key, value);
      // Debug.Log(key + ": " + value);
    }
  }

  // writes everything to app data in a simple csv
  private void Save()
  {
    string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    string fileName = $"telemetry_{timestamp}.csv";
    string filePath = Path.Combine(Application.persistentDataPath, fileName);

    using (StreamWriter writer = new StreamWriter(filePath))
    {
      writer.WriteLine("Key,Value"); // CSV Header
      foreach (var pair in valuePairs)
      {
        writer.WriteLine($"{pair.Key},{pair.Value}");
      }
    }

    Debug.Log($"Telemetry saved to {filePath}");
  }

  private void Clear()
  {
    valuePairs.Clear();
  }

}