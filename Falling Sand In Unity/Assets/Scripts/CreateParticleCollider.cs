using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PointOutput
{
	public List<Vector2> points;
}

public class CreateParticleCollider : MonoBehaviour
{
    public ParticleLogic particleLogic;
	PointOutput[] pointOutputs;
    public ParticleColliderGroup[] groups;
    [Space(10)]
    [Tooltip("How many physics frames should the script skip before generating the collider.")]
    public int skipPhysicsFrames = 0;
    int framesSkiped = 0;

    private void Start()
    {
		pointOutputs = new PointOutput[groups.Length];
        for (int i = 0; i < groups.Length; i++)
        {
			pointOutputs[i] = new PointOutput();
			pointOutputs[i].points = new List<Vector2>();
            groups[i].outputParticles = new byte[particleLogic.simWidth, particleLogic.simHeight];
        }
    }

    private void FixedUpdate()
    {
		//Check if you should skip this physics frame
        framesSkiped--;
        if(framesSkiped < 0)
        {
            framesSkiped = skipPhysicsFrames;
        }
        else
        {
            return;
        }

		//Clear the marching squares list
		for (int i = 0; i < pointOutputs.Length; i++)
		{
			pointOutputs[i].points.Clear();
		}

        //Eliminate particles that don't have neighbors
        for (int i = 0; i < groups.Length; i++)
        {
            IgnoreParticles(groups[i]);
        }

		//Marching squares
		for (int i = 0; i < groups.Length; i++)
		{
			MarchingSquares(groups[i], i);
		}
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			return;
		}

		for (int i = 0; i < pointOutputs[0].points.Count; i+=2)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(pointOutputs[0].points[i], pointOutputs[0].points[i + 1]);
		}
	}

	void MarchingSquares(ParticleColliderGroup g, int index)
	{
		int h = particleLogic.simHeight;
		int w = particleLogic.simWidth;
		
		for (int i = 0; i < h - 1; i++)
		{
			for (int j = 0; j < w - 1; j++)
			{
				//res is the distance from ont pixel to the other
				float resX = 1 / (particleLogic.pixelsPerUnit / transform.localScale.x);
				float resY = 1 / (particleLogic.pixelsPerUnit / transform.localScale.y);
				float x = (j - w / 2f) / (particleLogic.pixelsPerUnit / transform.localScale.x);
				float y = (i - h / 2f) / (particleLogic.pixelsPerUnit / transform.localScale.y);
				Vector2 a = new Vector2(x + resX * 0.5f, y);
				Vector2 b = new Vector2(x + 1, y + resY * 0.5f);
				Vector2 c = new Vector2(x + resY * 0.5f, y + 1);
				Vector2 d = new Vector2(x, y + resY * 0.5f);
				byte state = (byte)BinaryStateConverter(
					CheckForParticleInGroup(j, i, g),
					CheckForParticleInGroup(j + 1, i, g),
					CheckForParticleInGroup(j + 1, i + 1, g),
					CheckForParticleInGroup(j, i + 1, g));
				//byte state = (byte)BinaryStateConverter(
				//	g.outputParticles[j, i],
				//	g.outputParticles[j + 1, i],
				//	g.outputParticles[j + 1, i + 1],
				//	g.outputParticles[j, i + 1]);

				switch (state)
				{
					case 1:
						pointOutputs[index].points.Add(a);
						pointOutputs[index].points.Add(d);
						break;
					case 2:
						pointOutputs[index].points.Add(a);
						pointOutputs[index].points.Add(b);
						break;
					case 3:
						pointOutputs[index].points.Add(b);
						pointOutputs[index].points.Add(d);
						break;
					case 4:
						pointOutputs[index].points.Add(b);
						pointOutputs[index].points.Add(c);
						break;
					case 5:
						pointOutputs[index].points.Add(a);
						pointOutputs[index].points.Add(b);
						pointOutputs[index].points.Add(c);
						pointOutputs[index].points.Add(d);
						break;
					case 6:
						pointOutputs[index].points.Add(a);
						pointOutputs[index].points.Add(c);
						break;
					case 7:
						pointOutputs[index].points.Add(c);
						pointOutputs[index].points.Add(d);
						break;
					case 8:
						pointOutputs[index].points.Add(c);
						pointOutputs[index].points.Add(d);
						break;
					case 9:
						pointOutputs[index].points.Add(a);
						pointOutputs[index].points.Add(c);
						break;
					case 10:
						pointOutputs[index].points.Add(a);
						pointOutputs[index].points.Add(d);
						pointOutputs[index].points.Add(b);
						pointOutputs[index].points.Add(c);
						break;
					case 11:
						pointOutputs[index].points.Add(b);
						pointOutputs[index].points.Add(c);
						break;
					case 12:
						pointOutputs[index].points.Add(b);
						pointOutputs[index].points.Add(d);
						break;
					case 13:
						pointOutputs[index].points.Add(a);
						pointOutputs[index].points.Add(b);
						break;
					case 14:
						pointOutputs[index].points.Add(a);
						pointOutputs[index].points.Add(d);
						break;
				}
			}
		}

		int BinaryStateConverter(bool a, bool b, bool c, bool d)
		{
			int ia = a ? 1 : 0;
			int ib = b ? 1 : 0;
			int ic = c ? 1 : 0;
			int id = d ? 1 : 0;
			return ia + ib * 2 + ic * 4 + id * 8;
		}
	}

    void IgnoreParticles(ParticleColliderGroup g)
    {
		if(g.minNeighbors == 0)
		{
			for (int y = 0; y < particleLogic.simHeight; y++)
			{
				for (int x = 0; x < particleLogic.simWidth; x++)
				{
					if (particleLogic.particles[x, y].type != 0)
					g.outputParticles[x, y] = particleLogic.particles[x, y].type;
				}
			}
			return;
		}

        int neighbors = 0;
        bool[,] keepParticleH = new bool[particleLogic.simWidth, particleLogic.simHeight];
        bool[,] keepParticleV = new bool[particleLogic.simWidth, particleLogic.simHeight];

        //Horizontal neighbor check
		for (int y = 0; y < particleLogic.simHeight; y++){
			for (int x = 0; x < particleLogic.simWidth; x++){   //Repeats for every particle

                if (particleLogic.particles[x, y].type == 0
                    || !CheckForParticleInGroup(x, y, g))
                {
                    continue;
                }

                //Checks for neighbors
                int ii = 1;
                while (true)
                {
					if (x + ii >= particleLogic.simWidth)
					{
						//neighbors = ii - 1;
						keepParticleH[x, y] = true;
						break;
					}

					if (CheckForParticleInGroup(x + ii, y, g))
					{
						neighbors++;
					}
					else
					{
						if(neighbors < g.minNeighbors)
						{
							if (x == 0)
							{
								keepParticleH[x, y] = true;
							}
							else
							{
								keepParticleH[x, y] = false;
							}
						}
						else
						{
							keepParticleH[x, y] = true;
						}
						break;
					}
					ii++;
                }

				if (keepParticleH[x, y])
                {
                    for (int i = 1; i <= neighbors; i++)
                    {
                        keepParticleH[x + i, y] = true;
                    }
                    x += neighbors + 1;
                }
                neighbors = 0;
			}
		}

		//Vertical neighbor check
		for (int x = 0; x < particleLogic.simWidth; x++)
		{
			for (int y = 0; y < particleLogic.simHeight; y++)
			{   //Repeats for every particle

				if (particleLogic.particles[x, y].type == 0
                    || !CheckForParticleInGroup(x, y, g))
				{
					continue;
				}

				//Checks for neighbors
				int ii = 1;
				while (true)
				{
					if (y + ii >= particleLogic.simHeight)
					{
						//neighbors = ii - 1;
						keepParticleV[x, y] = true;
						break;
					}

					if (CheckForParticleInGroup(x, y + ii, g))
					{
						neighbors++;
					}
					else
					{
						if (neighbors < g.minNeighbors)
						{
							if (y == 0)
							{
								keepParticleV[x, y] = true;
							}
							else
							{
								keepParticleV[x, y] = false;
							}
						}
						else
						{
							keepParticleV[x, y] = true;
						}
						break;
					}
					ii++;
				}

				if (keepParticleV[x, y])
				{
					for (int i = 1; i <= neighbors; i++)
					{
						keepParticleV[x, y + i] = true;
					}
					y += neighbors + 1;
				}
				neighbors = 0;
			}
		}

		//keepParticle array to Particle array
		for (int y = 0; y < particleLogic.simHeight; y++)
		{
			for (int x = 0; x < particleLogic.simWidth; x++)
			{
                if (keepParticleH[x, y] && keepParticleV[x, y])
                {
                    g.outputParticles[x, y] = particleLogic.particles[x, y].type;
                }
                else
                {
                    g.outputParticles[x, y] = 0;
				}
			}
		}
	}

	bool CheckForParticleInGroup(int x, int y, ParticleColliderGroup g)
	{
		for (int i = 0; i < g.particleObjects.Length; i++)  //Checks for every particle object
		{
			if (particleLogic.particles[x, y].type == g.particleObjects[i].type)
			{
				return true;
			}
		}
		return false;
	}
}