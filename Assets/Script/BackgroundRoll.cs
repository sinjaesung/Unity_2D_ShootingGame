using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundRoll : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float scrollRange = 19f;//오브젝트끼리의 차이값
    [SerializeField] private float moveSpeed;

    Vector3 dir = Vector3.down;
    Vector3 originPos;

    // Start is called before the first frame update
    void Start()
    {
        originPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += dir * moveSpeed * Time.deltaTime;

        if (transform.position.y <= (originPos.y-scrollRange))
            transform.position = originPos;
    }
}
