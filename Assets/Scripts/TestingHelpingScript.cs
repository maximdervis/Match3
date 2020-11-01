using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TestingHelpingScript : MonoBehaviour
{
    [SerializeField]
    public InputField field;

    private float moveSpeed, swapSpeed;

    void Start()
    {
        field.text = "1";
        moveSpeed = Constants.MoveAnimationDuration;
        swapSpeed = Constants.SwappingBallsAnimationDuration;
    }

    // Update is called once per frame
    void Update()
    {
        Constants.MoveAnimationDuration = moveSpeed / Convert.ToInt32(field.text);
        Constants.SwappingBallsAnimationDuration = swapSpeed / Convert.ToInt32(field.text);
    }
}
