using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class CreatedBallsInfo
{
    private List<GameObject> NewBalls { get; set; }

    public IEnumerable<GameObject> CreatedBalls
    {
        get
        {
            return NewBalls.Distinct();
        }
    }

    public void AddBall(GameObject go)
    {
        if (!NewBalls.Contains(go))
            NewBalls.Add(go);
    }

    public CreatedBallsInfo()
    {
        NewBalls = new List<GameObject>();
    }
}