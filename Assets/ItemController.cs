using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public int type;

    public float cellSize;
    public float swapTime;
    public float fallTime;

    public event Action MovementFinished;

    public bool deleted;

    private float _time;
    private Vector3 _startPosition;
    private Vector3 _target;
    private int _rowsToFall;

    private bool _moving;
    private bool _falling;

    public void Move(Cell me, Cell other)
    {
        if (_moving)
        {
            transform.position = _target;
        }

        _moving = true;
        _startPosition = transform.position;
        _time = 0;
        _target = _startPosition + new Vector3((other.Col - me.Col) * cellSize, (other.Row - me.Row) * cellSize);
    }

    public void Fall(Cell me, int rowsToFall)
    {
        _falling = true;
        _startPosition = transform.position;
        _time = 0;
        _rowsToFall = rowsToFall;
        _target = _startPosition + new Vector3(0, -rowsToFall * cellSize);
    }

    private void FixedUpdate()
    {
        if (_moving || _falling)
        {
            _time += Time.fixedDeltaTime / (_moving ? swapTime : fallTime * (float) Math.Sqrt(_rowsToFall));
            transform.position = Vector3.Lerp(_startPosition, _target, _time);
            if (transform.position == _target)
            {
                _moving = false;
                _falling = false;
                MovementFinished?.Invoke();
            }
        }
    }
}