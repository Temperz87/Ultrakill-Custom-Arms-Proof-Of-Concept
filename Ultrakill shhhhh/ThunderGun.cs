using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThundergunShockWave : MonoBehaviour
{
    private List<Collider> hitColliders = new List<Collider>();

    private void OnCollisionEnter(Collision collision)
    {
        this.CheckCollision(collision.collider);
    }
    private void OnTriggerEnter(Collider collision)
    {
        this.CheckCollision(collision);
    }
    private void CheckCollision(Collider col)
    {
        if (col.gameObject.layer == 14)
        {
            EnemyIdentifierIdentifier enemyIdentifierIdentifier = col.gameObject.GetComponent<EnemyIdentifierIdentifier>();
            if (enemyIdentifierIdentifier != null && enemyIdentifierIdentifier.eid != null)
            {
                Rigidbody body = enemyIdentifierIdentifier.GetComponentInChildren<Rigidbody>(true);
                if (!hitColliders.Contains(col) && body)
                {
                    hitColliders.Add(col);
                    body.AddForceAtPosition(new Vector3(base.transform.position.x - enemyIdentifierIdentifier.transform.position.x, 0f, base.transform.position.z - enemyIdentifierIdentifier.transform.position.z) * 999f, col.transform.position);
                }
            }
        }
        return;
    }
}