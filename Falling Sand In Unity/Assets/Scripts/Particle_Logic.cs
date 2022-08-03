using System.Collections;
using System.Collections.Generic;
//using System;
using UnityEngine;

[System.Serializable]
public class Particle
{
    public byte type;
        //type 0 - air
        //type 1 - sand
        //type 2 - water
		//type 3 - solid
    public bool useSecondColor = false;
	public bool hasBeenUpdated = false;

	public sbyte fluidHVel = 1;
}

public class Particle_Logic : MonoBehaviour
{
	int numKeyPressed = 1;

    public int simWidth = 10;
    public int simHeight = 10;
    public Particle[,] particles;
	Camera cam;

	[Space(10f)]
	public Texture2D texture;

	public Color colorDiffrence;
	public Color airColor;
	public Color sandColor;
	public Color waterColor;
	public Color solidColor;

	public Color[] particleColors;
	public Renderer rend;

	private void Awake()
	{
		particles = new Particle[simWidth, simHeight];
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i % simWidth, i / simWidth] = new Particle();
			particles[i % simWidth, i / simWidth].useSecondColor = Random.value > 0.5f;
			particles[i % simWidth, i / simWidth].fluidHVel = Random.value > 0.5f ? (sbyte)1 : (sbyte)-1;
		}
		texture = new Texture2D(simWidth, simHeight);
		particleColors = new Color[particles.Length];
		cam = Camera.main;
		texture.filterMode = FilterMode.Point;


	}

	#region Helping Functions
	int PositionToIndex (int xPos, int yPos)
	{
		xPos = Mathf.Clamp(xPos, 0, simWidth -1);
		yPos = Mathf.Clamp(yPos, 0, simHeight - 1);
        return yPos * simWidth + xPos;
	}

	Vector2Int IndexToPosition(int index)
	{
		return new Vector2Int(index % simWidth, index / simWidth);
	}

    bool CheckForParticle(byte type, Vector2Int atPos)
	{
		if (particles[atPos.x, atPos.y].type == type)
		{
            return true;
		}
		else
		{
			return false;
		}
	}

	bool CheckForAnyParticle(Vector2Int atPos)
	{
		atPos.x = Mathf.Clamp(atPos.x, 0, simWidth - 1);
		atPos.y = Mathf.Clamp(atPos.y, 0, simHeight - 1);

		if (particles[atPos.x, atPos.y].type != 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	void DeleteParticle(Vector2Int atPos)
	{
		atPos.x = Mathf.Clamp(atPos.x, 0, simWidth - 1);
		atPos.y = Mathf.Clamp(atPos.y, 0, simHeight - 1);
		//if (atatIndexPos < 0 || atIndex >= particles.Length)
		//{
		//	Debug.LogWarning("In the DeleteParticle() function, atIndex (= " + atIndex + ") int is outside of array bounds");
		//	return;
		//}

		particles[atPos.x, atPos.y].type = 0;
	}

	void CreateParticle(byte type, Vector2Int atPos)
	{
		atPos.x = Mathf.Clamp(atPos.x, 0, simWidth - 1);
		atPos.y = Mathf.Clamp(atPos.y, 0, simHeight - 1);
		//if (atPos.y < 0 || atPos.y >= particles.Length)
		//{
		//	Debug.LogWarning("In the CreateParticle() function, atIndex (= " + atPos + ") int is outside of array bounds");
		//	return;
		//}

		particles[atPos.x, atPos.y].type = type;
	}

	void SetParticleUpdateStatus(Vector2Int atPos, bool status)
	{
		atPos.x = Mathf.Clamp(atPos.x, 0, simWidth - 1);
		atPos.y = Mathf.Clamp(atPos.y, 0, simHeight - 1);

		particles[atPos.x, atPos.y].hasBeenUpdated = status;
	}
	#endregion

	private void Update()
	{
		Vector2 mousePos = cam.ScreenToViewportPoint(Input.mousePosition);
		mousePos = new Vector2(
			Mathf.Clamp(mousePos.x, 0, 1f),
			Mathf.Clamp(mousePos.y, 0, 1f));

		Vector2Int gridMousePos = new Vector2Int((int)(mousePos.x * simWidth), (int)(mousePos.y * simHeight));

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
				for (int y = -2; y < 3; y++)
				{
					for (int x = -2; x < 3; x++)
					{
						CreateParticle((byte)numKeyPressed, new Vector2Int(gridMousePos.x + x, gridMousePos.y + y));
					}
				}
			}
			else
			{
				CreateParticle((byte)numKeyPressed, new Vector2Int(gridMousePos.x, gridMousePos.y));
			}
		}
		if (Input.GetMouseButton(1))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				for (int y = -2; y < 3; y++)
				{
					for (int x = -2; x < 3; x++)
					{
						DeleteParticle(new Vector2Int(gridMousePos.x + x, gridMousePos.y + y));
					}
				}
			}
			else
			{
				DeleteParticle(new Vector2Int(gridMousePos.x, gridMousePos.y));
			}
		}
	}

	private void FixedUpdate()      //Runs at 50 FPS by default
	{
		//CreateParticle(1, PositionToIndex(12, 24));	//Create a test particle flow

		for (int y = 0; y < simHeight; y++)
		{
			for (int x = 0; x < simWidth; x++)
			{
				if (particles[x, y].hasBeenUpdated && particles[x, y].type != 0)
				{
					continue;
				}

				if (CheckForParticle(1, new Vector2Int(x, y)))     //Sand physics
				{
					SandPhysics(new Vector2Int(x, y));
				}
				else if (CheckForParticle(2, new Vector2Int(x, y)))     //Water physics
				{
					WaterPhysics(new Vector2Int(x, y));
				}
				else if (CheckForParticle(3, new Vector2Int(x, y)))     //Solid physics
				{
					continue;
				}
			}
		}
		ConvertDataToPixels();
	}

	public void ConvertDataToPixels()    //	!!!!! ADD MORE IFs WHEN YOU ADD MORE PARTICLE TYPES !!!!!!
	{
		for (int i = 0; i < particles.Length; i++)
		{
			int ix = i % simWidth;
			int iy = i / simWidth;
			particles[ix, iy].hasBeenUpdated = false;	//Reset update status for every particle

			if (CheckForParticle(0, new Vector2Int(ix, iy)))
			{
				particleColors[i] = airColor;
			}
			else if (CheckForParticle(1, new Vector2Int(ix, iy)))
			{
				if (particles[ix, iy].useSecondColor)
				{
					particleColors[i] = sandColor * colorDiffrence;
				}
				else
				{
					particleColors[i] = sandColor;
				}
			}
			else if (CheckForParticle(2, new Vector2Int(ix, iy)))
			{
				if (particles[ix, iy].useSecondColor)
				{
					particleColors[i] = waterColor * colorDiffrence;
				}
				else
				{
					particleColors[i] = waterColor;
				}
			}
			else if (CheckForParticle(3, new Vector2Int(ix, iy)))
			{
				if (particles[ix, iy].useSecondColor)
				{
					particleColors[i] = solidColor * colorDiffrence;
				}
				else
				{
					particleColors[i] = solidColor;
				}
			}
		}

		texture.SetPixels(particleColors);
		texture.Apply();
		rend.material.mainTexture = texture;
	}

	void SandPhysics(Vector2Int particlePos)
	{
		if (particlePos.y == 0)
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x, particlePos.y), true);
			return;
		}

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x, particlePos.y - 1)))    //Check for particle down
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x, particlePos.y - 1), true);
			CreateParticle(1, new Vector2Int(particlePos.x, particlePos.y - 1));
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x + 1, particlePos.y - 1)))   //Check for particle down right
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + 1, particlePos.y - 1), true);
			CreateParticle(1, new Vector2Int(particlePos.x + 1, particlePos.y - 1));
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x - 1, particlePos.y - 1)))   //Check for particle down left
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x - 1, particlePos.y - 1), true);
			CreateParticle(1, new Vector2Int(particlePos.x - 1, particlePos.y - 1));
		}
		else
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x, particlePos.y), true);
			return;
		}

		DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));   //Remove the initial position particle
	}

	void WaterPhysics(Vector2Int particlePos)
	{
		sbyte lastVelocity = particles[particlePos.x, particlePos.y].fluidHVel;

		if (!CheckForAnyParticle(new Vector2Int(particlePos.x, particlePos.y - 1)) && particlePos.y != 0)    //Check for particle down
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x, particlePos.y - 1), true);
			CreateParticle(2, new Vector2Int(particlePos.x, particlePos.y - 1));
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x + 1, particlePos.y - 1)) && particlePos.y != 0)   //Check for particle down right
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + 1, particlePos.y - 1), true);
			CreateParticle(2, new Vector2Int(particlePos.x + 1, particlePos.y - 1));
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x - 1, particlePos.y - 1)) && particlePos.y != 0)   //Check for particle down left
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x - 1, particlePos.y - 1), true);
			CreateParticle(2, new Vector2Int(particlePos.x - 1, particlePos.y - 1));
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x + lastVelocity, particlePos.y)))	//Check towards velocity
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + lastVelocity, particlePos.y), true);
			CreateParticle(2, new Vector2Int(particlePos.x + lastVelocity, particlePos.y));
			particles[particlePos.x + lastVelocity, particlePos.y].fluidHVel = lastVelocity;
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x + 1, particlePos.y)))		//Check for particle right
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x + 1, particlePos.y), true);
			CreateParticle(2, new Vector2Int(particlePos.x + 1, particlePos.y));
			particles[particlePos.x + 1, particlePos.y].fluidHVel = 1;
		}
		else if (!CheckForAnyParticle(new Vector2Int(particlePos.x - 1, particlePos.y)))       //Check for particle left
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x - 1, particlePos.y), true);
			CreateParticle(2, new Vector2Int(particlePos.x - 1, particlePos.y));
			particles[particlePos.x - 1, particlePos.y].fluidHVel = -1;
		}
		else
		{
			SetParticleUpdateStatus(new Vector2Int(particlePos.x, particlePos.y), true);
			return;
		}

		DeleteParticle(new Vector2Int(particlePos.x, particlePos.y));   //Remove the initial position particle
	}
}