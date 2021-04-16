using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class Helpers
{
    // Reference: https://answers.unity.com/questions/938496/buttononclickaddlistener.html
    // UI SetListener helper that removes previous listener and adds a new one
    // this version is for onClick.
    public static void SetListener(this UnityEvent uEvent, UnityAction call)
    {
        uEvent.RemoveAllListeners();
        uEvent.AddListener(call);
    }
}
