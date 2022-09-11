using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DrawParticles : MonoBehaviour
{
	public ParticleLogic particleLogic;
	public ParticleObject[] particleObjects;
	[Range(0, 30)]
	public int thickness = 5;
	[Space (5)]
	[Header("Color gizmos:")]
	public bool showPoint = true;
	[Range(0.5f, 1f)]
	public float pointSize = 0.9f;
	public Color pointColor = Color.black;
	public bool showSquare = true;
	public Color squareColor = Color.red;
	sbyte numKeyPressed = 0;
	Camera cam;
	Vector2Int previousMousePos;
	Vector2Int currentMousePos;
	

	private void Awake()
	{
		cam = Camera.main;
		previousMousePos = currentMousePos;
	}

	private void Update()
	{
		Vector2 mousePos = cam.ScreenToViewportPoint(Input.mousePosition);
		mousePos = new Vector2(
			Mathf.Clamp(mousePos.x, 0, 1f),
			Mathf.Clamp(mousePos.y, 0, 1f));
		
		Vector2Int gridMousePos = 
			new Vector2Int((int)(mousePos.x * particleLogic.simWidth), (int)(mousePos.y * particleLogic.simHeight));

		GetAlphaInput();		
		if(!(0 > Input.mousePosition.x || 0 > Input.mousePosition.y 
			|| Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y))
		{
			GetScrollThickness();
			GetScrollInput();
		}

		if(previousMousePos.x == 0 || previousMousePos.x == particleLogic.simWidth
			 || previousMousePos.y == 0 || previousMousePos.y == particleLogic.simHeight)
		{
			previousMousePos = gridMousePos;
		}

		currentMousePos = gridMousePos;

		//if (Input.GetMouseButton(0))
		//{
		//	if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
		//	{
		//		for (int y = -shiftThickness / 2; y < Mathf.CeilToInt((float)shiftThickness / 2); y++)
		//		{
		//			for (int x = -shiftThickness / 2; x < Mathf.CeilToInt((float)shiftThickness / 2); x++)
		//			{
		//				if (Input.GetKey(KeyCode.LeftControl))
		//				{
		//					if(x*x + y*y > (shiftThickness/2)*(shiftThickness / 2))
		//					{
		//						continue;
		//					}
		//				}
		//				lineDrawing(previousMousePos.x + x, previousMousePos.y + y,
		//					currentMousePos.x + x, currentMousePos.y + y, true);
		//			}
		//		}
		//	}
		//	else
		//	{
		//		lineDrawing(previousMousePos.x, previousMousePos.y, currentMousePos.x, currentMousePos.y, true);
		//	}
		//}
		//if (Input.GetMouseButton(1))
		//{
		//	if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
		//	{
		//		for (int y = -shiftThickness / 2; y < Mathf.CeilToInt((float)shiftThickness / 2); y++)
		//		{
		//			for (int x = -shiftThickness / 2; x < Mathf.CeilToInt((float)shiftThickness / 2); x++)
		//			{
		//				if (Input.GetKey(KeyCode.LeftControl))
		//				{
		//					if (x * x + y * y > (shiftThickness / 2) * (shiftThickness / 2))
		//					{
		//						continue;
		//					}
		//				}
		//				lineDrawing(previousMousePos.x + x, previousMousePos.y + y,
		//					currentMousePos.x + x, currentMousePos.y + y, false);
		//			}
		//		}
		//	}
		//	else
		//	{
		//		lineDrawing(previousMousePos.x, previousMousePos.y, currentMousePos.x, currentMousePos.y, false);
		//	}
		//}

		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
		{
			for (int y = -thickness / 2; y < Mathf.CeilToInt((float)thickness / 2); y++)
			{
				for (int x = -thickness / 2; x < Mathf.CeilToInt((float)thickness / 2); x++)
				{
					if (Input.GetKey(KeyCode.LeftControl))
					{
						if (x * x + y * y > (thickness / 2) * (thickness / 2) + 1f)
						{
							continue;
						}
					}
					if (Input.GetMouseButton(0))
					{
						lineDrawing(previousMousePos.x + x, previousMousePos.y + y,
							currentMousePos.x + x, currentMousePos.y + y, true);
					}
					else if (Input.GetMouseButton(1))
					{
						lineDrawing(previousMousePos.x + x, previousMousePos.y + y,
							currentMousePos.x + x, currentMousePos.y + y, false);
					}
				}
			}
		}
		else
		{
			if (Input.GetMouseButton(0))
			{
				lineDrawing(previousMousePos.x, previousMousePos.y, currentMousePos.x, currentMousePos.y, true);
			}
			else if (Input.GetMouseButton(1))
			{
				lineDrawing(previousMousePos.x, previousMousePos.y, currentMousePos.x, currentMousePos.y, false);
			}
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
			for (int y = 0; y < particleLogic.particles.GetLength(1); y++)
			{
				for (int x = 0; x < particleLogic.particles.GetLength(0); x++)
				{
					particleLogic.DeleteParticle(x, y);
				}
			}
		}

		previousMousePos = currentMousePos;
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
		else
		{
			return;
		}

		numKeyPressed = (sbyte)Mathf.Clamp(numKeyPressed, 0, particleObjects.Length - 1);
		Debug.Log("Selected " + particleObjects[numKeyPressed].name);
	}

	void GetScrollInput()
	{
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
		{
			return;
		}

		if(Input.mouseScrollDelta.y < 0 && numKeyPressed >= 0)
		{
			numKeyPressed--;
		}
		else if(Input.mouseScrollDelta.y > 0)
		{
			numKeyPressed++;
		}
		else
		{
			return;
		}

		numKeyPressed = (sbyte)Mathf.Clamp(numKeyPressed, 0, particleObjects.Length - 1);
		Debug.Log("Selected " + particleObjects[numKeyPressed].name);
	}

	void GetScrollThickness()
	{
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
		{
			if (Input.mouseScrollDelta.y < 0 && numKeyPressed >= 0)
			{
				thickness--;
			}
			else if (Input.mouseScrollDelta.y > 0)
			{
				thickness++;
			}
			else
			{
				return;
			}
			thickness = Mathf.Clamp(thickness, 1, 30);
			Debug.Log("Thickness is now " + thickness);
		}
		else
		{
			return;
		}
	}

	void lineDrawing(int x0, int y0, int x1, int y1, bool create)
	{
		int dx = Mathf.Abs(x1 - x0);

		int sx = x0 < x1 ? 1 : -1;

		int dy = -Mathf.Abs(y1 - y0);

		int sy = y0 < y1 ? 1 : -1;

		float error = dx + dy;


		while (true)
		{
			if (create){
				particleLogic.CreateParticle(particleObjects[numKeyPressed].type, x0, y0, false);
			}
			else{
				particleLogic.DeleteParticle(x0, y0);
			}

			if (x0 == x1 && y0 == y1)
				break;

			float e2 = 2 * error;

			if (e2 >= dy)
			{
				if (x0 == x1)
					break;

				error = error + dy;
				x0 = x0 + sx;

			}

			if (e2 <= dx)
			{

				if (y0 == y1) 
					break;

				error = error + dx;
				y0 = y0 + sy;

			}
		}
	}


	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Vector2 pointPos = currentMousePos + Vector2.one * 0.5f -
												new Vector2(particleLogic.simWidth * 0.5f, particleLogic.simHeight * 0.5f);
		Vector2 pointScale = new Vector3(transform.localScale.x / (particleLogic.simWidth * 10f),
						transform.localScale.y / (particleLogic.simHeight * 10f));
		if (showPoint)
		{
			Gizmos.color = pointColor;
			Gizmos.DrawWireCube(pointPos, pointScale * pointSize);
		}
		if (showSquare)
		{
			Gizmos.color = squareColor;
			Gizmos.DrawWireCube(pointPos - Vector2.one * ((thickness - 1) % 2) * 0.5f, pointScale * thickness);
		}
	}
}