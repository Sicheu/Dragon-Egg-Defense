using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 그리드 타일의 meterial 을 커서 위치에 따라 변경시키기 위한 매니저
public class MaterialManager : SceneSingleton<MaterialManager>
{
    public Material outlineMaterial; // 변경시킬 meterial
}