using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void ToggleMiniMap()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
