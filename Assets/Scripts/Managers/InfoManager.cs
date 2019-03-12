using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfoManager : MonoBehaviour
{
	GameObject info;
	Text text;
	bool isShowingInformation = false;

	void Awake ()
	{
		text = GetComponent <Text> ();
		info = GameObject.FindWithTag("Info");
	}


	void Update ()
	{
		if (isShowingInformation)
		{
			text.text = Environment.Instance.information;
		}
		else
		{
			text.text = "";
		}
		if (Input.GetKeyDown (KeyCode.C))
		{
			if(!isShowingInformation)
			{
				isShowingInformation = true;
			}
			else
			{
				isShowingInformation = false;
			}
		}
	}
}
