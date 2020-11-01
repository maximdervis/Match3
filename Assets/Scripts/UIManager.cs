using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    public bool isSkillWithTime = false;

    public static UIManager instance;

    public GameObject switchWaveModeButton;
    public GameObject switch45DegreesModeButton;
    public GameObject switchColorDeletingModeButton;
    public GameObject switchDeletongHorLineModeButton;
    public GameObject switchDeletingVertLineModeButton;
    public GameObject switchTrippleBallDeletingButtom;

    public Text timer;

    private void Start()
    {
        instance = GetComponent<UIManager>();
    }

    public IEnumerator Timer(int timeSeconds)
    {
        string startText = timer.text;
        timer.text = Convert.ToString(timeSeconds);
        for (int i = 0; i < timeSeconds; i++)
        {
            yield return new WaitForSeconds(1f);
            timer.text = Convert.ToString(Convert.ToInt32(timer.text) - 1);
        }
        timer.text = startText;
    }

    public IEnumerator DisableFFDMode(int timeSeconds)
    {
        isSkillWithTime = true;
        StartCoroutine(Timer(timeSeconds));
        yield return new WaitForSeconds(timeSeconds);
        BallsManager.mode = GameMode.None;
        switch45DegreesModeButton.GetComponent<Image>().color = Color.white;
        isSkillWithTime = false;
    }


    public void SwitchTrippleBallDeletingMode()
    {
        if (BallsManager.mode == GameMode.None && BallsManager.state == GameState.None)
        {
            BallsManager.mode = GameMode.TrippleBallDeleting;
            switchTrippleBallDeletingButtom.GetComponent<Image>().color = Color.red;
        }
        else if (BallsManager.mode == GameMode.TrippleBallDeleting  )
        {
            BallsManager.mode = GameMode.None;
            switchTrippleBallDeletingButtom.GetComponent<Image>().color = Color.white;
        }

    }

    public void SwitchWaveMode()
    {
        if (BallsManager.mode == GameMode.None && BallsManager.state == GameState.None)
        {
            BallsManager.mode = GameMode.ChangingWave;
            switchWaveModeButton.GetComponent<Image>().color = Color.red;
        }
        else if (BallsManager.mode == GameMode.ChangingWave)
        {
            BallsManager.mode = GameMode.None;
            switchWaveModeButton.GetComponent<Image>().color = Color.white;
        }

    }

    public void Switch45DegreesMode()
    {
        if (BallsManager.mode == GameMode.None && BallsManager.state == GameState.None)
        {
            BallsManager.mode = GameMode.DeletingBallsFFD;
            switch45DegreesModeButton.GetComponent<Image>().color = Color.red;

        }
        else if(BallsManager.mode == GameMode.DeletingBallsFFD && !isSkillWithTime && BallsManager.state == GameState.None)
        {
            BallsManager.mode = GameMode.None;
            switch45DegreesModeButton.GetComponent<Image>().color = Color.white;
        }
    }

    public void SwitchColorDeletingMode()
    {
        if (BallsManager.mode == GameMode.None && BallsManager.state == GameState.None)
        {
            BallsManager.mode = GameMode.TypeDeleting;
            switchColorDeletingModeButton.GetComponent<Image>().color = Color.red;
        }
        else if(BallsManager.mode == GameMode.TypeDeleting)
        {
            BallsManager.mode = GameMode.None;
            switchColorDeletingModeButton.GetComponent<Image>().color = Color.white;
        }
    }

    public void SwitchDeleteHorLineMode()
    {
        if (BallsManager.mode == GameMode.None && BallsManager.state == GameState.None)
        {
            BallsManager.mode = GameMode.HorizontalLineDeleting;
            switchDeletongHorLineModeButton.GetComponent<Image>().color = Color.red;
        }
        else if (BallsManager.mode == GameMode.HorizontalLineDeleting)
        {
            BallsManager.mode = GameMode.None;
            switchDeletongHorLineModeButton.GetComponent<Image>().color = Color.white;
        }
    }

    public void SwitchDeleteVertLineMode()
    {
        if (BallsManager.mode == GameMode.None && BallsManager.state == GameState.None)
        {
            BallsManager.mode = GameMode.VerticalLineDeleting;
            switchDeletingVertLineModeButton.GetComponent<Image>().color = Color.red;
        }
        else if (BallsManager.mode == GameMode.VerticalLineDeleting)
        {
            BallsManager.mode = GameMode.None;
            switchDeletingVertLineModeButton.GetComponent<Image>().color = Color.white;
        }
    }
}