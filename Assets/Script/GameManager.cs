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
    //true�̸� EnemySpawner �ڷ�ƾ(��� ����) �ڷ�ƾ��ü�� �������� ��� ��׶���
    //EnemySpawner ���ӿ�����Ʈ ��ü�� ��Ȱ��,Ȱ���� �����ϴ�.

    [Header("Level")]
    [SerializeField]
    int[] levelArray = new int[5]
    {
        10,
        10,
        15,
        10,
        20
    }; //������ �� ��ü���� �������� ����
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
            return levelPerTypeCount[level];//���� ����0,1,2,3,4,5,...�� ���� ��ȯ������      
        }
    }
    public Dictionary<int, bool> levelPerClear = new Dictionary<int, bool>();
    public int levelPerMonsterCount = 0;
    public int LevelAmountCatchMonsterCount = 0;
    public int killCount = 0;
    //0+10*1+10*2+15*2+10*3+20*3 :������ ���� ��ƾ��ϴ� ������ Kills���� �޶�����,�� ���ǿ� ���յǾ� ������������

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
    public int level=0;//0������ 1����
    public float time = 0;//���� �÷��� �ð�
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
        //���� �ʻ��� ���� ��ü��
        var enemyList = FindObjectsOfType<Enemy>();

        return enemyList.Length;
    }

    //Update is called once per frame
    private void Update()
    {
        if (isTimeCheck)//���ӿ��������� ��� true
        {
            time += Time.deltaTime;//���ӽ��� �̷��� ��� ������������, �ʴ����� deltaTime��������
            UpdateTime();
        }

        if (level >= 4)
        {
           
            // var enemyList = FindObjectsOfType<Enemy>();
            Debug.Log("������ 5�� �Ѿ������� �ʻ� �����ִ� ������ 0������ ��� ���Žÿ��� ��������!" + NowEnemyCount());
            var enemyList = FindObjectsOfType<Enemy>();
            for (int e = 0; e < enemyList.Length; e++)
            {
                Debug.Log(e + "| ���� �ʻ��� ��� �����ִ� ����:" + enemyList[e].transform.name);
            }
            if(NowEnemyCount() == 0 && isGameEnd==false)
            {
                Debug.Log("����5 �Ѱ��,�ʻ��� ��� �� ���Žÿ� ��������");
                StopCoroutine(enemySpawner.SpawnEnemy());
                StopAllCoroutines();//������ 5�� �Ѱ��, ��� �� �����(���� 0����)
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
            Debug.Log($"[[LevelPerMonsterAdaptCount]]���� ����{level}�� ������ ���� ��ƾ��� ���Ͱ�ü��:{LevelAmountCatchMonsterCount},����Kills��:{killCount}");

            if (killCount >= LevelAmountCatchMonsterCount && levelPerClear.ContainsKey(level) == false)
            {
                //����1Ŭ����,����2����(����2����ų������&levelPerClear��ųʸ���2Űclear���ΰ� ���� Add���� ��������
                //Contains�����ʰ��ִ� �����ΰ�쿡�� Add����,����������=>����3���� �������,��ȯ�ڷ�ƾ�����
                //����ų��>=������ ���� ����catch�� ��������&&����3 ���� ��Ŭ����(�� Add)�ΰ�쿡�� ���� ��������
                //level3 �̹� Added�Ǿ��ִ� ��� �����ϰ�,���� �߰��Ǿ����� ������쿡�� ���ñ������� ->����4����
                //level5:4index Level������ �����߰�,�ʻ� ��� ���� 0�� �����ΰ�� Update��׶��忡�� GameOveró��.
                levelPerClear.Add(level, true);
                //����1>=a ���Ǻ��ս÷�����,����2>=b ���Ǻ��ս� ������
                Debug.Log($"[[LevelPerMonsterAdaptCount]]���� ����{level},{killCount}/{LevelAmountCatchMonsterCount} ���� ���սÿ� ����������");
                //���� ��ȯ�� ������ AddEnemyList�Լ����� ��ü�� ���޿Ϸ�ÿ� �̹� �������
                NextRoundMoveInvoke();
            }
            else if (killCount >= LevelAmountCatchMonsterCount &&  levelPerClear.ContainsKey(level) == true)
            {
                Debug.Log($"[[LevelPerMonsterAdaptCount]]���� ����{level} {levelPerClear[level]} ���� ������ �̹� Added�Ǿ��ִ� ���·� �ش緹��Ŭ������� ������� �̹� �޼�����");
            }

            yield return new WaitForSeconds(0.4f);
        }
    }
    public void NextRoundMoveInvoke()
    {
        if (level < 6)
        {
            Debug.Log("���� ������ ��Ȳ�� ���� ���Ͱ�ü�� ��� ���� �Ŀ� ��������� ����");
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
        levelText.text = "Level : " + (level+1);//���� �� ���,�������ÿ� ����
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
        //�÷��̾ �������� �ε�������
        currentHP -= _damage;

        UpdateHP();
        StartCoroutine(Hit());//���󺯰�(SpriteRenderer)
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
        return score;//������ ������(�׵����� ����)�� �׳� ��ȯ
    }

    public void SetScore(int _score)
    {
        score += _score;//������ ����
        UpdateScore();
    }
    #endregion

   
    #region ���� ���� �Լ�
    public int GetLeveL()
    {
        return level;//������ ������
    }

    public void SetLevel(int _level)
    {
        level = _level;//������ ����.���� �� ��Ƽ�(�÷��̾ �����ؼ� ������쿡�� ��������)
        //���� �ϳ��� ������쿡�� Level��
        level = level >= 5 ? 4 : level;//�ִ밪4
    }

    public float GetTime()
    {
        return time;//���� ù �����ĺ��� ���ݱ��� ���� ���� �÷��̰���ð�
    }

    public void SetTime(float _time)
    {
        time = _time;
    }
    #endregion

    #region �� ���� �Լ�
    public void AddEnemyList(GameObject _enemy)
    {
        /*if (enemyList.Count >= levelArray[level])
        {
            //���� ���� �ڷ�ƾ���� level�� �������ŭ ���� ���� �Ϸ�Ǿ����� ����
            isSpawnAble = false;
            Debug.Log("EnemeyList add:" + enemyList.Count + "," + levelArray[level]);
            enemyList.Add(_enemy);
        }
        else {
            //���� ���� �ڷ�ƾ���� level�� �������ŭ ��� ���� �Ϸ�Ǿ��ٸ� ���� ���꿩�� �÷��� ��������.
            isSpawnAble = false;
            Debug.Log("EnemeyList full maximum: ���������!!" + enemyList.Count + "," + levelArray[level]);
            //50������ �� ��ȯ�ߴٸ� �׳� ���� ����� �Ѿ��
            SetLevel(level + 1);
            UpdateLevel();
        }*/
        Debug.Log("AddEnemyList Add��û:" + _enemy.transform.name);
        enemyList.Add(_enemy);

        if(enemyList.Count >= levelArray[level])
        {
            Debug.Log("EnemeyList full maximum: ��ȯ���߱�!!" + enemyList.Count + "," + levelArray[level]);
            isSpawnAble = false;
            StopCoroutine(enemySpawner.SpawnEnemy());
        }
    }
    #endregion
   
    #region GameLogic

    public void GameReplay()
    {
        Debug.Log("���Ӹ��÷��� ȣ��");
        //InitializeGame();
        //GameStart();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void InitializeGame()
    {
        isGameEnd = false;
        killCount = 0;
        Debug.Log("InitializeGame ȣ��");
        Time.timeScale = 1;

        StopAllCoroutines();//���� ù �ʱ�ȭ�ÿ� ��� �ڷ�ƾ ���� �ʱ�ȭ.
        startBtn.onClick.RemoveListener(GameStart);
        startBtn.onClick.AddListener(GameStart);

        PlayBgm(bgms[0]);
        introCanvas.SetActive(true);//��Ʈ��ȭ�鿭��
        mainCanvas.SetActive(false);//�ΰ���ȭ��ݱ�
        resultCanvas.SetActive(false);//���ӿ���ȭ��ݱ�
        player.SetActive(false);//�÷��̾��Ȱ��ȭ
        enemySpawner.gameObject.SetActive(false);//���������(�ڷ�ƾ)��Ȱ��ȭ

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
        Debug.Log("GameStartȣ��>");
        PlayBgm(bgms[1]);//bgm1����
        introCanvas.SetActive(false);//��Ʈ��ȭ��ݱ�
        mainCanvas.SetActive(true);//�ΰ���ȭ�鿭��
        player.SetActive(true);//�÷��̾�Ȱ��ȭ
        enemySpawner.gameObject.SetActive(true);//���������(�ڷ�ƾ)Ȱ��ȭ
        isTimeCheck = true;//Ÿ��üũ

        isSpawnAble = true;
        StartCoroutine(enemySpawner.SpawnEnemy());//���� ��ȯ �ڷ�ƾ
        StartCoroutine(LevelPerMonsterAdaptCount());//������ Ŭ���� ���� ��ȸ

        UpdateLevel();
    }

    public void GameOver()
    {
        isGameEnd = true;
        Debug.Log("GameOverȣ��");
        StopAllCoroutines();//��� �ڷ�ƾ ����
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
