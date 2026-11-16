using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gates : MonoBehaviour
{
    [SerializeField]
    private int _score = 0;

    private void OnTriggerEnter(Collider other)
    {
        var ball = other.GetComponent<Ball>();

        if (ball == null)
        {
            return;
        }
        _score++;

        Debug.Log($"Гол! Текущий счёт: {_score}");

        Destroy(ball.gameObject);
    }
}
