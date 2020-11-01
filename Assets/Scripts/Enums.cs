using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum GameState
{
    None,
    SelectionStarted
}

public enum AnimatingType
{
    AnimatingMovement,
    AnimatingColor, 
    AnimatingWave, 
    None
}

public enum GameMode
{
    None,
    TrippleBallDeleting,
    TypeDeleting,
    ChangingWave,
    HorizontalLineDeleting, 
    VerticalLineDeleting,
    DeletingBallsFFD
}