using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private MovementTransform movement;
    public float projectileDistance = 30;//�߻�ü �ִ� �߻�Ÿ�
    [SerializeField] private int damage = 5;//�߻�ü ���ݷ�

    [SerializeField] private GameObject hit_effect_prefab;

    public void Awake()
    {
        
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    public void Setup(Vector3 position,float attack_distance)
    {
        movement = GetComponent<MovementTransform>();
        projectileDistance = attack_distance;
        // transform.LookAt(position);

        StartCoroutine("OnMove", position);
    }
    private void Update()
    {
        if (transform.position.y <= -999)
        {
            Debug.Log("EnemyProjectile ���� transformY��ġ�� -999���Ϸ� �͹��Ͼ��� �۰� ������ �ڽŻ���" + gameObject);
            //memoryPool.DeactivatePoolItem(gameObject);
            Destroy(gameObject);
        }
        float originfromDistance = Vector3.Magnitude(new Vector3(transform.position.x, transform.position.y, transform.position.z) - new Vector3(0, 0, 0));
        if (originfromDistance >= 99999)
        {
            Debug.Log("EnemyProjectile ���� transform��ġ�� �������κ��� �͹��Ͼ��� �ָ� �ڽŻ���" + gameObject + "transformposition:" + transform.position);
            //memoryPool.DeactivatePoolItem(gameObject);
            Destroy(gameObject);
        }
    }

    private IEnumerator OnMove(Vector3 targetPosition)
    {
        Vector3 start = transform.position;

        //transform.LookAt(targetPosition);//�ش� �ڽ���ġ���� ������ǥ Ư����ġ �ٶ󺸰��Ѵ�.
        movement.MoveTo((targetPosition - transform.position).normalized);

        while (true)
        {
            if(Vector3.Distance(transform.position,start) >= projectileDistance)
            {
                //Destroy(gameObject);//�߻�ü �߻��Ŀ� �߻�ü �̵��ȰŸ����� �ִ��Ÿ� �Ѿ�� �߻�ü ����
                if(transform != null)
                {
                    Debug.Log("EnemyProjectile]]�ִ��Ÿ� ����� ����");
                    Instantiate(hit_effect_prefab, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            Debug.Log("EnemyProjectileHit����" + collision.transform.name + "," + collision.transform.tag);

            if (collision.CompareTag("Player"))
            {
                // Debug.Log("EnemyProjectileHit" + other.transform.name + "," + transform+"monsterdamage:"+damage+",�÷��̾����:"+ playerdefense+",���뵥����:"+ (damage - playerdefense));
                //other.GetComponent<PlayerController>().TakeDamage(damage);
                //playerController.TakeDamage(damage);
                //HealthPlayer healthCom = playerController.GetComponent<HealthPlayer>();            GameManager.Instance.Damage(enemyPower);//�÷��̾ ������ ����.
                GameManager.Instance.Damage(damage);//�÷��̾ ������ ����.
                Debug.Log("EnemyProjectile OnTriggerEnter2D collision target:"+ collision.name);
                /*float takeDamage = damage - playerdefense;
                if (takeDamage <= 0)
                {
                    takeDamage = 0;
                }
                // Debug.Log("EnemyProjectileHit �������뵥����:" + takeDamage);
                healthCom.TakeDamage(takeDamage);*/

                //impactMemoryPool.SpawnImpact(other, transform);
                if (transform != null)
                {
                    // impactMemoryPool.OnSpawnImpact(hit_effect_prefab, transform.position, Quaternion.Inverse(transform.rotation), damage,true);
                    //impactMemoryPool.OnSpawnImpact(hit_effect_prefab, transform.position, Quaternion.identity, damage, true);
                    Instantiate(hit_effect_prefab, transform.position, Quaternion.identity);
                }
            }

            if (!collision.CompareTag("Enemy") && !collision.CompareTag("PlayerProjectile") && !collision.CompareTag("EnemyProjectile"))
            {
                // memoryPool.DeactivatePoolItem(gameObject);
                Debug.Log("Enemy��ҳ� Enemy,PlayerBullet�� �����ѰͿ� �ε������ enemyProjectile����");
                Destroy(gameObject);
            }
        }
    }
}
