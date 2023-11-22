using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectIdle : MonoBehaviour
{
    float changeIdleTime = 10.5f;
    Animator animator;
    Coroutine coroutine;

    // Start is called before the first frame update

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        coroutine =  StartCoroutine(ChangeIdle());
    }

    void OnDisable()
    {
        StopCoroutine(coroutine);
    }

    IEnumerator ChangeIdle()
    {
        while(true)
        {
            yield return new WaitForSeconds(changeIdleTime);

            if(animator!=null)
            {
                animator.SetTrigger("ChangeIdle");
            }

        }
    }

}
