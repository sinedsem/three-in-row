using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Cell
{
    public int Row { get; }
    public int Col { get; }

    public Cell(int row, int col)
    {
        this.Row = row;
        this.Col = col;
    }

    public override string ToString()
    {
        return "(" + Row + ";" + Col + ")";
    }
}

public class GameController : MonoBehaviour
{
    public Settings settings;
    public GameObject itemPrefab;
    public GameObject linePrefab;

    private GameObject[,] board;

    private int pendingActions = 0;

    void Start()
    {
        board = new GameObject[settings.rows, settings.cols];

        var cellStartX = -settings.cols * settings.cellSize / 2 + settings.cellSize / 2;
        var cellStartY = -settings.rows * settings.cellSize / 2 + settings.cellSize / 2;

        for (var i = 0; i < settings.rows; i++)
        {
            for (var j = 0; j < settings.cols; j++)
            {
                var item = Instantiate(itemPrefab,
                    new Vector3(cellStartX + j * settings.cellSize, cellStartY + i * settings.cellSize),
                    Quaternion.identity);
                var itemController = item.GetComponent<ItemController>();
                itemController.cellSize = settings.cellSize;
                itemController.swapTime = settings.swapTime;
                itemController.MovementFinished += ActionFinished;

                board[i, j] = item;
            }
        }

        DrawLines();
    }

    private void DrawLines()
    {
        var startX = -settings.cols * settings.cellSize / 2;
        var startY = -settings.rows * settings.cellSize / 2;

        for (var j = 0; j <= settings.cols; j++)
        {
            var line = Instantiate(linePrefab, new Vector3(startX + j * settings.cellSize, 0), Quaternion.identity);
            line.transform.localScale = new Vector3(1, settings.rows * settings.cellSize, 1);
        }

        for (var j = 0; j <= settings.rows; j++)
        {
            var line = Instantiate(linePrefab, new Vector3(0, startY + j * settings.cellSize, 0),
                Quaternion.Euler(0, 0, 90));
            line.transform.localScale = new Vector3(1, settings.cols * settings.cellSize, 1);
        }
    }

    private void ActionFinished()
    {
        pendingActions--;
    }

    public void OnSwap(Cell cell1, Cell cell2)
    {
        if (pendingActions != 0)
        {
            return;
        }

        pendingActions = 2;

        var item1 = board[cell1.Row, cell1.Col];
        var item2 = board[cell2.Row, cell2.Col];

        item1.GetComponent<ItemController>().Move(cell1, cell2);
        item2.GetComponent<ItemController>().Move(cell2, cell1);

        board[cell1.Row, cell1.Col] = item2;
        board[cell2.Row, cell2.Col] = item1;
    }
}