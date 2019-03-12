using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeManager : MonoBehaviour
{
	bool isPaused = false;
	Text text;

    void Awake ()
    {
        text = GetComponent <Text> ();
    }


    void Update ()
    {
		text.text = "Time: " + System.Math.Round(Environment.Instance.timer, 1);
		if (Input.GetKeyDown (KeyCode.Space))
		{
			if(!isPaused)
			{
				Time.timeScale = 0;
				isPaused = true;
			}
			else
			{
				Time.timeScale = 1;
				isPaused = false;
			}
		}
    }
}
