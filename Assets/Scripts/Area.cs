using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class Area : MonoBehaviour
{
    public Vector2[,] spawnPositions;
    public GameObject[] bgs;
    public bool visible;
    public int areaColumns, areaRows;

    private int[,] cellAreaLevel;
    private bool[,] cellArea;

    private GameObject[,] areaCells;
    private int currentCellsCount;
    public int levelsCount;
    private GameObject[] levelsBg;
    
    private float moveAreaDelay = 3f;

    private void SetupSpawnPositions()
    {
        spawnPositions = new Vector2[Constants.Columns, Constants.Rows];
        for (int column = 0; column < Constants.Rows; column++)
        {
            for (int row = 0; row < Constants.Columns; row++)
            {
                spawnPositions[column, row] = Constants.BottomRight + new Vector2(column * Constants.BallsSize.x, row * Constants.BallsSize.y);
            }
        }
    }

    public void EnableArea()
    {
        CreateNewArea(areaColumns, areaRows);
    }

    public bool CheckCellArea(int column, int row)
    {
        if(cellAreaLevel[column, row] > 0)
        {
            return true;
        }else
        {
            return false;
        }
    }

    public IEnumerator MoveAreaInRandomDirection()
    {
        int areaLeftColumn = 0;
        int areaBottomRow = 0;

        
        while (visible)
        {

            for (int column = 0; column < Constants.Columns; column++)
            {
                for (int row = 0; row < Constants.Rows; row++)
                {
                    if (cellArea[column, row])
                    { 

                        areaBottomRow = row;
                        areaLeftColumn = column;
                        column = Constants.Columns;
                        row = Constants.Rows;
                    }
                }
            }

            List<Vector2> possibleDirections = new List<Vector2>()
                {new Vector2(1f, 1f),
                new Vector2(-1f, -1f),
                new Vector2(1f, -1f),
                new Vector2(-1f, 1f),
                new Vector2(1f, 0f),
                new Vector2(-1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(0f, -1f)};
                    
            for(int i = 0; i < possibleDirections.Count; i++)
            {
                if (areaLeftColumn + possibleDirections[i].x + areaColumns - 1f >= Constants.Columns ||
                    areaBottomRow + possibleDirections[i].y < 0 ||
                    areaBottomRow + possibleDirections[i].y + areaRows - 1f >= Constants.Rows ||
                    areaLeftColumn + possibleDirections[i].x < 0)
                {
                    possibleDirections.Remove(possibleDirections[i]);
                    i = -1;
                }
            }

            Vector2 moveDirection = possibleDirections[Random.Range(0, possibleDirections.Count)];
            GameObject[,] areaCellsCopy = new GameObject[Constants.Columns, Constants.Rows];
            int[,] cellAreaLevelCopy = new int[Constants.Columns, Constants.Rows];
            for (int column = areaLeftColumn + (int)moveDirection.x; column < areaLeftColumn + areaColumns + (int)moveDirection.x; column++)
            {
                for (int row = areaBottomRow + (int)moveDirection.y; row < areaBottomRow + areaRows + (int)moveDirection.y; row++)
                {
                    if (column < Constants.Columns &&
                        row >= 0 &&
                        row < Constants.Rows &&
                        column >= 0 &&
                        column - (int)moveDirection.x >= 0 &&
                        column - (int)moveDirection.x < Constants.Columns &&
                        row - (int)moveDirection.y >= 0 &&
                        row - (int)moveDirection.y < Constants.Rows &&
                        cellAreaLevel[column - (int)moveDirection.x, row - (int)moveDirection.y] != 0 &&
                        visible)
                    {
                        cellAreaLevelCopy[column, row] = cellAreaLevel[column - (int)moveDirection.x, row - (int)moveDirection.y];
                        areaCellsCopy[column, row] = Instantiate(areaCells[column - (int)moveDirection.x, row - (int)moveDirection.y], spawnPositions[column, row], areaCells[column - (int)moveDirection.x, row - (int)moveDirection.y].transform.rotation);
                        Destroy(areaCells[column - (int)moveDirection.x, row - (int)moveDirection.y]);
                    }
                }

            }
            cellAreaLevel = new int[Constants.Columns, Constants.Rows];
            areaCells = new GameObject[Constants.Columns, Constants.Rows];
            cellArea = new bool[Constants.Columns, Constants.Rows];
            for (int column = areaLeftColumn + (int)moveDirection.x; column < areaLeftColumn + areaColumns + (int)moveDirection.x; column++)
            {
                for (int row = areaBottomRow + (int)moveDirection.y; row < areaBottomRow + areaRows + (int)moveDirection.y; row++)
                {
                    areaCells[column, row] = areaCellsCopy[column, row];
                    cellAreaLevel[column, row] = cellAreaLevelCopy[column, row];
                    cellArea[column, row] = true;
                }
                        
            }



            areaLeftColumn += (int)moveDirection.x;
            areaBottomRow += (int)moveDirection.y;
            yield return new WaitForSeconds(moveAreaDelay);  
        }
    }

    public void MakeAreaVisible()
    {
        visible = true;
        for (int column = 0; column < Constants.Columns; column++)
        {
            for (int row = 0; row < Constants.Rows; row++)
            {
                if (cellAreaLevel[column, row] == 1)
                {
                    
                    areaCells[column, row] = GameObject.Instantiate(levelsBg[0], spawnPositions[column, row], levelsBg[0].transform.rotation);
                    cellAreaLevel[column, row]++;
                }
            }
        }
        StartCoroutine(MoveAreaInRandomDirection());

    }

    private void ClearCurrentArea()
    {
        
        levelsBg = new GameObject[levelsCount];
        areaCells = new GameObject[Constants.Columns, Constants.Rows];
        cellAreaLevel = new int[Constants.Columns, Constants.Rows];
        cellArea = new bool[Constants.Columns, Constants.Rows];
        visible = false;
        currentCellsCount = areaColumns * areaRows;

        List<GameObject> possibleBgs = new List<GameObject>();
        possibleBgs.AddRange(bgs);

        for (int i = 0; i < levelsCount; i++)
        {
            GameObject newBg = possibleBgs[Random.Range(0, possibleBgs.Count)]; ;
            possibleBgs.Remove(newBg);
            levelsBg[i] = newBg;
        }    
    }
        
    public bool SameTypeWithBackground(Ball ball, int column, int row)
    {
        if (ball.Type == areaCells[column, row].GetComponent<Background>().deletingType || areaCells[column, row].GetComponent<Background>().deletingType == "Empty")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void DeleteAreaCell(int column, int row)
    {
        GameObject.Destroy(areaCells[column, row]);

        if (cellAreaLevel[column, row] == levelsCount + 1)
        {

            cellAreaLevel[column, row] = 0;
            currentCellsCount--;
        }
        else
        {
            //appearance should be here
            areaCells[column, row] = GameObject.Instantiate(levelsBg[cellAreaLevel[column, row] - 1], spawnPositions[column, row], levelsBg[cellAreaLevel[column, row] - 1].transform.rotation);
            cellAreaLevel[column, row]++;
        }

        if (currentCellsCount == 0)
        {
            CreateNewArea(areaColumns, areaRows);
        }
    }

    public void CreateNewArea(int columns, int rows)
    {
        
        ClearCurrentArea();
        int horLeftOffset = Random.Range(0, Constants.Columns - columns);
        int vertLeftOffset = Random.Range(0, Constants.Rows - rows);
        for (int column = horLeftOffset; column < horLeftOffset + columns; column++)
        {
            for (int row = vertLeftOffset; row < vertLeftOffset + rows; row++)
            {
                cellAreaLevel[column, row] = 1;
                cellArea[column, row] = true;
            }
        }

    }

    private T1[,] Copy<T1>(T1[,] array, int left, int bottom, int horSize, int verSize)
    {
        T1[,] newArray = new T1[horSize, verSize];
        for (int i = left; i < left + horSize; i++)
            for (int j = bottom; j < bottom + verSize; j++)
                newArray[i - left, j - bottom] = array[i, j];
        return newArray;
    }
    void Start()
    {
        SetupSpawnPositions();
        EnableArea();
    }
}
