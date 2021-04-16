using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapManager : MonoBehaviour
{
    // All information from this player (rotation, ...)
    [SerializeField] private Transform _player;

    private void LateUpdate()
    {
        // I will do this just for Y because my camera cannot go in the terrain
        transform.position = new Vector3(_player.position.x, transform.position.y, _player.position.z);
    }
}
