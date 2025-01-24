using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCollision : MonoBehaviour
{
    [SerializeField] private Transform platform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(platform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }
}
