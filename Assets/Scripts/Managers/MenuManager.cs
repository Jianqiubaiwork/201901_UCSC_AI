using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
	public static MenuManager Instance { get; set; }
	public int GAME_MODE { get; set; }

	public Toggle Human;
	public Toggle AI;

	// Start is called before the first frame update
	void Awake()
	{
		GAME_MODE = 1;
		Instance = this;
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void ActiveToggle()
	{
		if (Human.isOn)
		{
			GAME_MODE = 1;
		}
		else if (AI.isOn)
		{
			GAME_MODE = 2;
		}
	}

	public void OnSubmit()
	{
		ActiveToggle();
		SceneManager.LoadScene("Game");
	}
}