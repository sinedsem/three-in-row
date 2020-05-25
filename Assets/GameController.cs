using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public Settings settings;
    public GameObject itemPrefab;
    public GameObject linePrefab;
    public Sprite[] itemSprites;

    private GameObject[,] _board;
    private int _pendingActions = 0;
    private Cell? _swapping1;
    private Cell? _swapping2;
    private float _cellStartX;
    private float _cellStartY;

    void Start()
    {
        _cellStartX = -settings.cols * settings.cellSize / 2 + settings.cellSize / 2;
        _cellStartY = -settings.rows * settings.cellSize / 2 + settings.cellSize / 2;

        FillBoard();

        // DrawLines();
    }

    private void CheckBoard2()
    {
        for (int j = 0; j < settings.cols; j++)
        {
            int remaining = 0;
            for (int i = 0; i < settings.rows; i++)
            {
                var cell = new Cell(i, j);
                var item = GetItem(cell);
                if (item.deleted)
                {
                    Destroy(_board[i, j]);
                }
                else
                {
                    if (i > remaining)
                    {
                        _pendingActions++;
                        item.Fall(cell, i - remaining);
                        _board[remaining, j] = _board[i, j];
                    }

                    remaining++;
                }
            }

            int toAdd = settings.rows - remaining;

            for (int i = 0; i < toAdd; i++)
            {
                var spawnRow = settings.rows + i;
                var fallRow = remaining++;
                var spawned = SpawnItem(spawnRow, j, Random.Range(0, itemSprites.Length));
                _board[fallRow, j] = spawned;

                spawned.GetComponent<ItemController>().Fall(new Cell(spawnRow, j), spawnRow - fallRow);
                _pendingActions++;
            }
        }
    }

    public void OnSwap(Cell cell1, Cell cell2)
    {
        if (_pendingActions != 0)
        {
            return;
        }

        _pendingActions = 4;

        _swapping1 = cell1;
        _swapping2 = cell2;

        _board[cell1.Row, cell1.Col].GetComponent<ItemController>().Move(cell1, cell2);
        _board[cell2.Row, cell2.Col].GetComponent<ItemController>().Move(cell2, cell1);
    }

    private void FillBoard()
    {
        _board = new GameObject[settings.rows, settings.cols];

        for (var i = 0; i < settings.rows; i++)
        {
            for (var j = 0; j < settings.cols; j++)
            {
                var cellType = Random.Range(0, itemSprites.Length);

                while (TwoLeftSameType(i, j, cellType) || TwoAboveSameType(i, j, cellType))
                {
                    cellType = (cellType + 1) % itemSprites.Length;
                }


                var item = SpawnItem(i, j, cellType);

                _board[i, j] = item;
            }
        }
    }

    private GameObject SpawnItem(int row, int col, int cellType)
    {
        var item = Instantiate(itemPrefab,
            new Vector3(_cellStartX + col * settings.cellSize, _cellStartY + row * settings.cellSize),
            Quaternion.identity);
        var itemController = item.GetComponent<ItemController>();
        itemController.cellSize = settings.cellSize;
        itemController.swapTime = settings.swapTime;
        itemController.fallTime = settings.fallTime;
        itemController.MovementFinished += ActionFinished;
        itemController.type = cellType;
        var spriteRenderer = item.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = itemSprites[cellType];
        return item;
    }

    private bool TwoAboveSameType(int row, int col, int cellType)
    {
        if (row < 2)
        {
            return false;
        }

        return GetItem(new Cell(row - 1, col)).type == cellType &&
               GetItem(new Cell(row - 2, col)).type == cellType;
    }

    private bool TwoLeftSameType(int row, int col, int cellType)
    {
        if (col < 2)
        {
            return false;
        }

        return GetItem(new Cell(row, col - 1)).type == cellType &&
               GetItem(new Cell(row, col - 2)).type == cellType;
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
        _pendingActions--;
        if (_pendingActions == 0)
        {
            CheckBoard();
            CheckBoard2();
        }

        if (_pendingActions == 2 && _swapping1.HasValue && _swapping2.HasValue)
        {
            var cell1 = _swapping1.Value;
            var cell2 = _swapping2.Value;
            var item1 = _board[cell1.Row, cell1.Col];
            var item2 = _board[cell2.Row, cell2.Col];
            _board[cell1.Row, cell1.Col] = item2;
            _board[cell2.Row, cell2.Col] = item1;
            if (CheckBoard())
            {
                _pendingActions = 0;
                CheckBoard2();
            }
            else
            {
                _board[cell1.Row, cell1.Col] = item1;
                _board[cell2.Row, cell2.Col] = item2;
                item1.GetComponent<ItemController>().Move(cell2, cell1);
                item2.GetComponent<ItemController>().Move(cell1, cell2);
            }

            _swapping1 = null;
            _swapping2 = null;
        }
    }

    public bool CheckBoard()
    {
        var dictionary = new Dictionary<Cell, HashSet<Cell>>();

        for (int i = 0; i < settings.rows; i++)
        {
            for (int j = 0; j < settings.cols - 2; j++)
            {
                CheckCells(
                    new Cell(i, j),
                    new Cell(i, j + 1),
                    new Cell(i, j + 2),
                    dictionary);
            }
        }

        for (int i = 0; i < settings.rows - 2; i++)
        {
            for (int j = 0; j < settings.cols; j++)
            {
                CheckCells(
                    new Cell(i, j),
                    new Cell(i + 1, j),
                    new Cell(i + 2, j),
                    dictionary);
            }
        }

        if (dictionary.Count == 0)
        {
            return false;
        }

        var processedCells = new HashSet<Cell>();

        foreach (var cell in dictionary.Keys)
        {
            if (processedCells.Contains(cell))
            {
                continue;
            }

            var set = dictionary[cell];
            processedCells.UnionWith(set);

            foreach (var c in set)
            {
                GetItem(c).deleted = true;
            }
        }

        return true;
    }

    private void CheckCells(Cell cell1, Cell cell2, Cell cell3, Dictionary<Cell, HashSet<Cell>> dictionary)
    {
        var type1 = GetItem(cell1).type;
        var type2 = GetItem(cell2).type;
        var type3 = GetItem(cell3).type;

        if (type1 == type2 && type2 == type3)
        {
            HashSet<Cell> set;
            if (dictionary.ContainsKey(cell1))
            {
                set = dictionary[cell1];
            }
            else if (dictionary.ContainsKey(cell2))
            {
                set = dictionary[cell2];
            }
            else if (dictionary.ContainsKey(cell3))
            {
                set = dictionary[cell3];
            }
            else
            {
                set = new HashSet<Cell>();
            }

            set.Add(cell1);
            set.Add(cell2);
            set.Add(cell3);
            dictionary[cell1] = set;
            dictionary[cell2] = set;
            dictionary[cell3] = set;
        }
    }

    private ItemController GetItem(Cell cell)
    {
        return _board[cell.Row, cell.Col].GetComponent<ItemController>();
    }
}