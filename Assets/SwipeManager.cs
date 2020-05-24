using System;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class SwapEvent : UnityEvent<Cell, Cell>
{
}

public class SwipeManager : MonoBehaviour
{
    public Settings settings;
    public Camera camera;

    public float swipeThreshold = 50f;
    public float timeThreshold = 0.3f;

    private Cell? initialCell;
    private Cell? currentCell;


    public SwapEvent OnSwap;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            initialCell = CalculateCell(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            currentCell = CalculateCell(Input.mousePosition);
            CheckSwap();
        }

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                initialCell = CalculateCell(touch.position);
            }

            if (touch.phase == TouchPhase.Moved)
            {
                currentCell = CalculateCell(touch.position);
                CheckSwap();
            }
        }
    }

    private void CheckSwap()
    {
        if (!initialCell.HasValue || !currentCell.HasValue)
        {
            return;
        }

        var init = initialCell.Value;
        var cur = currentCell.Value;

        if (init.Row == cur.Row && init.Col == cur.Col)
        {
            return;
        }

        if (init.Row == cur.Row && Math.Abs(init.Col - cur.Col) == 1
            || init.Col == cur.Col && Math.Abs(init.Row - cur.Row) == 1)
        {
            OnSwap.Invoke(init, cur);
            initialCell = null;
        }
    }

    private Cell? CalculateCell(Vector3 mousePosition)
    {
        var worldPoint = camera.ScreenToWorldPoint(mousePosition);

        var boardWidth = settings.cols * settings.cellSize;
        var relativeMouseX = worldPoint.x + boardWidth / 2;
        var col = (int) Math.Floor(relativeMouseX / settings.cellSize);
        if (col < 0 || col >= settings.cols)
        {
            return null;
        }

        var boardHeight = settings.rows * settings.cellSize;
        var relativeMouseY = worldPoint.y + boardHeight / 2;
        var row = (int) Math.Floor(relativeMouseY / settings.cellSize);
        if (row < 0 || row >= settings.rows)
        {
            return null;
        }

        return new Cell(row, col);
    }
}