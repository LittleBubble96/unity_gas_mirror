using UnityEditor;
using UnityEngine;

public class EffectBase : MonoBehaviour
{
#if UNITY_EDITOR
    
    private ParticleSystem m_ParticleSystem;
    /// <summary>
    /// 上一次系统时间
    /// </summary>
    private double m_PreviousTime;
    /// <summary>
    /// 当前运行时间
    /// </summary>
    private float m_RunningTime;
    
    public void OnInit()
    {
        m_PreviousTime = EditorApplication.timeSinceStartup;
        m_ParticleSystem = transform.GetComponentInChildren<ParticleSystem>();
        EditorApplication.update += inspectorUpdate;
    }
    
    void OnDisable()
    {
        EditorApplication.update -= inspectorUpdate;
    }

    void OnDestroy()
    {
        EditorApplication.update -= inspectorUpdate;
    }
    
    private void inspectorUpdate()
    {
        var delta = EditorApplication.timeSinceStartup - m_PreviousTime;
        m_PreviousTime = EditorApplication.timeSinceStartup;

        m_RunningTime = Mathf.Clamp(m_RunningTime + (float)delta, 0f, 5);
        update();
    }
    /// <summary>
    /// 预览播放状态下的更新
    /// </summary>
    private void update()
    {
        if (Application.isPlaying || m_ParticleSystem == null)
        {
            return;
        }

        if (m_RunningTime >= 5)
        {
            gameObject.SetActive(false);
            return;
        }

        m_ParticleSystem.Simulate(m_RunningTime, true);
        SceneView.RepaintAll();
     
    }
#endif

}