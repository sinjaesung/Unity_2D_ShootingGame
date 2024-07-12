using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyList;//0,1,2 세종류
    //레벨0:enemy1만 50마리 1
    //레벨1:enemy1,2 100,100마리 2
    //레벨2:enemy1,2 150,150마리 2
    //레벨3:enemy1,2,3 150,150,150마리 3
    //레벨4:enemy1,2,3 200,200,200마리 3

    [SerializeField] private float spawnTime;
    [SerializeField] private float posY;

    private float limitMin_X = -2.85f;
    private float limitMax_X = 2.86f;

    // Start is called before the first frame update
   
   public IEnumerator SpawnEnemy()
    {
        //생성가능한 상태인지?? 초기 앱 실행이후에 계속 조건만 맞다면 백그라운드 실행
        while (GameManager.Instance.isSpawnAble)
        {
            Debug.Log("현재 레벨과 레벨PerTypeCount:" + GameManager.Instance.level + "," + GameManager.Instance.LevelPerTypeCount);
            GameObject enemyGameObjectSample=null;
            for(int l=0; l<GameManager.Instance.LevelPerTypeCount; l++)
            {
                var enemyGameObject = enemyList[l];
                Debug.Log(l + "| 생성에너미 개체:" + enemyGameObject.transform.name);
                float posX = Random.Range(limitMin_X, limitMax_X);

                Vector3 position = new Vector3(posX, posY, 0);
                GameObject enemyObject = Instantiate(enemyGameObject, position, Quaternion.identity);
                enemyGameObjectSample = enemyGameObject;
            }
            Debug.Log("추가할 enemyGameObjectSample:" + enemyGameObjectSample.transform.name);
            GameManager.Instance.AddEnemyList(enemyGameObjectSample);//LevelPerTypeCount개수 여부 상관없이 항상 한종류씩 취급 추가

            yield return new WaitForSeconds(spawnTime);//-2.85~2.85
        }
    }
}
