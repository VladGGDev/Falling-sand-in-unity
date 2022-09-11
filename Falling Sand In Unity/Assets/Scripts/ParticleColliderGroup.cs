using UnityEngine;

[System.Serializable]
public class ParticleColliderGroup
{
    [Tooltip("The particle objects that the outpuc collider will be created for.")]
    public ParticleObject[] particleObjects;
    [Tooltip("The minimum amount of neighbors a particle has to have to not be ignored when creating the collider.")]
    public byte minNeighbors = 1;
    [Tooltip("The collider that will be made in the form of the particle shape.\n" +
		"You should use a diffrent collider for solids, liquids, and harmful particles.")]
    public PolygonCollider2D outputCollider;
    
    public byte[,] outputParticles;
}
