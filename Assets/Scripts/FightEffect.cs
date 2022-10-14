using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightEffect : MonoBehaviour
{
    public ParticleSystem smokeParticles;
    public AnimationCurve particleScale;
    public GameObject animationObject;
    public Animator animator;
    public float showAnimationObjectDelay;
    public float destroyAnimationObjectDelay;

    private void Awake()
    {
        StartCoroutine(ScaleParticles());
        StartCoroutine(ShowAnimationObject());
    }

    private IEnumerator ScaleParticles()
    {
        float timeAlive = 0f;

        while(true)
        {
            float scale = particleScale.Evaluate(timeAlive);
            smokeParticles.gameObject.transform.localScale = new Vector3(scale, scale, scale);
            timeAlive += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ShowAnimationObject()
    {
        animationObject.SetActive(false);
        yield return new WaitForSeconds(showAnimationObjectDelay);
        animationObject.SetActive(true);
        animator.Play("Scene");
        yield return new WaitForSeconds(destroyAnimationObjectDelay);
        
        foreach (Transform child in animationObject.transform)
        {
            if(child.tag == "FightAnimationDestroyMe")
            {
                Destroy(child.gameObject);
            }
        }
    }
}
