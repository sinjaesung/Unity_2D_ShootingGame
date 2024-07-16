using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private Vector3 dir = Vector3.up;

    [SerializeField] private float limitMax_y = 5;
    [SerializeField] private float Damage=30f;

    [SerializeField] private GameObject explodePrefab;//bullet�� ��ƼŬ�� �ܴ�.

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
            Instantiate(explodePrefab, transform.position, Quaternion.identity);//��������Ʈ
            //StartCoroutine(collision.GetComponent<Enemy>().Dead());//Enemy Dead�ڷ�ƾ ����
            collision.GetComponent<Enemy>().Damage(Damage);//źȯ ������

            Destroy(this.gameObject);//Bullet�ڽſ�����Ʈ ����
        }
    }
}
