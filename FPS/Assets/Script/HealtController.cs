using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealtController : MonoBehaviour {

    [SerializeField] private float healt = 100f;

	public void ApplyDamage(float damage)
    {
        healt -= damage;

        if(healt <= 0)
        {
            Destroy(gameObject);
        }
    }
}
