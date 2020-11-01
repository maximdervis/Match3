using System.Numerics;
using Vector2 = UnityEngine.Vector2;

public static class Constants
{
    public static  int Rows = 8;
    public static  int Columns = 8;

    public static  int MinimumStartTypeCount = 12;
    public static  int MaximumStartTypeCount = 16;

    public static  float SwappingBallsAnimationDuration = 1f;
    public static  float MoveAnimationDuration = 3f;
    public static  float ExplosionDuration = 0.3f;
    public static  Vector2 gurglingOffset = new Vector2(0f, 0.1f);
    public static  float gurglingEffectDuration = 0.1f;
    public static  float DelayBeforeAnimation = 0.3f;
    public static  Vector2 appearanceOffset = new Vector2(0f, 1f);
    public static  int MinimumCompulsoryMatches = 1;
    public static  int MaximumCompulsoryMatches = 3;
    public static  Vector2 SwappingBallsOffset = new Vector2(0f, 0.1f);
    public static  Vector2 BottomRight = new Vector2(-4.2f, -3.9f);
    public static  Vector2 BallsSize = new Vector2(1.2f, 1.1f);


    public static  int MinimumMatches = 3;
}