using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private MovementTransform movement;
    public float projectileDistance = 30;//발사체 최대 발사거리
    [SerializeField] private int damage = 5;//발사체 공격력

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
            Debug.Log("EnemyProjectile 현재 transformY위치가 -999이하로 터무니없이 작게 나오면 자신삭제" + gameObject);
            //memoryPool.DeactivatePoolItem(gameObject);
            Destroy(gameObject);
        }
        float originfromDistance = Vector3.Magnitude(new Vector3(transform.position.x, transform.position.y, transform.position.z) - new Vector3(0, 0, 0));
        if (originfromDistance >= 99999)
        {
            Debug.Log("EnemyProjectile 현재 transform위치가 원점으로부터 터무니없이 멀면 자신삭제" + gameObject + "transformposition:" + transform.position);
            //memoryPool.DeactivatePoolItem(gameObject);
            Destroy(gameObject);
        }
    }

    private IEnumerator OnMove(Vector3 targetPosition)
    {
        Vector3 start = transform.position;

        //transform.LookAt(targetPosition);//해당 자신위치에서 월드좌표 특정위치 바라보게한다.
        movement.MoveTo((targetPosition - transform.position).normalized);

        while (true)
        {
            if(Vector3.Distance(transform.position,start) >= projectileDistance)
            {
                //Destroy(gameObject);//발사체 발사후에 발사체 이동된거리량이 최대사거리 넘어서면 발사체 삭제
                if(transform != null)
                {
                    Debug.Log("EnemyProjectile]]최대사거리 벗어나면 삭제");
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
            Debug.Log("EnemyProjectileHit형태" + collision.transform.name + "," + collision.transform.tag);

            if (collision.CompareTag("Player"))
            {
                // Debug.Log("EnemyProjectileHit" + other.transform.name + "," + transform+"monsterdamage:"+damage+",플레이어방어력:"+ playerdefense+",적용데미지:"+ (damage - playerdefense));
                //other.GetComponent<PlayerController>().TakeDamage(damage);
                //playerController.TakeDamage(damage);
                //HealthPlayer healthCom = playerController.GetComponent<HealthPlayer>();            GameManager.Instance.Damage(enemyPower);//플레이어가 데미지 받음.
                GameManager.Instance.Damage(damage);//플레이어가 데미지 받음.
                Debug.Log("EnemyProjectile OnTriggerEnter2D collision target:"+ collision.name);
                /*float takeDamage = damage - playerdefense;
                if (takeDamage <= 0)
                {
                    takeDamage = 0;
                }
                // Debug.Log("EnemyProjectileHit 최종적용데미지:" + takeDamage);
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
                Debug.Log("Enemy요소나 Enemy,PlayerBullet을 제외한것에 부딪힌경우 enemyProjectile삭제");
                Destroy(gameObject);
            }
        }
    }
}
