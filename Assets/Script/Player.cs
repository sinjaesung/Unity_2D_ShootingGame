using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed;
    [Header("Limit")]
     private float limitMin_X = -2.85f;
     private float limitMax_X = 2.85f;
     private float limitMin_Y = -4.5f;
     private float limitMax_Y = 4.5f;

    [Header("Bullet")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private float delayTime = 0.3f;
    float timer = 0;
    [SerializeField] private GameObject autoBullet;
    [SerializeField] private float autoDelayTime = 0.3f;
    float autoTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Shot();
        AutoShot();
    }
    private void LateUpdate()
    {
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, limitMin_X, limitMax_X),Mathf.Clamp(transform.position.y,limitMin_Y,limitMax_Y),0);
    }
    void Movement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(x, y, 0);

        transform.position += move * moveSpeed * Time.deltaTime;
    }

    void Shot()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            timer += Time.deltaTime;
            if(timer > delayTime)
            {
                Instantiate(bullet, transform.position, Quaternion.identity);
                GameManager.Instance.PlayBulletSound();
                timer = 0;
            }      
        }
    }

    void AutoShot()
    {
        autoTimer += Time.deltaTime;

        if(autoTimer > autoDelayTime)
        {
            Instantiate(autoBullet, new Vector3(transform.position.x - 1, transform.position.y, transform.position.z), Quaternion.identity);
            Instantiate(autoBullet, new Vector3(transform.position.x + 1, transform.position.y, transform.position.z), Quaternion.identity);
            autoTimer = 0;
        }
    }
}
