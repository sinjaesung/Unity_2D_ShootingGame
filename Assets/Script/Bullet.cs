using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private Vector3 dir = Vector3.up;

    [SerializeField] private float limitMax_y = 5;
    [SerializeField] private float Damage=30f;

    [SerializeField] private GameObject explodePrefab;//bullet에 파티클을 단다.

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    private void LateUpdate()
    {
        if(transform.position.y >= limitMax_y)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Instantiate(explodePrefab, transform.position, Quaternion.identity);//폭발이펙트
            //StartCoroutine(collision.GetComponent<Enemy>().Dead());//Enemy Dead코루틴 실행
            collision.GetComponent<Enemy>().Damage(Damage);//탄환 데미지

            Destroy(this.gameObject);//Bullet자신오브젝트 삭제
        }
    }
}
