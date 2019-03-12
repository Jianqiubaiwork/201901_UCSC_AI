using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;

public class Environment : MonoBehaviour
{
	// System Variables
	public static Environment Instance { get; set; }
	public MazeSpawner mazeSpawner;
	public float timer = 0.0f;
	static int episode = 0;
	const int MAX_EPISODE = 1000;
	const int TIME_CAP = 5000;
	int FRAME_GAP = 0;
	int frameNumber = 0;
	int frameGap = 0;

	// Agent Variables
	public PlayerHealth playerHealth;
	public PlayerMovement playerMovement;
	public PlayerShooting playerShooting;
	const int moveCost = 1;
	const int collisionCost = 1;
	const int explorationReward = 10;
	//Vector3 oldPosition;
	float MOVEMENT_STEP_SIZE = 5f;
	int x = 0;
	int y = 0;
	bool isCollision = false;
	bool isNewStateExplored = false;
	bool isChangingAction = true;

	// State Variables
	int N_STATES=0;
	int stateIndex=0;
	int nextStateIndex=0;
	List<int> exploredStates;

	//Action Variables
	const int ACTION_NUMBERS = 4;
	int actionIndex=-1;
	List<int> actionList = new List<int> {0,1,2,3};

	// Q-Learning Parameters
	static float[,] q_table;
	const float alpha = 1f; //learning rate
	const float gamma = 0f; //delay rate
	const float epsilon = 1f; //e-greedy

	// IO Variables
	public string information = "";
	string filename = "./qtable.txt";

	void Awake()
	{
		episode += 1;
		frameNumber=0;
		Initialize();
		exploredStates = new List<int>();
		IdentifyNextState();
		MOVEMENT_STEP_SIZE = (int)mazeSpawner.CellWidth;
		Instance = this;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (MenuManager.Instance.GAME_MODE == 2)
		{
			if (playerShooting.damageMade > 600 || timer > TIME_CAP)
			{
				SceneManager.LoadScene("Game");
			}

			if (episode <= MAX_EPISODE)
			{
				//CheckIsChangingAction();
				if (isChangingAction)
				{
					actionIndex = ChooseAction();
				}
				MakeAction();
				//Debug.Log("At frame " + frameNumber + " : ( " + actionIndex + " )");
				//moveCost+=1;
				Learning();
			}
			//else
			//{
			//	UnityEditor.EditorApplication.isPlaying = false;
			//}

			timer += Time.deltaTime;
			frameNumber +=1;
		}
	}



	void Initialize()
	{
		N_STATES = mazeSpawner.Rows * mazeSpawner.Columns;
		q_table = new float[N_STATES+1,ACTION_NUMBERS+1];
	}

	void IdentifyNextState()
	{
		//input: currentState, action
		//output: state

		Vector3 pos = playerMovement.playerRigidbody.position;
		x = (int)(pos.x/mazeSpawner.CellWidth) + 1;
		y = (int)(pos.z/mazeSpawner.CellWidth) + 1;
		nextStateIndex = (y-1)*mazeSpawner.Columns + x;
		//Debug.Log("next state is " + nextStateIndex);
		//Debug.Log(exploredStates.Find(e => e == nextStateIndex));
		if (exploredStates.Find(e => e == nextStateIndex) == 0) 
		{
			isNewStateExplored = true;
			exploredStates.Add(nextStateIndex);
		}
	}

	/*void CheckIsChangingAction()
	{
		
	}*/

	int ChooseAction()
	{
		//input: currentState, q_table
		//output: action
		float randomNumber = Random.Range(0.0f, 1.0f);
		if (randomNumber <= epsilon)
		{
			actionIndex = Argmax(stateIndex);
		}
		else
		{
			actionIndex = actionList[Random.Range(0, actionList.Count)];
		}
		return actionIndex;
	}

	void MakeAction()
	{
		//actionIndex = 2;
		switch (actionIndex)
		{
		case 0:
			// move left
			playerMovement.Move(-1,0,out isCollision,out isChangingAction,MOVEMENT_STEP_SIZE);
			playerMovement.Animating (-MOVEMENT_STEP_SIZE, 0);
			break;
		case 1:
			// move top
			playerMovement.Move(0,1,out isCollision,out isChangingAction,MOVEMENT_STEP_SIZE);
			playerMovement.Animating (0, MOVEMENT_STEP_SIZE);
			break;
		case 2:
			// move right
			playerMovement.Move(1,0,out isCollision,out isChangingAction,MOVEMENT_STEP_SIZE);
			playerMovement.Animating (MOVEMENT_STEP_SIZE, 0);
			break;
		case 3:
			// move bottom
			playerMovement.Move(0,-1,out isCollision,out isChangingAction,MOVEMENT_STEP_SIZE);
			playerMovement.Animating (0, -MOVEMENT_STEP_SIZE);
			break;
		}
	}

	int Argmax(int currentState)
	{
		List<float> row = new List<float>();
		List<int> rowIndex = new List<int>();
		//Debug.Log(currentState);
		for (int i=0; i<ACTION_NUMBERS; i++)
		{
			row.Add(q_table[currentState, i]);
		}
		float max = row.Max();
		for (int i=0; i<ACTION_NUMBERS; i++)
		{
			if (row[i]==max)
			{
				rowIndex.Add(i);
			}
		}
		// for instance, rowIndex = {3, 5, 9}
		int randomIndex = Random.Range(0, rowIndex.Count); // 0, 1, 2
		return rowIndex[randomIndex];
	}

	void Learning()
	{
		IdentifyNextState();
		Q_Learning(nextStateIndex);
		Q_Learning_Console();
		stateIndex = nextStateIndex;
	}

	void Q_Learning(int nextStateIndex)
	{
		float r = RewardFunction();
		float ExactReward = r + gamma*(Argmax(nextStateIndex));
		float EstimatedReward = q_table[stateIndex, actionIndex];
		q_table[stateIndex, actionIndex] += alpha*(ExactReward - EstimatedReward);
	}

	void Q_Learning_Console()
	{
		information="";
		information+="At iteration " + frameNumber +"\n";
		for(int i=1;i<=N_STATES;i++)
		{
			information += "At state " + i + " : ";
			for(int j=0;j<ACTION_NUMBERS;j++)
			{
				information+=q_table[i,j] + " ";
			}
			information += "\n";
		}
	}

	float RewardFunction()
	{
		//int damageTaken = (int)(playerHealth.startingHealth - playerHealth.currentHealth)/10;
		float r = 0;
		if (isCollision) r -= collisionCost + moveCost;
		else r -= moveCost;
		if (isNewStateExplored)
		{
			r += explorationReward;
			isNewStateExplored = false;
		}
		return r;
	}
}