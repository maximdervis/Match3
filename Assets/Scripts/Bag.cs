using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomizingBag <T>
{
    public List<T> CurrentBag { get; set; }
    private List<T> StartBag { get; set; }

    public T TakeRandomItem()
    {
        if(CurrentBag.Count == 0)
        {
            throw new Exception("Bag is empty");
        }
        T takingItem = CurrentBag[Random.Range(0, CurrentBag.Count)];
        ProcessTakingItem(takingItem);
        return takingItem;
    }

    public void RemoveItemFromBag(T item)
    {
        CurrentBag.RemoveAll(delegate (T it) { return item.Equals(it); });
    }

    public T GetRandomItem()
    {
        if (CurrentBag.Count == 0)
        {
            throw new Exception("Bag is empty");
        }
        T takingItem = CurrentBag[Random.Range(0, CurrentBag.Count)];
        return takingItem;
    }

    private void ProcessTakingItem(T item)
    {
        RemoveItemFromBag(item);
        CurrentBag.AddRange(StartBag);
    }

    public RandomizingBag(List<T> startBag)
    {
        CurrentBag = new List<T>();
        StartBag = startBag;
        CurrentBag.AddRange(StartBag);
    }

}
