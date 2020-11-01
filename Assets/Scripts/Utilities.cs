using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public static class Utilities
{
    //проверяет, являются ли шыры соседними
    public static bool AreVerticalOrHorizontalNeighbors(Ball b1, Ball b2)
    {
        return (b1.Column == b2.Column ||
                        b1.Row == b2.Row)
                        && Mathf.Abs(b1.Column - b2.Column) <= 1
                        && Mathf.Abs(b1.Row - b2.Row) <= 1;
    }
}