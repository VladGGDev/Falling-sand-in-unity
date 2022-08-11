using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawParticles : MonoBehaviour
{
	public ParticleLogic particleLogic;
	[Range(0, 30)]
	public int shiftThickness = 5;
	int numKeyPressed = 1;
	Camera cam;

	private void Awake()
	{
		cam = Camera.main;
	}

	private void Update()
	{
		Vector2 mousePos = cam.ScreenToViewportPoint(Input.mousePosition);
		mousePos = new Vector2(
			Mathf.Clamp(mousePos.x, 0, 1f),
			Mathf.Clamp(mousePos.y, 0, 1f));

		Vector2Int gridMousePos = new Vector2Int((int)(mousePos.x * particleLogic.simWidth), (int)(mousePos.y * particleLogic.simHeight));

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			numKeyPressed = 1;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			numKeyPressed = 2;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			numKeyPressed = 3;
		}

		if (Input.GetMouseButton(0))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				for (int y = -shiftThickness / 2; y < Mathf.CeilToInt((float)shiftThickness / 2); y++)
				{
					for (int x = -shiftThickness / 2; x < Mathf.CeilToInt((float)shiftThickness / 2); x++)
					{
						particleLogic.CreateParticle((byte)numKeyPressed, new Vector2Int(gridMousePos.x + x, gridMousePos.y + y));
					}
				}
			}
			else
			{
				particleLogic.CreateParticle((byte)numKeyPressed, new Vector2Int(gridMousePos.x, gridMousePos.y));
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
					particleLogic.particles[x, y].type = 0;
				}
			}
		}
	}
}
