using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VignetteControl : MonoBehaviour
{

  Image image;

  // Start is called before the first frame update
  void Start()
  {
    image = GetComponent<Image>();
    if (image)
    {
      image.enabled = false;
    }
    else
    {
      Debug.Log("image not found");
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (image) image.enabled = PlayerHealthController.BULLET_TIME;
  }

}
