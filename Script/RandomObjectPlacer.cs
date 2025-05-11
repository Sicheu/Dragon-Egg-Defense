using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RandomObjectPlacer : MonoBehaviour
{
    public GameObject[] objectsToSpawn; // 배치할 오브젝트 배열 (예: 6개의 오브젝트)
    public Vector2 xRange = new Vector2(-80, 80); // X축 범위
    public Vector2 zRange = new Vector2(-100, 100); // Z축 범위
    public Vector2 exclusionXRange = new Vector2(-40, 40); // 제외할 X 영역 (-40 ~ 40)
    public Vector2 exclusionZRange = new Vector2(-40, 40); // 제외할 Z 영역 (-40 ~ 40)
    public float fixedY = 0f; // 고정된 Y 좌표 값
    public int numberObj = 100; // 배치할 총 오브젝트 수 (예: 100)

    // 이 함수는 에디터에서 호출되어 오브젝트를 배치합니다.
    public void SpawnObjectsInScene()
    {
        if (objectsToSpawn.Length == 0)
        {
            Debug.LogWarning("배치할 오브젝트가 없습니다.");
            return;
        }

        // 각 오브젝트가 몇 개씩 배치될지 결정할 리스트
        int[] objectCounts = new int[objectsToSpawn.Length];

        // 남은 오브젝트 수를 추적
        int remainingObjects = numberObj;

        // 1. 각 오브젝트에 배치할 개수를 랜덤으로 할당
        for (int i = 0; i < objectsToSpawn.Length - 1; i++)
        {
            // 각 오브젝트에 랜덤한 개수 할당 (남은 오브젝트 개수 내에서)
            objectCounts[i] = Random.Range(0, remainingObjects + 1);
            remainingObjects -= objectCounts[i];
        }
        // 마지막 오브젝트에 남은 모든 개수 할당
        objectCounts[objectsToSpawn.Length - 1] = remainingObjects;

        // 2. 오브젝트를 각자 할당된 개수만큼 배치
        for (int i = 0; i < objectsToSpawn.Length; i++)
        {
            for (int j = 0; j < objectCounts[i]; j++)
            {
                Vector3 randomPosition = GenerateRandomPosition();

                // 씬에 오브젝트 배치
                GameObject spawnedObject = (GameObject)PrefabUtility.InstantiatePrefab(objectsToSpawn[i]);
                spawnedObject.transform.position = randomPosition;
            }
        }

        // 결과 출력 (디버그용)
        for (int i = 0; i < objectCounts.Length; i++)
        {
            Debug.Log($"{i}번 오브젝트: {objectCounts[i]}개 배치");
        }
    }

    // 제외된 영역을 고려한 랜덤 위치 생성 함수
    private Vector3 GenerateRandomPosition()
    {
        Vector3 randomPosition;
        bool isInExclusionZone;

        do
        {
            // 랜덤한 위치를 생성
            randomPosition = new Vector3(
                Random.Range(xRange.x, xRange.y), // X 좌표 랜덤
                fixedY,                          // Y 좌표 고정
                Random.Range(zRange.x, zRange.y)  // Z 좌표 랜덤
            );

            // 생성된 위치가 제외 영역에 있는지 확인
            isInExclusionZone = (randomPosition.x > exclusionXRange.x && randomPosition.x < exclusionXRange.y) &&
                                (randomPosition.z > exclusionZRange.x && randomPosition.z < exclusionZRange.y);

        } while (isInExclusionZone); // 제외된 영역에 속하면 다시 위치 생성

        return randomPosition;
    }
}

[CustomEditor(typeof(RandomObjectPlacer))]
public class RandomObjectPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RandomObjectPlacer script = (RandomObjectPlacer)target;
        if (GUILayout.Button("Spawn Objects in Scene"))
        {
            script.SpawnObjectsInScene();
        }
    }
}
