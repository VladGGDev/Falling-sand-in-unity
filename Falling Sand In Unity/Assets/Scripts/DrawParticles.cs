using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawParticles : MonoBehaviour
{
	public ParticleLogic particleLogic;
	public ParticleObject[] particleObjects;
	[Range(0, 30)]
	public int shiftThickness = 5;
	byte numKeyPressed = 0;
	Camera cam;

	private void Awake()
	{
		cam = Camera.main;
		if (particleObjects.Length > 10)
		{
			Debug.LogWarning("You can't draw more than 10 diffrent particles, you don't have more than 10 number keys!");
		}
	}

	private void Update()
	{
		Vector2 mousePos = cam.ScreenToViewportPoint(Input.mousePosition);
		mousePos = new Vector2(
			Mathf.Clamp(mousePos.x, 0, 1f),
			Mathf.Clamp(mousePos.y, 0, 1f));

		Vector2Int gridMousePos = new Vector2Int((int)(mousePos.x * particleLogic.simWidth), (int)(mousePos.y * particleLogic.simHeight));

		GetAlphaInput();

		if (Input.GetMouseButton(0))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				for (int y = -shiftThickness / 2; y < Mathf.CeilToInt((float)shiftThickness / 2); y++)
				{
					for (int x = -shiftThickness / 2; x < Mathf.CeilToInt((float)shiftThickness / 2); x++)
					{
						particleLogic.CreateParticle(particleObjects[numKeyPressed].type, new Vector2Int(gridMousePos.x + x, gridMousePos.y + y), false);
					}
				}
			}
			else
			{
				particleLogic.CreateParticle(particleObjects[numKeyPressed].type, new Vector2Int(gridMousePos.x, gridMousePos.y), false);
			}
		}
		if (Input.GetMouseButton(1))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				for (int y = -shiftThickness / 2; y < Mathf.CeilToInt((float)shiftThickness / 2); y++)
				{
					for (int x = -shiftThickness / 2; x < Mathf.CeilToInt((float)shiftThickness / 2); x++)
					{
						particleLogic.DeleteParticle(new Vector2Int(gridMousePos.x + x, gridMousePos.y + y));
					}
				}
			}
			else
			{
				particleLogic.DeleteParticle(new Vector2Int(gridMousePos.x, gridMousePos.y));
			}
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
			for (int y = 0; y < particleLogic.particles.GetLength(1); y++)
			{
				for (int x = 0; x < particleLogic.particles.GetLength(0); x++)
				{
					particleLogic.DeleteParticle(new Vector2Int(x, y));
				}
			}
		}
	}

	void GetAlphaInput()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			numKeyPressed = 0;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			numKeyPressed = 1;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			numKeyPressed = 2;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			numKeyPressed = 3;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			numKeyPressed = 4;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			numKeyPressed = 5;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha7))
		{
			numKeyPressed = 6;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			numKeyPressed = 7;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			numKeyPressed = 8;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			numKeyPressed = 9;
		}

		numKeyPressed = (byte)Mathf.Clamp(numKeyPressed, 0, particleObjects.Length - 1);
	}
}
