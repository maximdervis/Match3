using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BallsArray
{

    private GameObject[,] balls = new GameObject[Constants.Columns, Constants.Rows];

    public GameObject this[int column, int row]
    {
        get
        {
            try
            {
                return balls[column, row];
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        set
        {
            balls[column, row] = value;
        }
    }

    public void Swap(GameObject g1, GameObject g2)
    {

        backupG1 = g1;
        backupG2 = g2;

        var g1Ball = g1.GetComponent<Ball>();
        var g2Ball = g2.GetComponent<Ball>();

        int g1Row = g1Ball.Row;
        int g1Column = g1Ball.Column;
        int g2Row = g2Ball.Row;
        int g2Column = g2Ball.Column;

        var temp = balls[g1Column, g1Row];
        balls[g1Column, g1Row] = balls[g2Column, g2Row];
        balls[g2Column, g2Row] = temp;

        Ball.SwapColumnRow(g1Ball, g2Ball);

    }

    public void UndoSwap()
    {
        if (backupG1 == null || backupG2 == null)
            throw new Exception("Backup is null");

        Swap(backupG1, backupG2);
    }

    private GameObject backupG1;
    private GameObject backupG2;

    public IEnumerable<GameObject> GetMatches(IEnumerable<GameObject> gos)
    {
        List<GameObject> matches = new List<GameObject>();
        foreach (var go in gos)
        {
            matches.AddRange(GetMatches(go).MatchedCandy);
        }
        return matches.Distinct();
    }
    public MatchesInfo GetMatches(GameObject go)
    {
        MatchesInfo matchesInfo = new MatchesInfo();

        var horizontalMatches = GetMatchesHorizontally(go);
        matchesInfo.AddObjectRange(horizontalMatches);

        var verticalMatches = GetMatchesVertically(go);
        matchesInfo.AddObjectRange(verticalMatches);

        return matchesInfo;
    }

    public IEnumerable<GameObject> GetMatchesFFD(IEnumerable<GameObject> gos)
    {
        List<GameObject> matches = new List<GameObject>();
        foreach (var go in gos)
        {
            matches.AddRange(GetMatchesFFD(go).MatchedCandy);
        }
        return matches.Distinct();
    }

    public MatchesInfo GetMatchesFFD(GameObject go)
    {
        MatchesInfo matchesInfo = new MatchesInfo();

        var FFDMatchesLeft = GetMatchesFFDLeft(go);
        matchesInfo.AddObjectRange(FFDMatchesLeft);

        var FFDMatchesRight = GetMatchesFFDRight(go);
        matchesInfo.AddObjectRange(FFDMatchesRight);

        return matchesInfo;
    }

    public IEnumerable<GameObject> GetEntireRow(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        int row = go.GetComponent<Ball>().Row;
        for (int column = 0; column < Constants.Columns; column++)
        {
            matches.Add(balls[column, row]);
        }
        return matches;
    }

    public IEnumerable<GameObject> GetEntireColumn(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        int column = go.GetComponent<Ball>().Column;
        for (int row = 0; row < Constants.Rows; row++)
        {
            matches.Add(balls[column, row]);
        }
        return matches;
    }

    private IEnumerable<GameObject> GetMatchesFFDRight(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        matches.Add(go);
        var ball = go.GetComponent<Ball>();

        if (ball.Column != 0 && ball.Row != 0)
            for (int column = ball.Column - 1, row = ball.Row - 1; column >= 0 && row >= 0; row--, column--)
            {
                if (balls[column, row].GetComponent<Ball>().IsSameType(ball))
                {
                    matches.Add(balls[column, row]);
                }
                else
                    break;
            }

        if (ball.Column != Constants.Columns - 1 && ball.Row != Constants.Rows - 1)
            for (int column = ball.Column + 1, row = ball.Row + 1; column < Constants.Columns && row < Constants.Rows; row++, column++)
            {
                if (balls[column, row].GetComponent<Ball>().IsSameType(ball))
                {
                    matches.Add(balls[column, row]);
                }
                else
                    break;
            }
        if (matches.Count < Constants.MinimumMatches)
            matches.Clear();

        return matches.Distinct();
    }

    private IEnumerable<GameObject> GetMatchesFFDLeft(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        matches.Add(go);
        var ball = go.GetComponent<Ball>();

        if (ball.Column != 0 && ball.Row != Constants.Rows - 1)
            for (int column = ball.Column - 1, row = ball.Row + 1; column >= 0 && row < Constants.Rows; row++, column--)
            {
                if (balls[column, row].GetComponent<Ball>().IsSameType(ball))
                {
                    matches.Add(balls[column, row]);
                }
                else
                    break;
            }

        if (ball.Column != Constants.Columns - 1 && ball.Row != 0)
            for (int column = ball.Column + 1, row = ball.Row - 1; column < Constants.Columns && row >= 0; row--, column++)
            {
                if (balls[column, row].GetComponent<Ball>().IsSameType(ball))
                {
                    matches.Add(balls[column, row]);
                }
                else
                    break;
            }

        if (matches.Count < Constants.MinimumMatches)
            matches.Clear();

        return matches.Distinct();
    }

    private IEnumerable<GameObject> GetMatchesHorizontally(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        matches.Add(go);
        var ball = go.GetComponent<Ball>();

        if (ball.Column != 0)
            for (int column = ball.Column - 1; column >= 0; column--)
            {
                if (balls[column, ball.Row].GetComponent<Ball>().IsSameType(ball))
                {
                    matches.Add(balls[column, ball.Row]);
                }
                else
                    break;
            }

        if (ball.Column != Constants.Columns - 1)
            for (int column = ball.Column + 1; column < Constants.Columns; column++)
            {
                if (balls[column, ball.Row].GetComponent<Ball>().IsSameType(ball))
                {
                    matches.Add(balls[column, ball.Row]);
                }
                else
                    break;
            }

        if (matches.Count < Constants.MinimumMatches)
            matches.Clear();

        return matches.Distinct();
    }

    private IEnumerable<GameObject> GetMatchesVertically(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        matches.Add(go);
        var ball = go.GetComponent<Ball>();
        if (ball.Row != 0)
            for (int row = ball.Row - 1; row >= 0; row--)
            {
                if (balls[ball.Column, row] != null &&
                    balls[ball.Column, row].GetComponent<Ball>().IsSameType(ball))
                {
                    matches.Add(balls[ball.Column, row]);
                }
                else
                    break;
            }

        if (ball.Row != Constants.Rows - 1)
            for (int row = ball.Row + 1; row < Constants.Rows; row++)
            {
                if (balls[ball.Column, row] != null &&
                    balls[ball.Column, row].GetComponent<Ball>().IsSameType(ball))
                {
                    matches.Add(balls[ball.Column, row]);
                }
                else
                    break;
            }


        if (matches.Count < Constants.MinimumMatches)
            matches.Clear();

        return matches.Distinct();
    }

    public void Remove(GameObject item)
    {
        balls[item.GetComponent<Ball>().Column, item.GetComponent<Ball>().Row] = null;
    }

    public IEnumerable<BallInfo> GetEmptyItems()
    {
        List<BallInfo> emptyItems = new List<BallInfo>();
        for (int column = 0; column < Constants.Columns; column++)
        {
            for (int row = 0; row < Constants.Rows; row++)
            {
                if (balls[column, row] == null)
                    emptyItems.Add(new BallInfo() { Column = column, Row = row });
            }
        }
        return emptyItems;
    }
}