using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player")]
    public GameObject player;
    [SerializeField] private float maxHP = 6000;
    [SerializeField] public float currentHP;
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
        10,
        10,
        15,
        10,
        20
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
    public Dictionary<int, bool> levelPerClear = new Dictionary<int, bool>();
    public int levelPerMonsterCount = 0;
    public int LevelAmountCatchMonsterCount = 0;
    public int killCount = 0;
    //0+10*1+10*2+15*2+10*3+20*3 :레벨에 따라서 잡아야하는 누적된 Kills수가 달라지고,이 조건에 부합되야 다음레벨도달

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

    public bool isGameEnd = false;
    // Start is called before the first frame update
    void Start()
    {
       Instance = this;

        currentHP = maxHP;
        hpSlider.maxValue = maxHP;

        UpdateHP();

        if (player != null)
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>();

        levelArray = new int[5] { 10, 10, 15, 10, 20 };

        InitializeGame();
    }
    public int NowEnemyCount()
    {
        //현재 맵상의 몬스터 개체수
        var enemyList = FindObjectsOfType<Enemy>();

        return enemyList.Length;
    }

    //Update is called once per frame
    private void Update()
    {
        if (isTimeCheck)//게임오버전까지 계속 true
        {
            time += Time.deltaTime;//게임시작 이래로 계속 누적더해진다, 초단위의 deltaTime지난정도
            UpdateTime();
        }

        if (level >= 4)
        {
           
            // var enemyList = FindObjectsOfType<Enemy>();
            Debug.Log("레벨이 5를 넘었고현재 맵상에 남아있는 적들이 0마리로 모두 제거시에만 게임종료!" + NowEnemyCount());
            var enemyList = FindObjectsOfType<Enemy>();
            for (int e = 0; e < enemyList.Length; e++)
            {
                Debug.Log(e + "| 현재 맵상의 모든 남아있는 적들:" + enemyList[e].transform.name);
            }
            if(NowEnemyCount() == 0 && isGameEnd==false)
            {
                Debug.Log("레벨5 넘겼고,맵상의 모든 적 제거시에 게임종료");
                StopCoroutine(enemySpawner.SpawnEnemy());
                StopAllCoroutines();//레벨이 5를 넘겼고, 모든 적 섬멸시(적이 0마리)
                GameOver();
            }
        }
    }
    private IEnumerator LevelPerMonsterAdaptCount()
    {
        while (true)
        {
            LevelAmountCatchMonsterCount = 0;
            for (int e = 0; e <= level; e++)
            {
                levelPerMonsterCount = levelArray[e] * levelPerTypeCount[e];
                Debug.Log(e + $"Level Per MonsterCount:{levelPerMonsterCount}");
                LevelAmountCatchMonsterCount += levelPerMonsterCount;
                Debug.Log(e + $"Level Per LevelAmountCatchMonsterCount:{LevelAmountCatchMonsterCount}");
            }
            Debug.Log($"[[LevelPerMonsterAdaptCount]]현재 레벨{level}과 레벨에 따른 잡아야할 몬스터개체수:{LevelAmountCatchMonsterCount},누적Kills수:{killCount}");

            if (killCount >= LevelAmountCatchMonsterCount && levelPerClear.ContainsKey(level) == false)
            {
                //레벨1클리어,레벨2도달(레벨2누적킬수도달&levelPerClear딕셔너리에2키clear여부가 아직 Add되지 않은상태
                //Contains하지않고있는 상태인경우에만 Add연산,레벨업실행=>레벨3도달 스폰어블,소환코루틴재실행
                //누적킬수>=레벨에 따른 누적catch수 도달조건&&레벨3 아직 미클리어(미 Add)인경우에만 관련 구문실행
                //level3 이미 Added되어있는 경우 무시하고,아직 추가되어있지 않은경우에만 관련구문실행 ->레벨4도달
                //level5:4index Level변수값 도달했고,맵상에 모든 적들 0인 상태인경우 Update백그라운드에서 GameOver처리.
                levelPerClear.Add(level, true);
                //레벨1>=a 조건부합시레벨업,레벨2>=b 조건부합시 레벨업
                Debug.Log($"[[LevelPerMonsterAdaptCount]]현재 레벨{level},{killCount}/{LevelAmountCatchMonsterCount} 조건 부합시에 레벨업진행");
                //몬스터 소환은 레벨별 AddEnemyList함수에서 개체수 도달완료시에 이미 종료상태
                NextRoundMoveInvoke();
            }
            else if (killCount >= LevelAmountCatchMonsterCount &&  levelPerClear.ContainsKey(level) == true)
            {
                Debug.Log($"[[LevelPerMonsterAdaptCount]]현재 레벨{level} {levelPerClear[level]} 관련 데이터 이미 Added되어있는 상태로 해당레벨클리어관련 실행로직 이미 달성상태");
            }

            yield return new WaitForSeconds(0.4f);
        }
    }
    public void NextRoundMoveInvoke()
    {
        if (level < 6)
        {
            Debug.Log("현재 레벨별 상황에 따른 몬스터개체수 모두 섬멸 후에 다음라운드로 진출");
            enemyList.Clear();
            SetLevel(level + 1);
            isSpawnAble = true;
            StartCoroutine(enemySpawner.SpawnEnemy());
            UpdateLevel();

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
        level = level >= 5 ? 4 : level;//최대값4
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
            Debug.Log("EnemeyList full maximum: 소환멈추기!!" + enemyList.Count + "," + levelArray[level]);
            isSpawnAble = false;
            StopCoroutine(enemySpawner.SpawnEnemy());
        }
    }
    #endregion
   
    #region GameLogic

    public void GameReplay()
    {
        Debug.Log("게임리플레이 호출");
        //InitializeGame();
        //GameStart();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void InitializeGame()
    {
        isGameEnd = false;
        killCount = 0;
        Debug.Log("InitializeGame 호출");
        Time.timeScale = 1;

        StopAllCoroutines();//게임 첫 초기화시에 모든 코루틴 종료 초기화.
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
        enemyList.Clear();
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
        StartCoroutine(enemySpawner.SpawnEnemy());//적군 소환 코루틴
        StartCoroutine(LevelPerMonsterAdaptCount());//레벨별 클리어 여부 조회

        UpdateLevel();
    }

    public void GameOver()
    {
        isGameEnd = true;
        Debug.Log("GameOver호출");
        StopAllCoroutines();//모든 코루틴 종료
        enemyList.Clear();

        Time.timeScale = 0;
        endBtn.onClick.RemoveListener(GameReplay);
        endBtn.onClick.AddListener(GameReplay);
        PlayBgm(bgms[2]);
        PlaySFX(sfxs[1]);//player dead
        resultCanvas.SetActive(true);
        mainCanvas.SetActive(false);
        player.SetActive(false);
        enemySpawner.gameObject.SetActive(false);

        resultScoreText.text = "Score : " + GetScore().ToString();
        resultLevelText.text = "Level : "+(GetLeveL()+1).ToString();
        resultTimeText.text = "Play Time : "+GetTime().ToString();
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
