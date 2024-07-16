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
    private float limitMax_X = 2.86f;//�ʱ� ������ġ�� �� ���� ���� �̵�,x ���ѰŸ���

    [Header("Enemy Info")]
    [SerializeField] private float maxHP = 100;
    [SerializeField] private float currentHP;
    [SerializeField] private float enemyPower = 1.2f;

    [Header("Attack")]
    [SerializeField]
    protected EnemyProjectile projectilePrefab; //�߻�ü ������
    [SerializeField]
    protected Transform projectileSpawnPoint; //�߻�ü ���� ��ġ
    public Transform attacktarget; //���� ���� ��� (�÷��̾�)

    [SerializeField] public bool attacking = false;
    [SerializeField]
    protected float attackRange = 5; //���� ���� (�� ���� �ȿ� ������ "Attack" ���·� ����)
    [SerializeField]
    private float attackDistance = 10;
    [SerializeField]
    protected float attackRate = 1; //���� �ӵ�
    protected float lastAttackTime = 0; //���� �ֱ� ���� ����

    public bool isDead = false;

    // Start is called before the first frame update

    private void Awake()
    {
        attacktarget = FindObjectOfType<Player>().transform;
    }
    private void Start()
    {
        //�ش� ��ü �����Ǿ����ÿ�
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
        //���� ��Ȱ��ȭ�� �� ���� ������� ���¸� �����ϰ�, ���¸� "None"���� ����
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
                //Ÿ�� ���� �ֽ�
                //LookRotationToTarget();
                if (Time.time - lastAttackTime > attackRate)
                {
                    attacking = true;

                    // RaycastHit hit;
                    //�����ֱⰡ �Ǿ� ������ �� �ֵ��� �ϱ� ���� ���� �ð� ����
                    lastAttackTime = Time.time;

                    //if(Physics.Raycast(transform.position,AttackDirection,out hit, attackRange, AttackLayer))
                    //{
                    if (GameManager.Instance.currentHP <= 0)
                    {
                        Debug.Log("ĳ���Ͱ� �����߿� �׾����� ������ �ߴ�!");
                        StopCoroutine("AttackExe");
                        AttackReset();
                        yield break;
                    }
                    Debug.Log("Enemy��� AttackExe �ڷ�ƾ ���ݽõ�>>" + attacktarget.position);
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
        GameManager.Instance.SetScore(1);//��������
        GameManager.Instance.killCount++;

        spriteRenderer.enabled = false;//�Ⱥ��̰�(spriteRender��ü ����)
        audioSource.Play();//�����ſ����
        col2D.enabled = false;//�ݸ�������
        moveSpeed = 0;
        Debug.Log("Enemy Dead�ÿ��� 1ȸ�� ���ؼ� SetScore+1,SetKillCount+1 ����");
        Destroy(gameObject,audioSource.clip.length);//�����ſ���� ��� ����� �ı�
        yield return null;
        //yield return new WaitForSeconds(audioSource.clip.length);
        //Debug.Log($"Enemy {audioSource.clip.length}�� �� ����");
    }
    IEnumerator Hit()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.02f);
        spriteRenderer.color = Color.white;
    }
    public void Damage(float _damage)
    {
        //���ʹ̰� �÷��̾�� �������� �浹�Ͽ� �¾�����
        Debug.Log($"Enemy Damaged by player enemyName:{transform.name}, Damage:{_damage}");
        currentHP -= _damage;

        StartCoroutine(Hit());//���󺯰�(SpriteRenderer)
        if(currentHP <= 0 && !isDead)
        {
            Debug.Log($"{transform.name} EnemyDied!");
            StartCoroutine(Dead());//���� ���� �ڷ�ƾ
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("�÷��̾� �����ġ�� �ش� ��ü���� �������� ����:" + transform.name + "=>Damaged:" + enemyPower);
            GameManager.Instance.Damage(enemyPower);//�÷��̾ ������ ����.
        }
    }
}
