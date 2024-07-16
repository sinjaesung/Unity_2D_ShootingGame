using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private Vector3 dir = Vector3.down;

    [SerializeField] private float limitMax_y = -5;
    [SerializeField] private AudioSource audioSource;
    private CapsuleCollider2D col2D;
    private SpriteRenderer spriteRenderer;

    private float limitMin_X = -2.85f;
    private float limitMax_X = 2.86f;//초기 생성위치로 재 설정 랜덤 이동,x 제한거리값

    [Header("Enemy Info")]
    [SerializeField] private float maxHP = 100;
    [SerializeField] private float currentHP;
    [SerializeField] private float enemyPower = 1.2f;

    [Header("Attack")]
    [SerializeField]
    protected EnemyProjectile projectilePrefab; //발사체 프리팹
    [SerializeField]
    protected Transform projectileSpawnPoint; //발사체 생성 위치
    public Transform attacktarget; //적의 공격 대상 (플레이어)

    [SerializeField] public bool attacking = false;
    [SerializeField]
    protected float attackRange = 5; //공격 범위 (이 범위 안에 들어오면 "Attack" 상태로 변경)
    [SerializeField]
    private float attackDistance = 10;
    [SerializeField]
    protected float attackRate = 1; //공격 속도
    protected float lastAttackTime = 0; //공격 주기 계산용 변수

    public bool isDead = false;

    // Start is called before the first frame update

    private void Awake()
    {
        attacktarget = FindObjectOfType<Player>().transform;
    }
    private void Start()
    {
        //해당 개체 생성되었을시에
        col2D = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHP = maxHP;

        StopCoroutine("AttackExe");
        StartCoroutine("AttackExe");

    }
    private void AttackReset()
    {
        StopCoroutine("AttackExe");
    }
    private void OnDisable()
    {
        //적이 비활성화될 때 현재 재생중인 상태를 종료하고, 상태를 "None"으로 설정
        AttackReset();
        StopAllCoroutines();
        Debug.Log("Enemy OnDisable");
    }
    // Update is called once per frame
    void Update()
    {
        //transform.position += dir * moveSpeed * Time.deltaTime;
       // float target_y = transform.position.y;
       // target_y = Mathf.Clamp(target_y, limitMax_y + 1.5f, 4.5f);
        //Debug.Log("Enemy position target_y:" + target_y);
        //transform.position = new Vector3(transform.position.x, target_y, transform.position.z);
    }
    private IEnumerator AttackExe()
    {
        while (true)
        {
            if (attacktarget)
            {
                //타겟 방향 주시
                //LookRotationToTarget();
                if (Time.time - lastAttackTime > attackRate)
                {
                    attacking = true;

                    // RaycastHit hit;
                    //공격주기가 되야 공격할 수 있도록 하기 위해 현재 시간 저장
                    lastAttackTime = Time.time;

                    //if(Physics.Raycast(transform.position,AttackDirection,out hit, attackRange, AttackLayer))
                    //{
                    if (GameManager.Instance.currentHP <= 0)
                    {
                        Debug.Log("캐릭터가 공격중에 죽었으면 공격을 중단!");
                        StopCoroutine("AttackExe");
                        AttackReset();
                        yield break;
                    }
                    Debug.Log("Enemy요소 AttackExe 코루틴 공격시도>>" + attacktarget.position);
                    EnemyProjectile projectileObject = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
                    projectileObject.Setup((attacktarget.position), attackDistance);

                    // }

                }
            }         
            yield return null;
        }
    }
    private void LateUpdate()
    {
        if (transform.position.y < limitMax_y)
        {
            //Destroy(this.gameObject);
            float posX = Random.Range(limitMin_X, limitMax_X);
        }
    }

    public IEnumerator Dead()
    {
        isDead = true;
        GameManager.Instance.SetScore(1);//점수증가
        GameManager.Instance.killCount++;

        spriteRenderer.enabled = false;//안보이게(spriteRender자체 제거)
        audioSource.Play();//적제거오디오
        col2D.enabled = false;//콜리젼제거
        moveSpeed = 0;
        Debug.Log("Enemy Dead시에만 1회에 한해서 SetScore+1,SetKillCount+1 실행");
        Destroy(gameObject,audioSource.clip.length);//적제거오디오 모두 재생후 파괴
        yield return null;
        //yield return new WaitForSeconds(audioSource.clip.length);
        //Debug.Log($"Enemy {audioSource.clip.length}초 뒤 삭제");
    }
    IEnumerator Hit()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.02f);
        spriteRenderer.color = Color.white;
    }
    public void Damage(float _damage)
    {
        //에너미가 플레이어에게 볼릿으로 충돌하여 맞았을때
        Debug.Log($"Enemy Damaged by player enemyName:{transform.name}, Damage:{_damage}");
        currentHP -= _damage;

        StartCoroutine(Hit());//색상변경(SpriteRenderer)
        if(currentHP <= 0 && !isDead)
        {
            Debug.Log($"{transform.name} EnemyDied!");
            StartCoroutine(Dead());//적군 제거 코루틴
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("플레이어 몸통박치기 해당 개체에게 데미지를 받음:" + transform.name + "=>Damaged:" + enemyPower);
            GameManager.Instance.Damage(enemyPower);//플레이어가 데미지 받음.
        }
    }
}
