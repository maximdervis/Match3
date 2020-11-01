using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Ball : MonoBehaviour
{
    
    public int Column { get; set; }
    public int Row { get; set; }
    public string Type { get; set; }


    public bool IsSameType(Ball otherBall)
    {
        if (otherBall == null || !(otherBall is Ball))
            throw new ArgumentException("otherShape");

        return string.Compare(this.Type, (otherBall as Ball).Type) == 0;
    }

    public void Assign(string type, int column, int row)
    {

        if (string.IsNullOrEmpty(type))
            throw new ArgumentException("type");

        Column = column;
        Row = row;
        Type = type;
    }

    public static void SwapColumnRow(Ball a, Ball b)
    {
        int temp = a.Row;
        a.Row = b.Row;
        b.Row = temp;

        temp = a.Column;
        a.Column = b.Column;
        b.Column = temp;
    }
}