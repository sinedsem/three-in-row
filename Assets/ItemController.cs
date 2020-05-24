using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public float cellSize;
    public float swapTime;

    public event Action MovementFinished;

    private float t;
    private Vector3 startPosition;
    private Vector3 target;
    private bool started;

    public void Move(Cell me, Cell other)
    {
        started = true;
        startPosition = transform.position;
        t = 0;
        target = startPosition + new Vector3((other.Col - me.Col) * cellSize, (other.Row - me.Row) * cellSize);
    }

    private void FixedUpdate()
    {
        if (started)
        {
            t += Time.fixedDeltaTime / swapTime;
            transform.position = Vector3.Lerp(startPosition, target, t);
            if (transform.position == target)
            {
                MovementFinished?.Invoke();
                started = false;
            }
        }
    }
}