using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : SceneSingleton<EffectManager>
{
    public float EffectSpeed;
    
    public GameObject[] particlePrefabs; // 파티클 프리팹 배열
    private Dictionary<int, Queue<ParticleSystem>> particlePools = new Dictionary<int, Queue<ParticleSystem>>(); // 인스턴스화 된 파티클 시스템 풀링
    private Dictionary<ParticleSystem, int> particleToIndexMap = new Dictionary<ParticleSystem, int>(); // 파티클 시스템과 인덱스를 매핑

    void Awake()
    {
        InitializeParticleSystems();
    }
    
    private void InitializeParticleSystems() // 파티클 시스템 오브젝트 풀링
    {
        particlePools.Clear(); // 기존 파티클 시스템 풀링 초기화
        particleToIndexMap.Clear(); // 파티클 시스템 인덱스 맵 초기화

        for (int i = 0; i < particlePrefabs.Length; i++)
        {
            Queue<ParticleSystem> pool = new Queue<ParticleSystem>(); // 파티클 시스템 변수 선언
            for (int j = 0; j < 5; j++) // 하나의 파티클 프리팹 종류에 5개의 시스템을 미리 만들어서 풀해둔다
            {
                ParticleSystem ps = Instantiate(particlePrefabs[i]).GetComponent<ParticleSystem>(); // 특정 파티클 프리팹의 시스템 생성
                ps.Stop(); // 재생을 멈춰두고
                ps.gameObject.SetActive(false); // 꺼놔서 리소스 덜 잡아먹게 최적화
                pool.Enqueue(ps); // 큐에 생성된 파티클 저장
                particleToIndexMap[ps] = i; // 파티클 시스템과 인덱스를 매핑
            }
            particlePools.Add(i, pool); // 딕셔너리에 저장
        }
    }
    
    public void PlayEffect(int index, Vector3 position) // 오브젝트 위치에 이펙트 생성
    {
        if (particlePools.ContainsKey(index))
        {
            ParticleSystem ps = GetPooledParticleSystem(index);
            if (ps != null)
            {
                ps.transform.position = position;
                ps.gameObject.SetActive(true);
                ps.Play();
                StartCoroutine(DeactivateAfterPlay(ps));
            }
            else
            {
                Debug.LogError("particle system error");
            }
        }
        else
        {
            Debug.LogError("particle system error");
        }
    }
    
    public void PlayEffectRotation(int index, Vector3 position, Transform target) // 오브젝트 위치에 이펙트 생성, 타겟을 향해 방향 설정
    {
        if (particlePools.ContainsKey(index))
        {
            ParticleSystem ps = GetPooledParticleSystem(index);
            if (ps != null)
            {
                ps.transform.position = position;
                ps.gameObject.SetActive(true);
                ps.Play();

                // 타겟을 지속적으로 추적하는 코루틴 시작
                StartCoroutine(UpdateRotation(ps, target));

                StartCoroutine(DeactivateAfterPlay(ps));
            }
            else
            {
                Debug.LogError("particle system error");
            }
        }
        else
        {
            Debug.LogError("particle system error");
        }
    }
    
    private IEnumerator UpdateRotation(ParticleSystem ps, Transform target) // 타겟 이동에 따라 로테이션을 변경하는 코루틴
    {
        while (ps.isPlaying)
        {
            if (target != null)
            {
                // 타겟을 향한 방향 계산
                Vector3 direction = (target.position - ps.transform.position).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction);

                // 파티클 시스템의 회전 업데이트
                ps.transform.rotation = rotation;
            }

            // 매 프레임마다 회전 업데이트
            yield return null;
        }
    }
    
    public GameObject PlayEffectPositionUp(int index, Vector3 position, int up) // 스턴 상태일 때, 유닛 위에 표시될 이펙트. 스턴이 끝날 때 비활성화 되어야 함으로 return 값으로 해당 파티클 시스템을 다룰 변수에 저장
    {
        if (particlePools.ContainsKey(index))
        {
            ParticleSystem ps = GetPooledParticleSystem(index);
            if (ps != null)
            {
                Vector3 upPosition = new Vector3(position.x, position.y + up, position.z);
                ps.transform.position = upPosition;
                ps.gameObject.SetActive(true);
                ps.Play();
                return ps.gameObject; // 생성된 파티클 객체 반환
            }
            else
            {
                Debug.LogError("particle system error");
                return null;
            }
        }
        else
        {
            Debug.LogError("particle system error");
            return null;
        }
    }
    
    public void DeactivateEffect(GameObject particleObject) // 작동중인 파티클 끄는 메서드
    {
        ParticleSystem ps = particleObject.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop();
            ps.gameObject.SetActive(false);
            int index = particleToIndexMap[ps];
            particlePools[index].Enqueue(ps);
        }
    }
    
    // 이펙트가 지속적이라, 오브젝트를 따라다녔으면 좋겠을 때 사용하는 함수, 단 사용시 해당 오브젝트 스크립트에 게임오브젝트 형식의 변수를 추가해야함.
    public GameObject PlayEffectFollow(int index, Transform targetPosition, GameObject target)
    {
        if (particlePools.ContainsKey(index))
        {
            ParticleSystem ps = GetPooledParticleSystem(index);
            if (ps != null)
            {
                ps.transform.position = targetPosition.position;
                ps.gameObject.SetActive(true);
                ps.Play();
                StartCoroutine(FollowTarget(ps, targetPosition, target));
                return ps.gameObject;
            }
            else
            {
                Debug.LogError("No available particle system in pool for index: " + index);
                return null;
            }
        }
        else
        {
            Debug.LogError("Invalid index for particle system array");
            return null;
        }
    }

    // 타겟에게 이펙트를 발사하는 메서드
    public GameObject PlayEffectShot(int index, Vector3 playerPosition, Transform targetPosition)
    {
        if (particlePools.ContainsKey(index))
        {
            ParticleSystem ps = GetPooledParticleSystem(index); // 오브젝트 풀 형식으로 이펙트 미리 만들어서 저장해놓음
            if (ps != null)
            {
                ps.transform.position = playerPosition;
                ps.gameObject.SetActive(true);
                ps.Play();
                
                StartCoroutine(MoveEffect(ps, targetPosition));
                return ps.gameObject;
            }
            else
            {
                Debug.LogError("No available particle system in pool for index: " + index);
                return null;
            }
        }
        else
        {
            Debug.LogError("Invalid index for particle system array");
            return null;
        }
    }
    
    private ParticleSystem GetPooledParticleSystem(int index)
    {
        if (particlePools[index].Count > 0) // 딕셔너리가 비어있지 않다면
        {
            return particlePools[index].Dequeue(); // 해당 인덱스에 저장된 파티클 시스템 반환
        }
        else // 비어있다면 = 미리 저장해둔 파티클 시스템 다 썻다면
        {
            ParticleSystem ps = Instantiate(particlePrefabs[index]).GetComponent<ParticleSystem>();
            ps.Stop();
            ps.gameObject.SetActive(false);
            particleToIndexMap[ps] = index; // 새로 생성된 파티클 시스템과 인덱스를 매핑
            return ps; // 다시 만들어서 저장해두고 반환
        }
    }
    
    private IEnumerator DeactivateAfterPlay(ParticleSystem ps)
    {
        while (ps != null && ps.IsAlive(true)) // null 붙인이유 : 씬이 재시작되거나 넘어가면서 이전 씬에서 파괴된 파티클 시스템을 참조하려해서
        {
            yield return null;
        }

        if (ps != null) // 객체가 이미 파괴된 상태에서 해당 객체에 접근하는것을 방지
        {
            ps.Stop();
            ps.gameObject.SetActive(false);
            int index = particleToIndexMap[ps]; // 파티클 시스템의 인덱스를 맵에서 가져옴 //Array.IndexOf(particlePrefabs, ps.gameObject); 기존 사용하던 코드의 문제점 : ps.gameObject 는 인스턴스화 된 오브젝트이고 particlePrefabs 는 프리팹을 저장하고 있기 때문에 둘은 동일하지 않으므로 항상 -1을 반환하여서
            particlePools[index].Enqueue(ps);
        }
    }
    
    // 오브젝트 따라다니는 코루틴
    private IEnumerator FollowTarget(ParticleSystem ps, Transform targetPosition, GameObject target)
    {
        while (ps != null && ps.isPlaying) 
        {
            if (targetPosition != null)
            {
                Soldier targetType = target.GetComponent<Soldier>(); // 보스 타입의 경우 크기가 커 이펙트가 묻히는 현상을 방지하기 위해 position에 변화를 줌
                if (targetType.BossType == BossType.Regular)
                {
                    ps.transform.position = targetPosition.position;
                }
                else if (targetType.BossType == BossType.Boss)
                {
                    Vector3 bossTargetPosition = new Vector3(targetPosition.position.x, targetPosition.position.y + 5, targetPosition.position.z);
                    ps.transform.position = bossTargetPosition;
                }
            }
            else
            {
                break; // 타겟이 사라진 경우, 루프를 중단
            }
            
            yield return null;
        }

        if (ps != null)
        {
            ps.Stop();
            ps.gameObject.SetActive(false);
            int index = particleToIndexMap[ps];
            particlePools[index].Enqueue(ps);
        }
    }

    // 타겟 오브젝트를 추적하여 이동하는 코루틴
    private IEnumerator MoveEffect(ParticleSystem ps, Transform targetPosition)
    {
        while (ps != null && ps.isPlaying && targetPosition != null && Vector3.Distance(ps.transform.position, targetPosition.position) > 0.1f) // targetPosition 널 체크로 날아가는 중에 데미지 계산이 먼저 되어 타겟이 Destroy 되어도 문제없게 함
        {
            if (ps == null || targetPosition == null)
            {
                break;
            }
            Vector3 nextPosition = Vector3.MoveTowards(ps.transform.position, targetPosition.position, EffectSpeed * Time.deltaTime);
            ps.transform.position = nextPosition;
    
            yield return null;
        }
    
        if (ps != null)
        {
            if (targetPosition != null && Vector3.Distance(ps.transform.position, targetPosition.position) < 0.1f) // targetPosition 널 체크로 날아가는 중에 데미지 계산이 먼저 되어 타겟이 Destroy 되어도 문제없게 함
            {
                ps.transform.position = targetPosition.position; // 타겟에 정확히 맞춤
            }
            
            ps.Stop();
            ps.gameObject.SetActive(false);
            int index = particleToIndexMap[ps];
            particlePools[index].Enqueue(ps);
        }
    }
}
