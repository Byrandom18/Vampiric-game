using System.Collections.Generic;
using UnityEngine;

public class SlimeDeath : MonoBehaviour
{
    [SerializeField] private bool canDoppel = false;
    public List<GameObject> summons = new List<GameObject>();
    
    public void Death()
    {
        if (canDoppel)
        {
            if (summons.Count == 0)
            {
                Debug.LogWarning("Список префабов пуст!");
                return;
            }
            
            int randomIndex = Random.Range(0, summons.Count);
            Instantiate(summons[randomIndex], transform.position + new Vector3(0.25f, 0f, 0f), Quaternion.identity);
            Instantiate(summons[randomIndex], transform.position + new Vector3(-0.25f, 0f, 0f), Quaternion.identity);
        }
        
    }
}
