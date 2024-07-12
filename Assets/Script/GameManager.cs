using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player")]
    public GameObject player;
    [SerializeField] private float maxHP = 6000;
    [SerializeField] private float currentHP;
    private SpriteRenderer playerSpriteRenderer;

    [Header("EnemySpawner")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] public List<GameObject> enemyList = new List<GameObject>();
    public bool isSpawnAble = false;
    //true이면 EnemySpawner 코루틴(계속 존재) 코루틴자체는 중지없이 계속 백그라운드
    //EnemySpawner 게임오브젝트 자체의 비활성,활성은 가능하다.

    [Header("Level")]
    [SerializeField]
    int[] levelArray = new int[5]
    {
        50,
        100,
        150,
        200,
        300
    }; //레벨당 몇 개체수씩 생성할지 여부
    [SerializeField]
    int[] levelPerTypeCount = new int[5]
    {
        1,
        2,
        2,
        3,
        3
    };
    public int LevelPerTypeCount
    {
        get
        {
            return levelPerTypeCount[level];//현재 레벨0,1,2,3,4,5,...에 따른 소환종류수
        }
    }

    [Header("UI")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text resultScoreText;
    [SerializeField] private TMP_Text resultLevelText;
    [SerializeField] private TMP_Text resultTimeText;
    [SerializeField] private GameObject introCanvas;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject resultCanvas;
    [SerializeField] private Button startBtn;
    [SerializeField] private Button endBtn;

    [Header("Audio")]
    [SerializeField] private AudioSource bgm;
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip[] bgms = new AudioClip[2];
    [SerializeField] private AudioClip[] sfxs = new AudioClip[2];//0:bullet,1:player Dead

    [Header("Element")]
    public int score=0;
    public int level=0;//0레벨이 1레벨
    public float time = 0;//게임 플레이 시간
    public bool isTimeCheck = false;

    // Start is called before the first frame update
    void Start()
    {
       Instance = this;

        currentHP = maxHP;
        hpSlider.maxValue = maxHP;

        UpdateHP();

        if (player != null)
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>();

        levelArray = new int[5] { 50, 100, 150, 200, 300 };

        InitializeGame();
    }

    //Update is called once per frame
    private void Update()
    {
        if (isTimeCheck)//게임오버전까지 계속 true
        {
            time += Time.deltaTime;//게임시작 이래로 계속 누적더해진다, 초단위의 deltaTime지난정도
            UpdateTime();
        }
    }
    #region UI
    public void UpdateHP()
    {
        hpText.text = "My HP : " + currentHP;
        hpSlider.value = currentHP;
    }
    public void UpdateScore()
    {
        scoreText.text = "Score : " + score;
    }
    public void UpdateLevel()
    {
        Debug.Log("UpdateLevel: now Level" + level + 1);
        levelText.text = "Level : " + (level+1);//몬스터 다 잡고,레벨업시에 적용
    }
    public void UpdateTime()
    {
        string hour = (Convert.ToInt32(time) / 3600).ToString();
        string min = (Convert.ToInt32(time) / 60 % 60).ToString();
        string sec = (Convert.ToInt32(time) % 60).ToString();

        timeText.text = hour + ":" + min + ":" + sec;
    }
    #endregion

    #region DamageRelated
    public void Damage(float _damage)
    {
        //플레이어가 적군에게 부딪혔을때
        currentHP -= _damage;

        UpdateHP();
        StartCoroutine(Hit());//색상변경(SpriteRenderer)
        if(currentHP <= 0)
        {
            Debug.Log("character die");
            GameOver();
        }
    }

    IEnumerator Hit()
    {
        playerSpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.02f);
        playerSpriteRenderer.color = Color.white;
    }
    #endregion

    #region ScoreRelated
    public int GetScore()
    {
        return score;//정수의 점수값(그동안의 점수)를 그냥 반환
    }

    public void SetScore(int _score)
    {
        score += _score;//점수값 갱신
        UpdateScore();
    }
    #endregion

   
    #region 레벨 관련 함수
    public int GetLeveL()
    {
        return level;//진행한 레벨값
    }

    public void SetLevel(int _level)
    {
        level = _level;//레벨값 지정.몬스터 다 잡아서(플레이어가 공격해서 잡은경우에만 최종삭제)
        //라운드 하나가 끝난경우에만 Level업
    }

    public float GetTime()
    {
        return time;//게임 첫 실행후부터 지금까지 누적 계산된 플레이경과시간
    }

    public void SetTime(float _time)
    {
        time = _time;
    }
    #endregion

    #region 적 관련 함수
    public void AddEnemyList(GameObject _enemy)
    {
        /*if (enemyList.Count >= levelArray[level])
        {
            //적군 생산 코루틴에서 level의 생산수만큼 그저 생산 완료되었는지 여부
            isSpawnAble = false;
            Debug.Log("EnemeyList add:" + enemyList.Count + "," + levelArray[level]);
            enemyList.Add(_enemy);
        }
        else {
            //적군 생산 코루틴에서 level의 생산수만큼 모두 생산 완료되었다면 관련 생산여부 플래그 생산중지.
            isSpawnAble = false;
            Debug.Log("EnemeyList full maximum: 다음라운드로!!" + enemyList.Count + "," + levelArray[level]);
            //50마리를 다 소환했다면 그냥 다음 라운드로 넘어가게
            SetLevel(level + 1);
            UpdateLevel();
        }*/
        Debug.Log("AddEnemyList Add요청:" + _enemy.transform.name);
        enemyList.Add(_enemy);

        if(enemyList.Count >= levelArray[level])
        {
            Debug.Log("EnemeyList full maximum: 다음라운드로!!" + enemyList.Count + "," + levelArray[level]);
            isSpawnAble = false;
            // StopCoroutine(enemySpawner.SpawnEnemy());
            Invoke("NextRoundMoveInvoke", 5f);
        }
    }
    #endregion

    public void NextRoundMoveInvoke()
    {
        Debug.Log("몬스터 모두 소환뒤에 5초 후에 다음라운드로 진출");
        enemyList.Clear();
        SetLevel(level + 1);
        isSpawnAble = true;
        UpdateLevel();
    }

    #region GameLogic

    public void GameReplay()
    {
        InitializeGame();
        GameStart();
    }
    void InitializeGame()
    {
        startBtn.onClick.RemoveListener(GameStart);
        startBtn.onClick.AddListener(GameStart);
        PlayBgm(bgms[0]);
        introCanvas.SetActive(true);//인트로화면열기
        mainCanvas.SetActive(false);//인게임화면닫기
        resultCanvas.SetActive(false);//게임오버화면닫기
        player.SetActive(false);//플레이어비활성화
        enemySpawner.gameObject.SetActive(false);//적생산공장(코루틴)비활성화

        currentHP = maxHP;
        hpSlider.maxValue = maxHP;

        UpdateHP();

        SetScore(0);
        SetTime(0);
        SetLevel(0);
        isTimeCheck = false;
    }
    public void GameStart()
    {
        Debug.Log("GameStart호출>");
        PlayBgm(bgms[1]);//bgm1실행
        introCanvas.SetActive(false);//인트로화면닫기
        mainCanvas.SetActive(true);//인게임화면열기
        player.SetActive(true);//플레이어활성화
        enemySpawner.gameObject.SetActive(true);//적생산공장(코루틴)활성화
        isTimeCheck = true;//타임체크

        isSpawnAble = true;
        StartCoroutine(enemySpawner.SpawnEnemy());

        UpdateLevel();
    }

    public void GameOver()
    {
        endBtn.onClick.AddListener(GameReplay);//게임 초기상태로
        PlayBgm(bgms[2]);
        PlaySFX(sfxs[1]);//player dead
        resultCanvas.SetActive(true);
        mainCanvas.SetActive(false);
        player.SetActive(false);
        enemySpawner.gameObject.SetActive(false);

        resultScoreText.text = GetScore().ToString();
        resultLevelText.text = GetLeveL().ToString();
        resultTimeText.text = GetTime().ToString();
        isTimeCheck = false;
    }

    #endregion

    #region SoundRelated
    void PlayBgm(AudioClip audioClip)
    {
        bgm.Stop();
        bgm.clip = audioClip;
        bgm.Play();
    }

    void PlaySFX(AudioClip _audioClip)
    {
        sfx.Stop();
        sfx.clip = _audioClip;
        sfx.Play();
    }

    public void PlayBulletSound()
    {
        sfx.Stop();
        sfx.clip = sfxs[0];
        sfx.Play();
    }
    #endregion
}
