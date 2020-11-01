using DG.Tweening;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class BallsManager : MonoBehaviour
{
    public BallsArray balls;
    public RandomizingBag<string> randomizingBag;

    public static GameState state = GameState.None;
    public static GameMode mode = GameMode.None;

    private Vector2[,] SpawnPositions;
    public GameObject[] BallsPrefabs;

    private bool[,] blockedBalls;

    void Start()
    {
        InitializeTypesOnPrefabBalls();
        InitializeBallAndSpawnPositions();
    }

    #region поиск и обработка нажатий
    private void Find1stBallOnClicking()
    {
        //get the hit position
        var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null) //we have a hit!!!
        {
            hitGoTimeline.Add(hit.collider.gameObject);
        }
        else
        {
            hitGoTimeline.Add(null);
        }
    }
    private void Find2stBallOnClicking()
    {
        //get the hit position
        var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null) //we have a hit!!!
        {
            hitGo2Timeline.Add(hit.collider.gameObject);
        }
        else
        {
            hitGo2Timeline.Add(null);
        }
    }
    private void Process1stClick()
    {
        if (hitGoTimeline.Last() != null)
        {
            if (mode == GameMode.ChangingWave)
            {

                StartCoroutine(StartWaveAnimation(hitGoTimeline.Last().GetComponent<Ball>().Column, hitGoTimeline.Last().GetComponent<Ball>().Row));
                //UIManager.instance.SwitchWaveMode();
            }
            else
            if (mode == GameMode.TypeDeleting)
            {

                List<BallInfo> deletingBalls = DeleteAllBallsOfType(hitGoTimeline.Last().GetComponent<Ball>().Type);
                deletingBalls = deletingBalls.Distinct().ToList();

                StartCoroutine(FillEmptyBallsAndClearMathces(CreateNewBalls, deletingBalls));
                //UIManager.instance.SwitchColorDeletingMode();
            }
            else
            if (mode == GameMode.HorizontalLineDeleting)
            {
                List<BallInfo> deletingBalls = RemoveLineFromScene(balls.GetEntireRow(hitGoTimeline.Last()));
                deletingBalls = deletingBalls.Distinct().ToList();
                StartCoroutine(FillEmptyBallsAndClearMathces(CreateNewBallsForHorizontalLine, deletingBalls));
               // UIManager.instance.SwitchDeleteHorLineMode();
            }
            else
            if (mode == GameMode.VerticalLineDeleting)
            {
                List<BallInfo> deletingBalls = RemoveLineFromScene(balls.GetEntireColumn(hitGoTimeline.Last()));
                deletingBalls = deletingBalls.Distinct().ToList();
                StartCoroutine(FillEmptyBallsAndClearMathces(CreateNewBallsForVerticalLine, deletingBalls));
                //UIManager.instance.SwitchDeleteVertLineMode();
            }
            else
            if (mode == GameMode.TrippleBallDeleting)
            {
                StartCoroutine(DeleteThreeBall(hitGoTimeline.Last().GetComponent<Ball>().Column, hitGoTimeline.Last().GetComponent<Ball>().Row));
                //UIManager.instance.SwitchTrippleBallDeletingMode();
            }
            else
            if (mode == GameMode.None || mode == GameMode.DeletingBallsFFD)
            {
                Select();
            }
        }
    }

    #endregion
    #region функции для начальной генерации шаров
    //получение списка всех типов шаров
    private List<string> GetTypesList(GameObject[] ballsPrefabs)
    {
        List<string> typesList = new List<string>();
        for (int i = 0; i < ballsPrefabs.Length; i++)
        {
            typesList.Add(ballsPrefabs[i].GetComponent<Ball>().Type);
        }
        return typesList;
    }
    private void SetupSpawnPositions()
    {
        for (int column = 0; column < Constants.Rows; column++)
        {
            for (int row = 0; row < Constants.Columns; row++)
            {
                SpawnPositions[column, row] = Constants.BottomRight
                    + new Vector2(column * Constants.BallsSize.x, row * Constants.BallsSize.y);
            }
        }
    }
    //определение начального кол-ва шаров различных цветов
    private Dictionary<string, int> GetTypesStartCount()
    {
        //инициализируем словарь
        Dictionary<string, int> typeStartCount = new Dictionary<string, int>();
        for (int i = 0; i < BallsPrefabs.Length; i++)
        {
            typeStartCount.Add(BallsPrefabs[i].GetComponent<Ball>().Type, 0);
        }

        //определяем кол-во первого типа
        int firstTypeStartCount = Random.Range(Constants.MinimumStartTypeCount, Constants.MaximumStartTypeCount);
        int ballsLeftCount = Constants.Columns * Constants.Rows;
        typeStartCount[BallsPrefabs[0].GetComponent<Ball>().Type] = firstTypeStartCount;
        ballsLeftCount -= firstTypeStartCount;

        //определяем кол-во остальных типов 
        for (int i = 0; i < BallsPrefabs.Length; i++)
        {
            int currentBallStartCount = Random.Range(Constants.MinimumStartTypeCount, Mathf.Min(ballsLeftCount - (BallsPrefabs.Length - (i + 1)) * Constants.MinimumStartTypeCount));
            typeStartCount[BallsPrefabs[i].GetComponent<Ball>().Type] = currentBallStartCount;
            ballsLeftCount -= currentBallStartCount;
        }
        return typeStartCount;
    }
    //проверка на возможность поставить шар типа на конкретное место при старте
    private bool PossibleToUse(string type, int column, int row)
    {
        if (column >= 1)
        {
            if (balls[column - 1, row] != null &&
                balls[column - 1, row].GetComponent<Ball>().Type == type)
            {
                return false;
            }
        }
        if (column < Constants.Columns - 1)
        {
            if (balls[column + 1, row] != null &&
               balls[column + 1, row].GetComponent<Ball>().Type == type)
            {
                return false;
            }
        }
        if (row >= 1)
        {
            if (balls[column, row - 1] != null &&
               balls[column, row - 1].GetComponent<Ball>().Type == type)
            {
                return false;
            }

        }
        if (row < Constants.Rows - 1)
        {
            if (balls[column, row + 1] != null &&
               balls[column, row + 1].GetComponent<Ball>().Type == type)
            {
                return false;
            }
        }
        if (row < Constants.Rows - 3 && column < Constants.Columns - 3)
        {
            if (balls[column + 1, row + 1] != null &&
                balls[column + 2, row + 2] != null &&
                balls[column + 3, row + 3] != null &&
                type == balls[column + 1, row + 1].GetComponent<Ball>().Type &&
                balls[column + 1, row + 1].GetComponent<Ball>().
                 IsSameType(balls[column + 2, row + 2].GetComponent<Ball>()) &&
                balls[column + 2, row + 2].GetComponent<Ball>().
                 IsSameType(balls[column + 3, row + 3].GetComponent<Ball>()))
            {
                return false;
            }
        }
        if (row >= 3 && column >= 3)
        {
            if (balls[column - 1, row - 1] != null &&
                balls[column - 2, row - 2] != null &&
                balls[column - 3, row - 3] != null &&
                type == balls[column - 1, row - 1].GetComponent<Ball>().Type &&
                balls[column - 1, row - 1].GetComponent<Ball>().
                 IsSameType(balls[column - 2, row - 2].GetComponent<Ball>()) &&
                balls[column - 2, row - 2].GetComponent<Ball>().
                 IsSameType(balls[column - 3, row - 3].GetComponent<Ball>()))
            {
                return false;
            }
        }
        if (row >= 3 && column < Constants.Columns - 3)
        {
            if (balls[column + 1, row - 1] != null &&
                balls[column + 2, row - 2] != null &&
                balls[column + 3, row - 3] != null &&
                type == balls[column + 1, row - 1].GetComponent<Ball>().Type &&
                balls[column + 1, row - 1].GetComponent<Ball>().
                 IsSameType(balls[column + 2, row - 2].GetComponent<Ball>()) &&
                balls[column + 2, row - 2].GetComponent<Ball>().
                 IsSameType(balls[column + 3, row - 3].GetComponent<Ball>()))
            {
                return false;
            }
        }
        if (row < Constants.Rows - 3 && column >= 3)
        {
            if (balls[column - 1, row + 1] != null &&
                balls[column - 2, row + 2] != null &&
                balls[column - 3, row + 3] != null &&
                type == balls[column - 1, row + 1].GetComponent<Ball>().Type &&
                balls[column - 1, row + 1].GetComponent<Ball>().
                 IsSameType(balls[column - 2, row + 2].GetComponent<Ball>()) &&
                balls[column - 2, row + 2].GetComponent<Ball>().
                 IsSameType(balls[column - 3, row + 3].GetComponent<Ball>()))
            {
                return false;
            }
        }
        return true;
    }
    //поиск типа шара которого меньше всего 
    private string FindTypeOfMinimumCount(Dictionary<string, int> typesCurrentCount)
    {
        int minTypeCount = typesCurrentCount[BallsPrefabs[0].GetComponent<Ball>().Type];
        string typeOfMinCount = BallsPrefabs[0].GetComponent<Ball>().Type;
        for (int i = 0; i < BallsPrefabs.Length; i++)
        {
            if (typesCurrentCount[BallsPrefabs[i].GetComponent<Ball>().Type] < minTypeCount)
            {
                typeOfMinCount = BallsPrefabs[i].GetComponent<Ball>().Type;
                minTypeCount = typesCurrentCount[BallsPrefabs[i].GetComponent<Ball>().Type]; ;
            }
        }
        return typeOfMinCount;
    }
    //начальная генерация шаров
    public void InitializeBallAndSpawnPositions()
    {
        if (balls != null)
            DestroyAllBalls();

        hitGoTimeline = new List<GameObject>();
        hitGo2Timeline = new List<GameObject>();

        blockedBalls = new bool[Constants.Columns, Constants.Rows];

        randomizingBag = new RandomizingBag<string>(GetTypesList(BallsPrefabs));
        balls = new BallsArray();
        SpawnPositions = new Vector2[Constants.Columns, Constants.Rows];

        #region определяем начальное кол-во разных типов шаров
        Dictionary<string, int> typesStartCount = GetTypesStartCount();
        Dictionary<string, int> typesCurrentCount = new Dictionary<string, int>();
        for (int i = 0; i < BallsPrefabs.Length; i++)
        {
            typesCurrentCount.Add(BallsPrefabs[i].GetComponent<Ball>().Type, 0);
        }
        #endregion

        for (int column = 0; column < Constants.Columns; column++)
        {
            for (int row = 0; row < Constants.Rows; row++)
            {
                List<string> possibleTypes = new List<string>();
                for (int i = 0; i < BallsPrefabs.Length; i++)
                {
                    if (PossibleToUse(BallsPrefabs[i].GetComponent<Ball>().Type, column, row)) possibleTypes.Add(BallsPrefabs[i].GetComponent<Ball>().Type);
                }
                #region убрать неподходящие спрайты


                for (int i = 0; i < possibleTypes.Count; i++)
                {
                    if (typesCurrentCount[possibleTypes[i]] > typesStartCount[possibleTypes[i]])
                    {
                        possibleTypes.Remove(possibleTypes[i]);
                        i = -1;
                    }
                }
                //предотвращает возможность отсутствия подходящего типа
                if (possibleTypes.Count == 0)
                {
                    string typeOfMinCount = FindTypeOfMinimumCount(typesCurrentCount);

                    for (int newColumn = 0; newColumn < Constants.Columns; newColumn++)
                    {
                        for (int newRow = 0; (newRow < Constants.Rows && newColumn < column) || (newRow < row && newColumn >= column); newRow++)
                        {
                            if (possibleTypes.Count == 0 &&
                               PossibleToUse(balls[newColumn, newRow].GetComponent<Ball>().Type, column, row) &&
                               PossibleToUse(typeOfMinCount, newColumn, newRow))
                            {
                                possibleTypes.Add(balls[newColumn, newRow].GetComponent<Ball>().Type);
                                Destroy(balls[newColumn, newRow]);
                                InstantiateAndPlaceNewBall(newColumn, newRow, GetBallOfType(typeOfMinCount));
                            }
                        }
                    }
                }
                #endregion 

                string newPossibleType = possibleTypes[Random.Range(0, possibleTypes.Count)];
                typesCurrentCount[newPossibleType]++;
                GameObject newBall = GetBallOfType(newPossibleType);
                InstantiateAndPlaceNewBall(column, row, newBall);

            }
        }

        SetupSpawnPositions();
    }
    #endregion
    #region вспомогрательные алгоритмы
    //получение шара по его типу
    private GameObject GetBallOfType(string type)
    {
        for (int i = 0; i < BallsPrefabs.Length; i++)
        {
            if (BallsPrefabs[i].GetComponent<Ball>().Type == type)
            {
                return BallsPrefabs[i];
            }
        }
        return null;
    }

    //обозначение типов шаров
    private void InitializeTypesOnPrefabBalls()
    {
        foreach (var item in BallsPrefabs)
        {
            item.GetComponent<Ball>().Type = item.name;

        }
    }
    private List<BallInfo> GetAllBalls()
    {
        List<BallInfo> allBalls = new List<BallInfo>();
        for (int column = 0; column < Constants.Columns; column++)
        {
            for (int row = 0; row < Constants.Rows; row++)
            {

                if (!blockedBalls[column, row]) allBalls.Add(new BallInfo() { Column = column, Row = row });
            }
        }
        return allBalls;
    }
    #endregion
    #region помощники появления и исчезновения шаров
    #region удаление шариков
    bool makingAreaVisible = false;
    bool removingArea = false;

    private List<BallInfo> RemoveLineFromScene(IEnumerable<GameObject> deletingBalls)
    {
        List<BallInfo> lineInfo = new List<BallInfo>();

        foreach (var item in deletingBalls)
        {
            if (!blockedBalls[item.GetComponent<Ball>().Column, item.GetComponent<Ball>().Row])
            {
                lineInfo.Add(new BallInfo() { Column = item.GetComponent<Ball>().Column, Row = item.GetComponent<Ball>().Row });
                balls[item.GetComponent<Ball>().Column, item.GetComponent<Ball>().Row] = null;
                Destroy(item);
            }

        }
        return lineInfo;
    }

    private void RemoveFromScene(IEnumerable<GameObject> deletingBalls)
    {
        //GameObject explosion = GetRandomExplosion();
        //var newExplosion = Instantiate(explosion, item.transform.position, Quaternion.identity) as GameObject;
        //Destroy(newExplosion, Constants.ExplosionDuration);

        foreach (var item in deletingBalls)
        {


            //int columnOfdeletingBall = item.GetComponent<Ball>().Column;
            //int rowOfDeletingBall = item.GetComponent<Ball>().Row;

            //if (GetComponent<Area>().CheckCellArea(columnOfdeletingBall, rowOfDeletingBall) && !GetComponent<Area>().visible && !removingArea && (mode == GameMode.DeletingBallsFFD || mode == GameMode.None))
            //{
            //    makingAreaVisible = true;
            //    GetComponent<Area>().MakeAreaVisible();
            //}
            //else if (GetComponent<Area>().CheckCellArea(columnOfdeletingBall, rowOfDeletingBall) && GetComponent<Area>().visible && !makingAreaVisible && (mode == GameMode.DeletingBallsFFD || mode == GameMode.None) && GetComponent<Area>().SameTypeWithBackground(item.GetComponent<Ball>(), columnOfdeletingBall, rowOfDeletingBall))
            //{
            //    GetComponent<Area>().DeleteAreaCell(columnOfdeletingBall, rowOfDeletingBall);
            //    removingArea = true;
            //}

            balls[item.GetComponent<Ball>().Column, item.GetComponent<Ball>().Row] = null;
            Destroy(item);


        }

    }
    private void DestroyAllBalls()
    {
        for (int column = 0; column < Constants.Columns; column++)
        {
            for (int row = 0; row < Constants.Rows; row++)
            {
                Destroy(balls[column, row]);
            }
        }
    }
    #endregion
    #region помещение новых шариков на сцену
    //создаёт новый цвет и помещает его в  определённую ячейку
    private void InstantiateAndPlaceNewBall(int column, int row, GameObject newBall)
    {
        GameObject go = Instantiate(newBall,
            Constants.BottomRight + new Vector2(column * Constants.BallsSize.x, row * Constants.BallsSize.y), Quaternion.identity)
            as GameObject;

        //assign the specific properties
        go.GetComponent<Ball>().Assign(newBall.GetComponent<Ball>().Type, column, row);
        go.GetComponent<SpriteRenderer>().sortingOrder += Constants.Rows - go.GetComponent<Ball>().Row - 1;


        balls[column, row] = go;
    }
    private GameObject InstantiateNewBallOfType(string type, BallInfo emptyBall)
    {
        var go = GetBallOfType(type);
        GameObject newBall = Instantiate(go, SpawnPositions[emptyBall.Column, emptyBall.Row] - Constants.appearanceOffset, Quaternion.identity)
            as GameObject;

        newBall.GetComponent<Ball>().Assign(go.GetComponent<Ball>().Type, emptyBall.Column, emptyBall.Row);
        newBall.GetComponent<SpriteRenderer>().sortingOrder += Constants.Rows - newBall.GetComponent<Ball>().Row - 1;


        balls[emptyBall.Column, emptyBall.Row] = newBall;
        return newBall;
    }
    #endregion
    #region определение новых шариков и их создание
    private CreatedBallsInfo CreateNewBallsForVerticalLine(List<BallInfo> emptyBalls)
    {
        Debug.Log("weacrnb " + emptyBalls.Count);
        CreatedBallsInfo newBallInfo = new CreatedBallsInfo();

        var emptyItems = emptyBalls;

        List<BallInfo> ballsWithCompulsoryMatches = new List<BallInfo>();
        Dictionary<BallInfo, string> matchType = new Dictionary<BallInfo, string>();
        foreach (var item in emptyItems)
        {
            if(balls[item.Column, item.Row] == null)
            {
                if (item.Column < Constants.Columns - 1 &&
               item.Column >= 1 &&
               balls[item.Column + 1, item.Row].GetComponent<Ball>().
                IsSameType(balls[item.Column - 1, item.Row].GetComponent<Ball>()))
                {
                    ballsWithCompulsoryMatches.Add(item);
                    matchType.Add(item, balls[item.Column - 1, item.Row].GetComponent<Ball>().Type);
                }
                else
            if (item.Column < Constants.Columns - 2 &&
               balls[item.Column + 1, item.Row].GetComponent<Ball>().
                IsSameType(balls[item.Column + 2, item.Row].GetComponent<Ball>()))
                {
                    ballsWithCompulsoryMatches.Add(item);
                    matchType.Add(item, balls[item.Column + 2, item.Row].GetComponent<Ball>().Type);
                }
                else
            if (item.Column >= 2 &&
               balls[item.Column - 1, item.Row].GetComponent<Ball>().
                IsSameType(balls[item.Column - 2, item.Row].GetComponent<Ball>()))
                {
                    ballsWithCompulsoryMatches.Add(item);
                    matchType.Add(item, balls[item.Column - 2, item.Row].GetComponent<Ball>().Type);
                }
            }
            


        }



        int compulsoryMatchesCount = Random.Range(Mathf.Min(Constants.MinimumCompulsoryMatches, ballsWithCompulsoryMatches.Count), Mathf.Min(Constants.MaximumCompulsoryMatches, ballsWithCompulsoryMatches.Count) + 1);
        for (int i = 0; i < compulsoryMatchesCount; i++)
        {

            BallInfo currentBallForMatch = ballsWithCompulsoryMatches[Random.Range(0, ballsWithCompulsoryMatches.Count)];
            string typeOfCurrentBallForMatch = matchType[currentBallForMatch];
            GameObject newBall = InstantiateNewBallOfType(typeOfCurrentBallForMatch, currentBallForMatch);
            ballsWithCompulsoryMatches.Remove(currentBallForMatch);
            emptyItems = emptyItems.Where(item => item != currentBallForMatch).ToList();
            balls[currentBallForMatch.Column, currentBallForMatch.Row] = newBall;
            newBallInfo.AddBall(newBall);
        }
        for (int i = 0; i < ballsWithCompulsoryMatches.Count; i++)
        {
            randomizingBag.RemoveItemFromBag(matchType[ballsWithCompulsoryMatches[i]]);
            string randomBallType = GetRandomBallType(randomizingBag);
            GameObject newBall = InstantiateNewBallOfType(randomBallType, ballsWithCompulsoryMatches[i]);
            balls[ballsWithCompulsoryMatches[i].Column, ballsWithCompulsoryMatches[i].Row] = newBall;
            emptyItems = emptyItems.Where(item => item != ballsWithCompulsoryMatches[i]).ToList();
            newBallInfo.AddBall(newBall);

        }
        foreach (var item in emptyItems)
        {
            if (balls[item.Column, item.Row] == null)
            {
                string randomBallType = GetRandomBallType(randomizingBag);
                GameObject newBall = InstantiateNewBallOfType(randomBallType, item);
                balls[item.Column, item.Row] = newBall;
                newBallInfo.AddBall(newBall);
            }
        }
        return newBallInfo;
    }
    private CreatedBallsInfo CreateNewBallsForHorizontalLine(List<BallInfo> emptyBalls)
    {

        CreatedBallsInfo newBallInfo = new CreatedBallsInfo();

        var emptyItems = emptyBalls;

        List<BallInfo> ballsWithCompulsoryMatches = new List<BallInfo>();
        Dictionary<BallInfo, string> matchType = new Dictionary<BallInfo, string>();
        foreach (var item in emptyItems)
        {
            if (balls[item.Column, item.Row] == null && !blockedBalls[item.Column, item.Row])
            {

                if (item.Row < Constants.Rows - 1 &&
                item.Row >= 1 &&
                balls[item.Column, item.Row + 1].GetComponent<Ball>().
                IsSameType(balls[item.Column, item.Row - 1].GetComponent<Ball>()))
                {
                    ballsWithCompulsoryMatches.Add(item);
                    matchType.Add(item, balls[item.Column, item.Row - 1].GetComponent<Ball>().Type);
                }
                else
                if (item.Row < Constants.Rows - 2 &&
                balls[item.Column, item.Row + 1].GetComponent<Ball>().
                IsSameType(balls[item.Column, item.Row + 2].GetComponent<Ball>()))
                {
                    ballsWithCompulsoryMatches.Add(item);
                    matchType.Add(item, balls[item.Column, item.Row + 2].GetComponent<Ball>().Type);
                }
                else
                if (item.Row >= 2 &&
                balls[item.Column, item.Row - 1].GetComponent<Ball>().
                IsSameType(balls[item.Column, item.Row - 2].GetComponent<Ball>()))
                {
                    ballsWithCompulsoryMatches.Add(item);
                    matchType.Add(item, balls[item.Column, item.Row - 2].GetComponent<Ball>().Type);
                }

            }
        }

        int compulsoryMatchesCount = Random.Range(Mathf.Min(Constants.MinimumCompulsoryMatches, ballsWithCompulsoryMatches.Count), Mathf.Min(Constants.MaximumCompulsoryMatches, ballsWithCompulsoryMatches.Count) + 1);

        for (int i = 0; i < compulsoryMatchesCount; i++)
        {

            BallInfo currentBallForMatch = ballsWithCompulsoryMatches[Random.Range(0, ballsWithCompulsoryMatches.Count)];
            string typeOfCurrentBallForMatch = matchType[currentBallForMatch];
            GameObject newBall = InstantiateNewBallOfType(typeOfCurrentBallForMatch, currentBallForMatch);
            ballsWithCompulsoryMatches.Remove(currentBallForMatch);
            emptyItems = emptyItems.Where(item => item != currentBallForMatch).ToList();
            balls[currentBallForMatch.Column, currentBallForMatch.Row] = newBall;
            newBallInfo.AddBall(newBall);
        }
        for (int i = 0; i < ballsWithCompulsoryMatches.Count; i++)
        {
            randomizingBag.RemoveItemFromBag(matchType[ballsWithCompulsoryMatches[i]]);
            string randomBallType = GetRandomBallType(randomizingBag);
            GameObject newBall = InstantiateNewBallOfType(randomBallType, ballsWithCompulsoryMatches[i]);
            balls[ballsWithCompulsoryMatches[i].Column, ballsWithCompulsoryMatches[i].Row] = newBall;
            emptyItems = emptyItems.Where(item => item != ballsWithCompulsoryMatches[i]).ToList();
            newBallInfo.AddBall(newBall);

        }
        foreach (var item in emptyItems)
        {
            string randomBallType = GetRandomBallType(randomizingBag);
            GameObject newBall = InstantiateNewBallOfType(randomBallType, item);
            balls[item.Column, item.Row] = newBall;
            newBallInfo.AddBall(newBall);
        }
        return newBallInfo;
    }
    private CreatedBallsInfo CreateNewBalls(List<BallInfo> emptyBalls)
    {

        CreatedBallsInfo newBallInfo = new CreatedBallsInfo();

        var emptyItems = emptyBalls;

        foreach (var item in emptyItems)
        {
            if(balls[item.Column, item.Row] == null)
            {
                if (balls[item.Column, item.Row] == null)
                {
                    string randomBallType = GetRandomBallType(randomizingBag);
                    GameObject newBall = InstantiateNewBallOfType(randomBallType, item);

                    balls[item.Column, item.Row] = newBall;
                    newBallInfo.AddBall(newBall);
                }
            }
            
        }
        return newBallInfo;
    }

    private IEnumerator FillBallsFromBag(RandomizingBag<string> bag, List<BallInfo> emptyBallsToFill)
    {
        CreatedBallsInfo newBallInfo = new CreatedBallsInfo();
        var emptyItems = emptyBallsToFill;

        foreach (var item in emptyItems)
        {
            string randomBallType = bag.GetRandomItem();
            bag.RemoveItemFromBag(randomBallType);
            GameObject newBall = InstantiateNewBallOfType(randomBallType, item);

            balls[item.Column, item.Row] = newBall;
            newBallInfo.AddBall(newBall);
        }

        foreach (var item in newBallInfo.CreatedBalls)
        {
            item.GetComponent<SpriteRenderer>().color = new Color(0.1254902f, 0.1411765f, 0.1764706f);
        }
        MoveAndAnimate(newBallInfo.CreatedBalls, Constants.appearanceOffset, Constants.MoveAnimationDuration);

        ChangeTone(newBallInfo.CreatedBalls, 0f, Constants.MoveAnimationDuration);
        yield return new WaitForSeconds(Constants.MoveAnimationDuration);
    }
    #endregion
    #endregion
    #region Для разных режимов(скилов)
    private bool[,] waveAnimationProgress;
    private List<GameObject>[] circles;

    private IEnumerator DeleteThreeBall(int column, int row)
    {

        RandomizingBag<string> bagForTrippleBallSkill = new RandomizingBag<string>(GetTypesList(BallsPrefabs));

        for (int i = 0; i < 3; i++)
        {

            bagForTrippleBallSkill.RemoveItemFromBag(balls[column, row].GetComponent<Ball>().Type);

            List<GameObject> deletingBalls = new List<GameObject>();
            List<BallInfo> deletingBallsInfo = new List<BallInfo>();
            deletingBalls.Add(balls[column, row]);
            deletingBallsInfo.Add(new BallInfo() { Column = column, Row = row });
            RemoveFromScene(deletingBalls);

            foreach(var item in deletingBallsInfo)
            {
                blockedBalls[item.Column, item.Row] = true;
            }
            yield return StartCoroutine(FillBallsFromBag(bagForTrippleBallSkill, deletingBallsInfo));
            foreach (var item in deletingBallsInfo)
            {
                blockedBalls[item.Column, item.Row] = false ;
            }
            yield return StartCoroutine(ClearMathces(CreateNewBalls, new List<BallInfo>() { new BallInfo { Column = column, Row = row } }));

        }
        state = GameState.None;


    }
    public IEnumerator StartWaveAnimation(int column, int row)
    {
        state = GameState.None;
        //animatingWave = true;
        string[,] newTypes = new string[Constants.Columns, Constants.Rows];
        randomizingBag = new RandomizingBag<string>(GetTypesList(BallsPrefabs));
        for (int column1 = 0; column1 < Constants.Columns; column1++)
        {
            for (int row1 = 0; row1 < Constants.Rows; row1++)
            {
                randomizingBag.RemoveItemFromBag(balls[column1, row1].GetComponent<Ball>().Type);
                string newSprite = randomizingBag.TakeRandomItem();
                newTypes[column1, row1] = newSprite;
            }
        }


        waveAnimationProgress = new bool[Constants.Columns, Constants.Rows];
        int circlesCount = 15;
        circles = new List<GameObject>[circlesCount];
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i] = new List<GameObject>();
        }
        StartFirstWaveSteps(column, row);
        for (int circleNumber = 3; circleNumber < circlesCount; circleNumber++)
        {
            for (int column1 = 0; column1 < Constants.Columns; column1++)
            {
                for (int row1 = 0; row1 < Constants.Rows; row1++)
                {
                    if (column1 >= 0 && row1 >= 0 && column1 < Constants.Columns && row1 < Constants.Rows && ((column1 < Constants.Columns - 1 && waveAnimationProgress[column1 + 1, row1]) || (column1 > 0 && waveAnimationProgress[column1 - 1, row1]) || (row1 < Constants.Rows - 1 && waveAnimationProgress[column1, row1 + 1]) || (row1 > 0 && waveAnimationProgress[column1, row1 - 1])) && (!waveAnimationProgress[column1, row1]))
                    {
                        circles[circleNumber].Add(balls[column1, row1]);
                    }
                }
            }

            for (int i = 0; i < circles[circleNumber].Count; i++)
            {
                waveAnimationProgress[circles[circleNumber][i].GetComponent<Ball>().Column, circles[circleNumber][i].GetComponent<Ball>().Row] = true;
            }
        }

        for (int circleNumber = 0; circleNumber < circlesCount; circleNumber++)
        {
            bool listNonblocked = true;
            foreach (var item in circles[circleNumber])     
            {
                if (blockedBalls[item.GetComponent<Ball>().Column, item.GetComponent<Ball>().Row])
                {
                    listNonblocked = false;
                    break;
                }
            }
            
            if (listNonblocked)
            {
                for (int i = 0; i < circles[circleNumber].Count; i++)
                {
  
                    balls[circles[circleNumber][i].GetComponent<Ball>().Column, circles[circleNumber][i].GetComponent<Ball>().Row].GetComponent<Ball>().Type  = newTypes[circles[circleNumber][i].GetComponent<Ball>().Column, circles[circleNumber][i].GetComponent<Ball>().Row];
                    balls[circles[circleNumber][i].GetComponent<Ball>().Column, circles[circleNumber][i].GetComponent<Ball>().Row].GetComponent<SpriteRenderer>().sprite = GetBallOfType(newTypes[circles[circleNumber][i].GetComponent<Ball>().Column, circles[circleNumber][i].GetComponent<Ball>().Row]).GetComponent<SpriteRenderer>().sprite;
                }
                StartCoroutine(GurglingEffect(circles[circleNumber], Constants.gurglingEffectDuration, Constants.gurglingOffset));
                yield return new WaitForSeconds(Constants.gurglingEffectDuration);
            }
        }

        var allBalls = GetAllBalls();
        Debug.Log(" wave balls count: " + allBalls.Count);
        yield return StartCoroutine(ClearMathces(CreateNewBalls, GetAllBalls()));
        state = GameState.None;
    }
    private IEnumerator GurglingEffect(List<GameObject> obj, float time, Vector3 gurglingOffset)
    {

        
            foreach (var item in obj)
            {
                blockedBalls[item.GetComponent<Ball>().Column, item.GetComponent<Ball>().Row] = true;
            }
            ChangeTone(obj, +25f, time);
            MoveAndAnimate(obj, gurglingOffset, time);
            yield return new WaitForSeconds(time);
            ChangeTone(obj, 0f, time);
            MoveAndAnimate(obj, -gurglingOffset, time);
            yield return new WaitForSeconds(time);
            ChangeTone(obj, -25f, time);
            MoveAndAnimate(obj, -gurglingOffset, time);
            yield return new WaitForSeconds(time);
            ChangeTone(obj, 0, time);
            MoveAndAnimate(obj, gurglingOffset, time);
            yield return new WaitForSeconds(time);
            foreach (var item in obj)
            {
                blockedBalls[item.GetComponent<Ball>().Column, item.GetComponent<Ball>().Row] = false;
            }
        
        
    }
    private void StartFirstWaveSteps(int column, int row)
    {
        waveAnimationProgress[column, row] = true;
        circles[0].Add(balls[column, row]);

        if (row < Constants.Rows - 1)
        {
            waveAnimationProgress[column, row + 1] = true;
            circles[1].Add(balls[column, row + 1]);
        }

        if (row > 0)
        {
            waveAnimationProgress[column, row - 1] = true;
            circles[1].Add(balls[column, row - 1]);
        }
        if (column < Constants.Columns - 1)
        {
            waveAnimationProgress[column + 1, row] = true;
            circles[1].Add(balls[column + 1, row]);
        }
        if (column > 0)
        {
            waveAnimationProgress[column - 1, row] = true;
            circles[1].Add(balls[column - 1, row]);
        }

        if (row < Constants.Rows - 1 && column < Constants.Columns - 1)
        {
            waveAnimationProgress[column + 1, row + 1] = true;
            circles[2].Add(balls[column + 1, row + 1]);
        }
        if (row > 0 && column > 0)
        {
            waveAnimationProgress[column - 1, row - 1] = true;
            circles[2].Add(balls[column - 1, row - 1]);
        }
        if (column < Constants.Columns - 1 && row > 0)
        {
            waveAnimationProgress[column + 1, row - 1] = true;
            circles[2].Add(balls[column + 1, row - 1]);
        }
        if (row < Constants.Rows - 1 && column > 0)
        {
            waveAnimationProgress[column - 1, row + 1] = true;
            circles[2].Add(balls[column - 1, row + 1]);
        }
    }
    private List<BallInfo> DeleteAllBallsOfType(string deletingType)
    {
        List<BallInfo> deletingBalls = new List<BallInfo>();
        List<GameObject> ballsToDelete = new List<GameObject>();
        for (int column = 0; column < Constants.Columns; column++)
        {
            for (int row = 0; row < Constants.Rows; row++)
            {
                if (!blockedBalls[column, row] && balls[column, row].GetComponent<Ball>().Type == deletingType)
                {
                    deletingBalls.Add(new BallInfo() { Column = column, Row = row });
                    ballsToDelete.Add(balls[column, row]);
                }
            }


        }
        RemoveFromScene(ballsToDelete);
        return deletingBalls;
    }
    private IEnumerator FillEmptyBallsAndClearMathces(Func<List<BallInfo>, CreatedBallsInfo> CreateNewBalls, List<BallInfo> emptyBallsToFill)
    {
        
        foreach(var item in emptyBallsToFill)
        {
            
            blockedBalls[item.Column, item.Row] = true;
           
           
        }
        yield return new WaitForSeconds(Constants.DelayBeforeAnimation);
        yield return StartCoroutine(FillEmptyBalls(CreateNewBalls, emptyBallsToFill));
        foreach (var item in emptyBallsToFill)
        {
            blockedBalls[item.Column, item.Row] = false;
        }
        yield return new WaitForSeconds(Constants.DelayBeforeAnimation);
        yield return StartCoroutine(ClearMathces(this.CreateNewBalls, emptyBallsToFill));
    }
    #endregion
    #region анимации

    private IEnumerator GradientTone(GameObject obj, float percentage, float changingTime)
    {
        ChangeTone(obj, percentage, changingTime / 2);
        yield return new WaitForSeconds(changingTime / 2);
        ChangeTone(obj, 0f, changingTime / 2);
        yield return new WaitForSeconds(changingTime / 2);

    }
    private IEnumerator SwapBallsPositions(GameObject obj1, GameObject obj2)
    {
        IncOrderInLayer(obj1);
        IncOrderInLayer(obj1);
        StartCoroutine(GradientTone(obj2, -30, Constants.SwappingBallsAnimationDuration));
        ChangeTone(obj1, 0f, Constants.SwappingBallsAnimationDuration);
        obj1.transform.DOMove(obj2.transform.position, Constants.SwappingBallsAnimationDuration);
        obj2.transform.DOMove(obj1.transform.position, Constants.SwappingBallsAnimationDuration);
        yield return new WaitForSeconds(Constants.SwappingBallsAnimationDuration);
        obj1.GetComponent<SpriteRenderer>().sortingOrder = GetBallOfType(obj1.GetComponent<Ball>().Type).GetComponent<SpriteRenderer>().sortingOrder + Constants.Rows - obj1.GetComponent<Ball>().Row - 1;
        obj2.GetComponent<SpriteRenderer>().sortingOrder = GetBallOfType(obj2.GetComponent<Ball>().Type).GetComponent<SpriteRenderer>().sortingOrder + Constants.Rows - obj2.GetComponent<Ball>().Row - 1;

    }
    //анимирование движение списка объектов
    private void MoveAndAnimate(IEnumerable<GameObject> balls, Vector2 offset, float movementTime)
    {
        foreach (var item in balls)
        {
            Vector2 currentPosition = item.transform.position;
            item.transform.DOMove(currentPosition + offset, movementTime);
        }
    }
    //удаление со сцены объекта
    private void ChangeTone(IEnumerable<GameObject> balls, float percentage, float colorChangingTime)
    {
        foreach (var item in balls)
        {
            item.GetComponent<SpriteRenderer>().DOColor(new Color(0.5f + (percentage / 200), 0.5f + (percentage / 200), 0.5f + (percentage / 200)), colorChangingTime);
        }
    }
    private void ChangeTone(GameObject ball, float percentage, float colorChangingTime)
    {
        ball.GetComponent<SpriteRenderer>().DOColor(new Color(0.5f + (percentage / 200), 0.5f + (percentage / 200), 0.5f + (percentage / 200)), colorChangingTime);
    }

    #endregion
    #region получение случайного шара и удаления
    private string GetRandomBallType(RandomizingBag<string> bag)
    {
        return bag.TakeRandomItem();
    }
    #endregion
    #region Поднятие и опускание объектов в слоях
    private void IncOrderInLayer(IEnumerable<GameObject> objects)
    {
        foreach (var item in objects)
        {
            item.GetComponent<SpriteRenderer>().sortingOrder++;
        }
    }
    private void DecOrderInLayer(IEnumerable<GameObject> objects)
    {
        foreach (var item in objects)
        {
            item.GetComponent<SpriteRenderer>().sortingOrder--;
        }
    }
    private void IncOrderInLayer(GameObject obj)
    {
        obj.GetComponent<SpriteRenderer>().sortingOrder++;
    }
    private void DecOrderInLayer(GameObject obj)
    {
        obj.GetComponent<SpriteRenderer>().sortingOrder--;
    }
    #endregion
    private List<GameObject> hitGoTimeline;
    private List<GameObject> hitGo2Timeline;
    #region выделение и девыделение шаров

    private void Select()
    {
        if (hitGoTimeline.Last() != null)
        {
            ChangeTone(hitGoTimeline.Last(), 30f, Constants.SwappingBallsAnimationDuration);
            state = GameState.SelectionStarted;

        }

    }
    private void Deselect()
    {
        if (hitGoTimeline.Last() != null)
        {
            ChangeTone(hitGoTimeline.Last(), 0f, Constants.SwappingBallsAnimationDuration);
            state = GameState.None;
        }

    }
    private void Reselect()
    {
        if (hitGoTimeline.Last() != null)
        {
            Deselect();
            Find1stBallOnClicking();
            Process1stClick();
        }
    }
    #endregion
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (state == GameState.None)
            {
                Find1stBallOnClicking();
                if (hitGoTimeline.Count != 0 && hitGoTimeline.Last() != null && !blockedBalls[hitGoTimeline.Last().GetComponent<Ball>().Column, hitGoTimeline.Last().GetComponent<Ball>().Row])
                {

                    Process1stClick();

                }

            }
            else
            if (state == GameState.SelectionStarted)
            {
                if (hitGoTimeline.Count == 0 || hitGoTimeline.Last() == null)
                {
                    state = GameState.None;
                }
                else
                {
                    Find2stBallOnClicking();
                    if (hitGoTimeline.Last() != null && hitGo2Timeline.Last() != null && hitGoTimeline.Last() != hitGo2Timeline.Last() && !blockedBalls[hitGo2Timeline.Last().GetComponent<Ball>().Column, hitGo2Timeline.Last().GetComponent<Ball>().Row])
                    {

                        if (!Utilities.AreVerticalOrHorizontalNeighbors(hitGoTimeline.Last().GetComponent<Ball>(),
                            hitGo2Timeline.Last().GetComponent<Ball>()))
                        {
                            Reselect();
                        }
                        else
                        {
                            Deselect();
                            StartCoroutine(SwapBallsAndClearMatches());

                        }
                    }
                    else
                    {
                        Deselect();
                    }
                }


            }
        }
    }

    private IEnumerator SwapBallsAndClearMatches()
    {
        GameObject hitGo1 = hitGoTimeline.Last();
        GameObject hitGo2 = hitGo2Timeline.Last();


        balls.Swap(hitGo1, hitGo2);
        
        List<GameObject> swappedBalls = new List<GameObject>() { hitGo1, hitGo2 };
        List<GameObject> totalMatches = new List<GameObject>();

        for (int i = 0; i < swappedBalls.Count; i++)
        {
            List<GameObject> currentMatch = new List<GameObject>();
            if (!blockedBalls[swappedBalls[i].GetComponent<Ball>().Column, swappedBalls[i].GetComponent<Ball>().Row] && balls[swappedBalls[i].GetComponent<Ball>().Column, swappedBalls[i].GetComponent<Ball>().Row] != null)
            {
                currentMatch.AddRange(balls.GetMatches(new List<GameObject>() { swappedBalls[i] }));
                for (int j = 0; j < currentMatch.Count; j++)
                {
                    if (blockedBalls[currentMatch.ToList()[j].GetComponent<Ball>().Column, currentMatch.ToList()[j].GetComponent<Ball>().Row])
                    {
                        currentMatch.Clear();
                        break;
                    }
                }

                totalMatches.AddRange(currentMatch);
            }

        }
        
        if (mode == GameMode.DeletingBallsFFD && !UIManager.instance.isSkillWithTime)
        {
            StartCoroutine(UIManager.instance.DisableFFDMode(5));
            List<BallInfo> allNonblockedBalls = GetAllBalls();
            for (int i = 0; i < allNonblockedBalls.Count; i++)
            {
                List<GameObject> currentMatch = new List<GameObject>();
                if (!blockedBalls[allNonblockedBalls[i].Column, allNonblockedBalls[i].Row] && balls[allNonblockedBalls[i].Column, allNonblockedBalls[i].Row] != null) 
                {
                    currentMatch.AddRange(balls.GetMatchesFFD(new List<GameObject>() { balls[allNonblockedBalls[i].Column, allNonblockedBalls[i].Row] }));
                    for (int j = 0; j < currentMatch.Count; j++)
                    {
                        if (blockedBalls[currentMatch.ToList()[j].GetComponent<Ball>().Column, currentMatch.ToList()[j].GetComponent<Ball>().Row])
                        {
                            currentMatch.Clear();
                            break;
                        }
                    }
                    totalMatches.AddRange(currentMatch);
                }
                

            }
        }

        totalMatches = totalMatches.Distinct().ToList();
        foreach (var item in totalMatches)
        {
            blockedBalls[item.GetComponent<Ball>().Column, item.GetComponent<Ball>().Row] = true;

        }


        blockedBalls[hitGo1.GetComponent<Ball>().Column, hitGo1.GetComponent<Ball>().Row] = true;
        blockedBalls[hitGo2.GetComponent<Ball>().Column, hitGo2.GetComponent<Ball>().Row] = true;
        yield return StartCoroutine(SwapBallsPositions(hitGo1, hitGo2));

        if (totalMatches.Count < Constants.MinimumMatches)
        {
            ChangeTone(hitGo1, 30f, Constants.SwappingBallsAnimationDuration);

            yield return new WaitForSeconds(Constants.SwappingBallsAnimationDuration / 2f);
            balls.Swap(hitGo1, hitGo2);
            yield return StartCoroutine(SwapBallsPositions(hitGo1, hitGo2));
            blockedBalls[hitGo1.GetComponent<Ball>().Column, hitGo1.GetComponent<Ball>().Row] = false;
            blockedBalls[hitGo2.GetComponent<Ball>().Column, hitGo2.GetComponent<Ball>().Row] = false;

        }
        else
        {
            blockedBalls[hitGo1.GetComponent<Ball>().Column, hitGo1.GetComponent<Ball>().Row] = false;
            blockedBalls[hitGo2.GetComponent<Ball>().Column, hitGo2.GetComponent<Ball>().Row] = false;
            while (totalMatches.Count() >= Constants.MinimumMatches)
            {
                List<BallInfo> totalMathcesInfo = new List<BallInfo>();
                foreach (var item in totalMatches)
                {
                    totalMathcesInfo.Add(new BallInfo() { Column = item.GetComponent<Ball>().Column, Row = item.GetComponent<Ball>().Row });

                }

                yield return new WaitForSeconds(Constants.DelayBeforeAnimation);

                RemoveFromScene(totalMatches);
                totalMatches.Clear();


                yield return new WaitForSeconds(Constants.DelayBeforeAnimation);

                yield return StartCoroutine(FillEmptyBalls(CreateNewBalls, totalMathcesInfo));

                foreach (var item in totalMathcesInfo)
                {
                    blockedBalls[item.Column, item.Row] = false;
                }
                for (int i = 0; i < totalMathcesInfo.Count; i++)
                {
                    List<GameObject> currentMatch = new List<GameObject>();
                    if (!blockedBalls[totalMathcesInfo[i].Column, totalMathcesInfo[i].Row] && balls[totalMathcesInfo[i].Column, totalMathcesInfo[i].Row] != null)
                    {
                        currentMatch.AddRange(balls.GetMatches(new List<GameObject>() { balls[totalMathcesInfo[i].Column, totalMathcesInfo[i].Row] }));
                        for (int j = 0; j < currentMatch.Count; j++)
                        {
                            if (blockedBalls[currentMatch.ToList()[j].GetComponent<Ball>().Column, currentMatch.ToList()[j].GetComponent<Ball>().Row])
                            {
                                currentMatch.Clear();
                                break;
                            }
                        }

                        totalMatches.AddRange(currentMatch);
                    }

                }
                
                if (mode == GameMode.DeletingBallsFFD)
                {
                    for (int i = 0; i < totalMathcesInfo.Count; i++)
                    {
                        List<GameObject> currentMatch = new List<GameObject>();
                        if (!blockedBalls[totalMathcesInfo[i].Column, totalMathcesInfo[i].Row] && balls[totalMathcesInfo[i].Column, totalMathcesInfo[i].Row] != null)
                        {
                            currentMatch.AddRange(balls.GetMatchesFFD(new List<GameObject>() { balls[totalMathcesInfo[i].Column, totalMathcesInfo[i].Row] }));
                            for (int j = 0; j < currentMatch.Count; j++)
                            {
                                if (blockedBalls[currentMatch.ToList()[j].GetComponent<Ball>().Column, currentMatch.ToList()[j].GetComponent<Ball>().Row])
                                {
                                    currentMatch.Clear();
                                    break;
                                }
                            }
                            totalMatches.AddRange(currentMatch);
                        }
                    }

                }
                foreach (var item in totalMatches)
                {
                    blockedBalls[item.GetComponent<Ball>().Column, item.GetComponent<Ball>().Row] = true;

                }
                totalMatches = totalMatches.Distinct().ToList();
            }
        }



        //if (totalMatches.Count() < Constants.MinimumMatches)



        //}else
        //{
        //    yield return StartCoroutine(ClearMathces(CreateNewBalls));



    }


    private IEnumerator ClearMathces(Func<List<BallInfo>, CreatedBallsInfo> CreateNewBalls, List<BallInfo> ballsToStartClearingWith)
    {
        List<GameObject> totalMatches = new List<GameObject>();

        for (int i = 0; i < ballsToStartClearingWith.Count; i++)
        {
            List<GameObject> currentMatch = new List<GameObject>();
            if (balls[ballsToStartClearingWith[i].Column, ballsToStartClearingWith[i].Row] != null &&  !blockedBalls[ballsToStartClearingWith[i].Column, ballsToStartClearingWith[i].Row] && balls[ballsToStartClearingWith[i].Column, ballsToStartClearingWith[i].Row] != null)
            {

                for (int j = 0; j < currentMatch.Count; j++)
                {
                    if (blockedBalls[currentMatch.ToList()[j].GetComponent<Ball>().Column, currentMatch.ToList()[j].GetComponent<Ball>().Row] && balls[currentMatch.ToList()[j].GetComponent<Ball>().Column, currentMatch.ToList()[j].GetComponent<Ball>().Row] != null)
                    {
                        currentMatch.RemoveAt(j);
                        j = -1;
                    }
                }
                totalMatches.AddRange(currentMatch);
            }
        }
        if (mode == GameMode.DeletingBallsFFD)
        {
            var allNonblockedBalls = GetAllBalls();
            for (int i = 0; i < allNonblockedBalls.Count; i++)
            {
                List<GameObject> currentMatch = new List<GameObject>();
                if (!blockedBalls[allNonblockedBalls[i].Column, allNonblockedBalls[i].Row] && balls[allNonblockedBalls[i].Column, allNonblockedBalls[i].Row] != null)
                {
                    currentMatch.AddRange(balls.GetMatchesFFD(new List<GameObject>() { balls[allNonblockedBalls[i].Column, allNonblockedBalls[i].Row] }));
                    for (int j = 0; j < currentMatch.Count; j++)
                    {
                        if (blockedBalls[currentMatch.ToList()[j].GetComponent<Ball>().Column, currentMatch.ToList()[j].GetComponent<Ball>().Row])
                        {
                            currentMatch.Clear();
                            break;
                        }
                    }
                }
                totalMatches.AddRange(currentMatch);
            }

        }

        while (totalMatches.Count() >= Constants.MinimumMatches)
        {
            List<BallInfo> totalMathcesInfo = new List<BallInfo>();
            foreach (var item in totalMatches)
            {
                totalMathcesInfo.Add(new BallInfo() { Column = item.GetComponent<Ball>().Column, Row = item.GetComponent<Ball>().Row });
                blockedBalls[item.GetComponent<Ball>().Column, item.GetComponent<Ball>().Row] = true;

            }

            yield return new WaitForSeconds(Constants.DelayBeforeAnimation);
            foreach (var item in totalMatches)

               
            RemoveFromScene(totalMatches);
            var columns = totalMatches.Select(go => go.GetComponent<Ball>().Column).Distinct();

            yield return new WaitForSeconds(Constants.DelayBeforeAnimation);

            yield return StartCoroutine(FillEmptyBalls(CreateNewBalls, totalMathcesInfo));

            foreach (var item in totalMathcesInfo)
            {
                blockedBalls[item.Column, item.Row] = false;

            }
            totalMatches.Clear();

            for (int i = 0; i < totalMathcesInfo.Count; i++)
            {
                List<GameObject> currentMatch = new List<GameObject>();
                if (!blockedBalls[totalMathcesInfo[i].Column, totalMathcesInfo[i].Row] && balls[totalMathcesInfo[i].Column, totalMathcesInfo[i].Row] != null)
                {
                    currentMatch.AddRange(balls.GetMatches(new List<GameObject>() { balls[totalMathcesInfo[i].Column, totalMathcesInfo[i].Row] }));
                    for (int j = 0; j < currentMatch.Count; j++)
                    {
                        if (blockedBalls[currentMatch.ToList()[j].GetComponent<Ball>().Column, currentMatch.ToList()[j].GetComponent<Ball>().Row])
                        {
                            currentMatch.Clear();
                            break;
                        }
                    }

                    totalMatches.AddRange(currentMatch);
                }
            }
            if (mode == GameMode.DeletingBallsFFD)
            {
                for (int i = 0; i < totalMathcesInfo.Count; i++)
                {
                    List<GameObject> currentMatch = new List<GameObject>();
                    if (!blockedBalls[totalMathcesInfo[i].Column, totalMathcesInfo[i].Row] && balls[totalMathcesInfo[i].Column, totalMathcesInfo[i].Row] != null)
                    {
                        currentMatch.AddRange(balls.GetMatchesFFD(new List<GameObject>() { balls[totalMathcesInfo[i].Column, totalMathcesInfo[i].Row] }));
                        for (int j = 0; j < currentMatch.Count; j++)
                        {
                            if (blockedBalls[currentMatch.ToList()[j].GetComponent<Ball>().Column, currentMatch.ToList()[j].GetComponent<Ball>().Row])
                            {
                                currentMatch.Clear();
                                break;
                            }
                        }

                        totalMatches.AddRange(currentMatch);
                    }

                }

            }
        }
        totalMatches = totalMatches.Distinct().ToList();
        makingAreaVisible = false;
        removingArea = false;
    }
    public IEnumerator FillEmptyBalls(Func<List<BallInfo>, CreatedBallsInfo> CreateNewBalls, List<BallInfo> emptyBallsToFill)
    {

        var newBallsInfo = CreateNewBalls(emptyBallsToFill);
        foreach (var item in newBallsInfo.CreatedBalls)
        {
            item.GetComponent<SpriteRenderer>().color = new Color(0.1254902f, 0.1411765f, 0.1764706f);
      
        }
        MoveAndAnimate(newBallsInfo.CreatedBalls, Constants.appearanceOffset, Constants.MoveAnimationDuration);

        ChangeTone(newBallsInfo.CreatedBalls, 0f, Constants.MoveAnimationDuration);
        yield return new WaitForSeconds(Constants.MoveAnimationDuration);
    }
}