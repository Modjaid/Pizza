using UnityEngine;

public class VFXManager : MonoBehaviour 
{
    public static VFXManager Instance { get; private set; }

    public ParticleSystem sprintEffect;
    public GameObject fightEffect;

    private void Awake()
    {
        Instance = this;
    }

    public void CreateFightEffect(PlayerAxisController player, Enemy enemy)
    {
        // todo: That's a dirty hack.
        player.SetRendererActive(false);
        player.IsHidden = true;
        //enemy.SetRendererActive(false);
        Destroy(enemy.gameObject);

        Vector3 spawnPosition = (player.transform.position + enemy.transform.position) / 2;

        Instantiate(fightEffect, spawnPosition, Quaternion.identity);
    }

    //note: damn...those single-lined methods.

    public void PlaySprintEffect()
    {
        sprintEffect.Play();
    }   

    public void StopSprintEffect()
    {
        sprintEffect.Stop();
    } 

    public void ClearSprintEffect()
    {
        sprintEffect.Clear();
    }
}