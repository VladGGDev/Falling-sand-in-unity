[System.Serializable]
public struct Particle
{
	public byte type;
	//type 0 - air
	//type 1 - sand
	//type 2 - water
	//type 3 - solid
	public float gradientColor;
	public bool hasBeenUpdated;
	public sbyte fluidHVel;
	public int framesWaited;
	public int freshSpread;
	public int lifeTime;
	public int totalLifeTime;
}	