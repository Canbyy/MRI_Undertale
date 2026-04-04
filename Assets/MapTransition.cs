using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundry;
    [SerializeField] Direction direction;
    CinemachineConfiner2D confiner;

    enum Direction { Up, Down, Left, Right }

    private void Awake()
    {
        confiner = FindAnyObjectByType<CinemachineConfiner2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            confiner.BoundingShape2D = mapBoundry;
            UpdatePlayerPosition(collision.gameObject);

            MapController_Manual.Instance?.HighlightArea(mapBoundry.name);
        }
    }

    void UpdatePlayerPosition(GameObject player)
    {
        Vector3 additivePos = player.transform.position;

        switch (direction)
        {
            case Direction.Up:
                additivePos.y += 2;
                break;
            case Direction.Down:
                additivePos.y += -2;
                break;
            case Direction.Left:
                additivePos.x += -2;
                break;
            case Direction.Right:
                additivePos.x += 2;
                break;
        }

        player.transform.position = additivePos;
    }
}
