using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TelemetryManager : MonoBehaviour
{
  private Dictionary<string, int> valuePairs = new Dictionary<string, int>();

  private static TelemetryManager instance;
  public static TelemetryManager Instance => instance;

  private float nextLogTime = 0f;
  private float logInterval = 1f;

  private float oldLinAccel = 0f;
  private float oldRotAccel = 0f;

  private float bossStartTime = 0f;
  private float bossDefeatTime = -1f;

  private List<string> periodicRows = new List<string>();
  private int shotsFiredCount = 0;
  private int missilesFiredCount = 0;
  private int damageTakenCount = 0;

  private void Awake()
  {
    if (instance != null && instance != this)
    {
      Destroy(this.gameObject);
      return;
    }
    instance = this;
    DontDestroyOnLoad(this.gameObject);

    periodicRows.Add("Time,ShipVelocity,ShipAcceleration,ShipLinearJerk,ShipRotSpeed,ShipRotAcceleration,ShipRotJerk,ShipSizeIndex,ShotsFired,MissilesFired,DamageTaken,ShipDestructions,Deaths,BossDefeatTime");
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

  private void LogPeriodicData()
  {
    PlayerController player = FindObjectOfType<PlayerController>();
    if (player == null) return;

    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

    float timeStamp = Time.time;
    float velocity = rb.velocity.magnitude;
    float currentLinAccel = player.GetCurrentAcceleration();
    float linJerk = (currentLinAccel - oldLinAccel) / Time.fixedDeltaTime;
    oldLinAccel = currentLinAccel;
    float rotSpeed = player.GetCurrentRotSpeed();
    float currentRotAccel = player.GetCurrentRotAcceleration();
    float rotJerk = (currentRotAccel - oldRotAccel) / Time.fixedDeltaTime;
    oldRotAccel = currentRotAccel;
    int sizeIndex = PlayerController.sizeIndex;

    int destructions = valuePairs.ContainsKey("ShipDestructions") ? valuePairs["ShipDestructions"] : 0;
    int deaths = valuePairs.ContainsKey("Deaths") ? valuePairs["Deaths"] : 0;
    int bossTime = bossDefeatTime >= 0f ? Mathf.RoundToInt(bossDefeatTime) : -1;

    string row = string.Format("{0:F2},{1:F2},{2:F2},{3:F2},{4:F2},{5:F2},{6:F2},{7},{8},{9},{10},{11},{12},{13}",
        timeStamp,
        velocity,
        currentLinAccel,
        linJerk,
        rotSpeed,
        currentRotAccel,
        rotJerk,
        sizeIndex,
        shotsFiredCount,
        missilesFiredCount,
        damageTakenCount,
        destructions,
        deaths,
        bossTime);

    periodicRows.Add(row);

    shotsFiredCount = 0;
    missilesFiredCount = 0;
    damageTakenCount = 0;
  }

  public void AddFloat(string key, float value)
  {
    if (!PlayerController.aiControlling) return;
    int intVal = Mathf.RoundToInt(value);
    Add(key, intVal);
  }

  public void AddDamageTaken(float damage)
  {
    damageTakenCount += Mathf.RoundToInt(damage);
  }

  public void AddDestruction()
  {
    Add("ShipDestructions");
  }

  public void IncrementDeaths()
  {
    Add("Deaths");
  }

  public void AddShotsFired(int count = 1)
  {
    shotsFiredCount += count;
  }

  public void AddMissilesFired(int count = 1)
  {
    missilesFiredCount += count;
  }

  public void MarkBossSpawned()
  {
    bossStartTime = Time.time;
    Add("BossSpawned");
  }

  public void MarkBossDefeated()
  {
    bossDefeatTime = Time.time - bossStartTime;
    AddFloat("BossDefeatTime", bossDefeatTime);

    int deaths = valuePairs.ContainsKey("Deaths") ? valuePairs["Deaths"] : 0;
    Add("DeathsBeforeBossDefeat", deaths);
    Add("BossDefeated");
  }

  public void Add(string key, int value = 1)
  {
    if (valuePairs.ContainsKey(key))
    {
      valuePairs[key] += value;
    }
    else
    {
      valuePairs.Add(key, value);
    }
  }

  private void Save()
  {
    string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    string fileName = $"telemetry_{timestamp}.csv";
    string filePath = Path.Combine(Application.persistentDataPath, fileName);

    using (StreamWriter writer = new StreamWriter(filePath))
    {
      foreach (string row in periodicRows)
      {
        writer.WriteLine(row);
      }
    }

    Debug.Log($"Telemetry saved to {filePath}");
  }

  private void Clear()
  {
    periodicRows.Clear();
    periodicRows.Add("Time,ShipVelocity,ShipAcceleration,ShipLinearJerk,ShipRotSpeed,ShipRotAcceleration,ShipRotJerk,ShipSizeIndex,ShotsFired,MissilesFired,DamageTaken,ShipDestructions,Deaths,BossDefeatTime");
    valuePairs.Clear();
    bossDefeatTime = -1f;
  }

}
