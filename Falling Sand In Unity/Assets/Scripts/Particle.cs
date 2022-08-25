using System.Collections;
using System.Collections.Generic;
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
	public sbyte fluidHVel = 0;
	public int framesWaited = 0;
	public int freshSpread = 0;
	public int lifeTime = 0;
}	