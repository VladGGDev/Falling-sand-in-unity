using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Particle", menuName = "Scriptable Objects/Particle")]
public class Particle : ScriptableObject
{
	[Tooltip("The type of the particle will be used in scripts.\n" +
		"Each particle must have a unique type (just like an id).")]
	public byte type;
	//type 0 - air
	//type 1 - sand
	//type 2 - water
	//type 3 - solid
	//public bool useSecondColor = false;
	//public bool hasBeenUpdated = false;

	[HideInInspector]
	public sbyte fluidHVel = 1;

	[Tooltip("The name is an optional alternative for the type\n")]
	public string particleName;
	public Color primaryColor = Color.black;
	[Tooltip("The secondary color is usually darker.")]
	public Color secondaryColor = Color.black;

	[HideInInspector]
	public bool hasBeenUpdated = false;
	[HideInInspector]
	public bool useSecondColor = false;

	[Space(15)]
	[Tooltip("How many times should this particle move per frame.")]
	public int stepsPerFrame = 1;
	[Tooltip("How many frames should a particle wait before moving.")]
	public int waitFrames = 0;
	[HideInInspector]
	int framesWaited = 0;

	[Space(15)]
	[Tooltip("The particle will check in the order of the elements in this array if it can move.")]
	public ParticleMovingDirection[] moveChecks = new ParticleMovingDirection[4];

	//[Space(15)]
	//public Particle[] cantTouch = new Particle[1];
}

[System.Serializable]
public class ParticleMovingDirection
{
	public enum MoveDirection
	{
		Down,
		RandomDownDiagonal,
		DownRight,
		DownLeft,
		TowardsHorisontalVelocity,
		RandomHorisontal,
		Right,
		Left,
		Up,
		RandomUpDiagonal,
		UpRight,
		UpLeft
	}

	[Tooltip("Choose the particle movement order.\n" +
		"For sand, use the first 4,\n" +
		"for water, use the first 8,\n" +
		"for gas, use the last 4.\n" +
		"If you're making something else, see what fits.")]
	public MoveDirection moveDirection;
	[Range(0, 1f)]
	[Tooltip("How likely is the particle to move this frame.\n" +
		"Choose 1 if the particle should always move.")]
	public float movementChance = 1f;
	[Tooltip("If the Movement Chance fails should this particle check if it can still move in another way?\n" +
		"Useful for making snow or gas.")]
	public bool continueIfFailed = false;
}

[System.Serializable]
public class MyClass
{
	public int numberA;
	public int numberB;
}