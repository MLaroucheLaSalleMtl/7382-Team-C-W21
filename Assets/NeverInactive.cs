using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeverInactive : MonoBehaviour
{
    private void LateUpdate()
    {
        if (!gameObject.activeSelf)
            Invoke(nameof(Activate), 10f);
    }

    void Activate()
    {
        gameObject.SetActive(true);
    }
}
