using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private Transform platform;
    [SerializeField] private bool canMove;
    [SerializeField] private Transform[] points;
    
    private IEnumerator Start()
    {
        platform.position = points[0].position;

        while (canMove)
        {
            platform.DOMove(points[1].position, duration).OnComplete(() => 
            {
                platform.DOMove(points[0].position, duration);
            });

            yield return new WaitForSeconds(duration * 2);
        }
          
    }
}
