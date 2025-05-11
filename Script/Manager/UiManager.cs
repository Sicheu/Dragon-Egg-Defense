using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : SimpleSingleton<UiManager>
{
   public Button Spwan;
   public Button SuperiorSpwan;
   public Button SelectSpwan;
   public Button Sell;

   public Button Coin;
   public Button Life;
   public Button Jewel;

   public TextMeshProUGUI _coin;
   public TextMeshProUGUI _life;
   public TextMeshProUGUI _jewel;

   public int coin = 300;
   public int life = 10;
   public int jewel = 0;

   public float countdownTime = 30;
   private float currentTime;
   public TextMeshProUGUI timerText;

   public GameObject SelectSpwanWindow;
   public Button RedDragonSpawn;
   public Button GreenDragonSpawn;
   public Button DarkDragonSpawn;
   public Button SelectSpwanWindowClose;

   public GameObject GameOverWindow;
   public Button ReStart;

   //public List<GameObject> Stage; // 스테이지 담을 리스트
   private int StageCount = 0; // 스테이지 표시 관리할 변수
   public TextMeshProUGUI stageText;
   
   void Awake()
   {
      SelectSpwanWindow.SetActive(false);
      GameOverWindow.SetActive(false);
      
      StartCoroutine(PhaseCountdown());
      
      Spwan.onClick.AddListener(SpwanClicked);
      SuperiorSpwan.onClick.AddListener(SuperiorSpwanClicked);
      SelectSpwan.onClick.AddListener(SelectSpawnClicked);
      Sell.onClick.AddListener(SellClicked);
      RedDragonSpawn.onClick.AddListener(() => SelectDragonSpawn("Red"));
      GreenDragonSpawn.onClick.AddListener(() => SelectDragonSpawn("Green"));
      DarkDragonSpawn.onClick.AddListener(() => SelectDragonSpawn("Dark"));
      
      Coin.onClick.AddListener(CoinCheat);
      Life.onClick.AddListener(RegeneratingLifePoint);
      Jewel.onClick.AddListener(CreateJewel);
      
      //ReStart.onClick.AddListener(GameReStart);
      
      SelectSpwanWindowClose.onClick.AddListener(SelectSpawnCloseClicked);
   }
   
   // 스폰 & 셀
   void SpwanClicked() // 램덤 스폰 버튼
   {
      SoundManager.Instance.PlaySound(15);
      
      if (coin >= 100)
      {
         MyPlayerController.Instance.SpwanCharacter();
         coin -= 100;
      }
   }

   void SuperiorSpwanClicked() // 상위 유닛 스폰 버튼
   {
      SoundManager.Instance.PlaySound(15);
      MyPlayerController.Instance.SpwanSuperiorCharacter();
   }

   void SelectSpawnClicked() // SelectSpawn 버튼 눌렀을 때, UI 창 띄우는 메서드
   {
      SoundManager.Instance.PlaySound(15);
      SelectSpwanWindow.SetActive(true);
   }

   void SelectSpawnCloseClicked() // SelectSpawnWindow 닫는 버튼 눌렀을때, UI 창 비활성화
   {
      SoundManager.Instance.PlaySound(15);
      SelectSpwanWindow.SetActive(false);
   }

   void SelectDragonSpawn(string dragonType)
   {
      if (jewel > 0)
      {
         jewel -= 1;
         
         if (dragonType == "Red")
         {
            MyPlayerController.Instance.SelectSpwanCharacter(2);
            SoundManager.Instance.PlaySound(12);
         }
         else if (dragonType == "Green")
         {
            MyPlayerController.Instance.SelectSpwanCharacter(1);
            SoundManager.Instance.PlaySound(12);
         }
         else if (dragonType == "Dark")
         {
            MyPlayerController.Instance.SelectSpwanCharacter(0);
            SoundManager.Instance.PlaySound(12);
         }
      }
   }

   void SellClicked()
   {
      MyPlayerController.Instance.SellSelectedCharacter();
      SoundManager.Instance.PlaySound(12);
   }
   
   // 외부요소
   public void GetDamaged(int damage) // 둥지가 데미지를 받았음을 UI 에 알림. 동시에 Nest 겟데미지 메서드 작동.
   {
      life -= damage;
      Nest.Instance.GetDamaged(damage);
      SoundManager.Instance.PlaySound(13);
   }

   // 재화관리
   private void CoinCheat()
   {
      coin += 1000;
   }

   private void RegeneratingLifePoint()
   {
      if (coin >= 1000 && life < 10)
      {
         coin -= 1000;
         life++;
         Nest.Instance.RegeneratingLifePoint();
         SoundManager.Instance.PlaySound(14);
      }
   }

   private void CreateJewel()
   {
      if (coin >= 1000)
      {
         coin -= 1000;
         jewel++;
         SoundManager.Instance.PlaySound(12);
      }
   }
   
   // 타이머
   IEnumerator PhaseCountdown()
   {
      while (true)
      {
         currentTime = countdownTime;

         while (currentTime > 0)
         {
            currentTime -= Time.deltaTime; // 매 프레임마다 Time.deltaTime(마지막 프레임과 현재 프레임 사이의 시간) 을 빼줌으로써 실시간으로 시간이 감소
            UpdateTimerUI(currentTime); // 감소된 시간을 타이머UI 에 반영
            yield return null; // 다음 프레임까지 이 코루틴의 실행을 잠시 멈춤 = 타이머가 프레임별로 업데이트 됨
         }

         currentTime = 0; // 0이 되면서 while을 빠져나왔겠지만, 0으로 한 번 더 선언하면서 0에 도달했음을 보장
         UpdateTimerUI(currentTime); // 타이머 UI 에 반영

         StageCount++; // 스테이지 UI 변수 ++

         yield return new WaitForSeconds(0f); // 1초 대기 후 타이머 초기화하고 새로운 카운트 다운 코루틴 시작
      }
   }

   void UpdateTimerUI(float time)
   {
      int minutes = Mathf.FloorToInt(time / 60);
      int secounds = Mathf.FloorToInt(time % 60);
      timerText.text = string.Format("{0:00}:{1:00}", minutes, secounds);
   }
   
   //기타
   private void GameOver() // 게임오버 UI 를 활성화
   {
      GameOverWindow.SetActive(true);
      Invoke("PauseGame", 0.5f); // 알이 부서지는 애니메이션이 나온 이후 게임을 일시정지 하기 위해 Invoke 사용
   }

   private void PauseGame() // 게임을 일시정지하는 메서드
   {
      Time.timeScale = 0f;
   }

   private void GameReStart() // 씬을 재시작 !!! 제대로 씬이 로드되지 않는 버그 발견. 시간 남으면 해결할것
   {
      Scene currentScene = SceneManager.GetActiveScene();
      SceneManager.LoadScene(currentScene.name);
      Time.timeScale = 1f;
   }

   // private void StageSetting(int index)
   // {
   //    // 모든 Stage를 비활성화
   //    for (int j = 0; j < Stage.Count; j++)
   //    {
   //       Stage[j].SetActive(false);
   //    }
   //
   //    for (int i = 0; i < Stage.Count; i++)
   //    {
   //       if (index < 10)
   //       {
   //          Stage[0].SetActive(true);
   //          Stage[index+10].SetActive(true);
   //       }
   //       else if (index < 20)
   //       {
   //          Stage[1].SetActive(true);
   //          Stage[index].SetActive(true);
   //       }
   //       else if (index < 30)
   //       {
   //          Stage[2].SetActive(true);
   //          Stage[index - 10].SetActive(true);
   //       }
   //       else if (index == 30)
   //       {
   //          Stage[3].SetActive(true);
   //          Stage[10].SetActive(true);
   //       }
   //    }
   //    
   // }

   private void StageSetting(int index)
   {
      if (index < 9)
      {
         stageText.text = $"Stage 0{1+StageCount}";
      }
      else if (index < 19)
      {
         stageText.text = $"Stage 1{1+StageCount-10}";
      }
      else if (index < 29)
      {
         stageText.text = $"Stage 2{1+StageCount-20}";
      }
      else if (index == 29)
      {
         stageText.text = "Stage 30";
      }
   }
   
   // 업데이트
   private void Update()
   {
      UpdateData(); // 데이터 업데이트
      StageSetting(StageCount); // 스테이지 UI 업데이트

      if (life <= 0)
      {
         GameOver();
      }
   }
   
   void UpdateData() // 외부요소에 의해 바뀐 변수들을 각 버튼의 TMP 에 적용
   {
      _coin.text = coin.ToString();
      _life.text = life.ToString();
      _jewel.text = jewel.ToString();
   }
}
