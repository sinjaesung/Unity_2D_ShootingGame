using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyList;//0,1,2 ������
    //����0:enemy1�� 50���� 1
    //����1:enemy1,2 100,100���� 2
    //����2:enemy1,2 150,150���� 2
    //����3:enemy1,2,3 150,150,150���� 3
    //����4:enemy1,2,3 200,200,200���� 3

    [SerializeField] private float spawnTime;
    [SerializeField] private float posY;

    private float limitMin_X = -2.85f;
    private float limitMax_X = 2.86f;

    // Start is called before the first frame update
   
   public IEnumerator SpawnEnemy()
    {
        //���������� ��������?? �ʱ� �� �������Ŀ� ��� ���Ǹ� �´ٸ� ��׶��� ����
        while (GameManager.Instance.isSpawnAble)
        {
            Debug.Log("���� ������ ����PerTypeCount:" + GameManager.Instance.level + "," + GameManager.Instance.LevelPerTypeCount);
            GameObject enemyGameObjectSample=null;
            for(int l=0; l<GameManager.Instance.LevelPerTypeCount; l++)
            {
                var enemyGameObject = enemyList[l];
                Debug.Log(l + "| �������ʹ� ��ü:" + enemyGameObject.transform.name);
                float posX = Random.Range(limitMin_X, limitMax_X);

                Vector3 position = new Vector3(posX, posY, 0);
                GameObject enemyObject = Instantiate(enemyGameObject, position, Quaternion.identity);
                enemyGameObjectSample = enemyGameObject;
            }
            Debug.Log("�߰��� enemyGameObjectSample:" + enemyGameObjectSample.transform.name);
            GameManager.Instance.AddEnemyList(enemyGameObjectSample);//LevelPerTypeCount���� ���� ������� �׻� �������� ��� �߰�

            yield return new WaitForSeconds(spawnTime);//-2.85~2.85
        }
    }
}
