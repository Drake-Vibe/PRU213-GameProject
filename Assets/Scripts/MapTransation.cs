using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapTransation : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundry;
    CinemachineConfiner2D confiner;
    [SerializeField] private Direction direction;
    [SerializeField] private float additivePos = 2f;
    enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private void Awake()
    {
        confiner = FindAnyObjectByType<CinemachineConfiner2D>();

        Debug.Log(gameObject.name + " -> " + mapBoundry.name);
}

        private void OnTriggerEnter2D(Collider2D collision)
        {
           if (collision.CompareTag("Player"))
        {   
        confiner.BoundingShape2D = mapBoundry;
        updatePlayerPosition(collision.gameObject);
        }
}
    private void updatePlayerPosition(GameObject player)
    {
        Vector3 newPosition = player.transform.position;

        switch (direction){
            case  Direction.Up:
                newPosition.y += additivePos;
                break;
            case Direction.Down:
                newPosition.y -= additivePos;
                break;
            case Direction.Left:
                newPosition.x -= additivePos;
                break;
            case Direction.Right:
                newPosition.x += additivePos;
                break;
        }

        player.transform.position = newPosition;
    } 
}




