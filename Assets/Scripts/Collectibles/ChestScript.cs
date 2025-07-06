using System.Collections.Generic;
using UnityEngine;

public class ChestScript : MonoBehaviour
{
    public List<GameObject> prefabs = new List<GameObject>();


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Projectile targetScript = collision.GetComponent<Projectile>();
        if (targetScript != null)
        {
            if (!targetScript.enemyLaunch)
            {
                SpawnRandomPrefab();
                Destroy(gameObject);
            }
        }
        else if (collision.CompareTag("Player") && collision.isTrigger)
        {
            SpawnRandomPrefab();
            Destroy(gameObject);
        }
        
    }


    public void SpawnRandomPrefab()
    {
        if (prefabs.Count == 0)
        {
            Debug.LogWarning("Список префабов пуст!");
            return;
        }

        int randomIndex = Random.Range(0, prefabs.Count);
        Instantiate(prefabs[randomIndex], transform.position, Quaternion.identity);
    }
}
