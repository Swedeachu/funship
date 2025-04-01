using System.Collections;
using UnityEngine;
using TMPro;

public class BillboardService : MonoBehaviour
{

  public static BillboardService Instance { get; private set; }

  [Header("Text Prefab")]
  public GameObject textPrefab; // Assign a TextMeshProUGUI prefab in the inspector

  private void Awake()
  {
    // Setup singleton
    if (Instance != null && Instance != this)
    {
      Destroy(this.gameObject);
      return;
    }

    Instance = this;
  }

  /// <summary>
  /// Instantiates a text prefab, sets its text, and auto-destroys it after 3 seconds.
  /// </summary>
  /// <param name="msg">Text to display</param>
  public void ShowText(string msg)
  {
    if (textPrefab == null)
    {
      Debug.LogWarning("Text prefab not assigned to BillboardService.");
      return;
    }

    GameObject instance = Instantiate(textPrefab, transform); // parent to canvas
    TextMeshProUGUI tmp = instance.GetComponent<TextMeshProUGUI>();

    if (tmp != null)
    {
      tmp.text = msg;
    }

    Destroy(instance, 3f);
  }

}
