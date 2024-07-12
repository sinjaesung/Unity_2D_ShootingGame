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

    // Start is called before the first frame update
    private void Start()
    {
        //해당 개체 생성되었을시에
        col2D = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHP = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += dir * moveSpeed * Time.deltaTime;
        float target_y = transform.position.y;
        target_y = Mathf.Clamp(target_y, limitMax_y + 1.5f, 4.5f);
        Debug.Log("Enemy position target_y:" + target_y);
        transform.position = new Vector3(transform.position.x, target_y, transform.position.z);
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
        spriteRenderer.enabled = false;//안보이게(spriteRender자체 제거)
        audioSource.Play();//적제거오디오
        col2D.enabled = false;//콜리젼제거
        moveSpeed = 0;
        Debug.Log("Enemy Dead");
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
        if(currentHP <= 0)
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
            //GameManager.Instance.Damage(enemyPower);//플레이어가 데미지 받음.
        }
    }
}
