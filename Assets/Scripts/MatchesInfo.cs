﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class MatchesInfo
{
    private List<GameObject> matchedCandies;

    public IEnumerable<GameObject> MatchedCandy
    {
        get
        {
            return matchedCandies.Distinct();
        }
    }

    public void AddObject(GameObject go)
    {
        if (!matchedCandies.Contains(go))
            matchedCandies.Add(go);
    }

    public void AddObjectRange(IEnumerable<GameObject> gos)
    {
        foreach (var item in gos)
        {
            AddObject(item);
        }
    }

    public MatchesInfo()
    {
        matchedCandies = new List<GameObject>();
    }
}