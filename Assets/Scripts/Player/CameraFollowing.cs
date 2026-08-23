using UnityEngine;

public class CameraFollowing : MonoBehaviour
{
    [SerializeField] private Transform _player;

    private void Start()
    {
        if (_player == null)
        {
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                _player = playerObject.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (_player == null)
        {
            return;
        }

        Vector3 position = transform.position;
        position.x = _player.position.x;
        position.y = _player.position.y;
        transform.position = position;
    }
}
