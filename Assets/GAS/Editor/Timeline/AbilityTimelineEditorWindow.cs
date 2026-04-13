using System;
using System.IO;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using VSEngine.GAS;
using Object = UnityEngine.Object;

namespace GAS.Editor
{
    public class AbilityTimelineEditorWindow : UnityEditor.EditorWindow
    {
        private string _timelineScenePath = "Assets/GAS/Scenes/AbilityTimelineScene.unity";
        private PlayableDirector _director;
        private bool _isPlaying = false;
        private GameplayAbilityAsset _abilityAsset;
        private TimelineAsset _currentTimeline;
        private Transform _previewContainer;
        
        private GameObject _previewActorPrefab;
        private GameObject _previewActor;
        
        private static GameObject _tempContainer;
        
        public static GameObject TempContainer
        {
            get
            {
                if (_tempContainer == null)
                {
                    _tempContainer = GameObject.Find("TempContainer");
                    if (_tempContainer == null)
                    {
                        _tempContainer = new GameObject("TempContainer");
                    }
                }
                return _tempContainer;
            }
        }
        
        private static GameplayCueManager _cueManager;

        public static GameplayCueManager CueManager
        {
            get
            {
                if (_cueManager == null)
                {
                    _cueManager = new GameplayCueManager();
                    _cueManager.Init();
                }
                return _cueManager;
            }
        }


        //
        public static void ShowWindow(GameplayAbilityAsset asset)
        {
            var window = GetWindow<AbilityTimelineEditorWindow>();
            window.titleContent = new GUIContent($"编辑技能时间线-{asset.Name}");
            window.Initialize(asset);
        }
        
        private void OpenAbilityScene()
        {
            if (SceneManager.GetActiveScene().path != _timelineScenePath)
            {
                EditorSceneManager.OpenScene(_timelineScenePath, OpenSceneMode.Single);
            }
        }
        
        private void Initialize(GameplayAbilityAsset asset)
        {
            _abilityAsset = asset;
            OpenAbilityScene();
            // 在这里初始化编辑器窗口，加载技能时间线数据等
            SetupTimeline();
            GenerateContainer();
        }

        private void GenerateContainer()
        {
            GameObject findObj = GameObject.Find("PreviewContainer");
            if (!findObj)
            {
                findObj = new GameObject("PreviewContainer");
            }
            _previewContainer = findObj.transform;
        }
        
        private void SetupTimeline()
        {
            _director = FindObjectOfType<PlayableDirector>();
            if (_director == null)
            {
                var directorGO = new GameObject("AbilityDirector");
                _director = directorGO.AddComponent<PlayableDirector>();
            }

            string timelinePath = _abilityAsset != null 
                ? GASSettingAsset.GAS_Timeline_ASSET_PATH + $"{_abilityAsset.Name}_Timeline.playable"
                : "Assets/GAS/Editor/Timelines/NewAbility_Timeline.playable";
                
            _currentTimeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(timelinePath);
            
            if (_currentTimeline == null)
            {
                // 如果没有现有的Timeline，创建一个新的
                _currentTimeline = TimelineCreator.CreateNewTimeline(timelinePath);
            }
            _director.playableAsset = _currentTimeline;
            SetupDefaultBindings();
            OpenTimelineEditor();
        }
        

        private void SetupDefaultBindings()
        {
            if (!_currentTimeline)
            {
                return;
            }
            if (!_previewActor)
            {
                return;
            }
            var animator = _previewActor.GetComponentInChildren<Animator>();
            if (animator == null) return;
            
            foreach (var output in _currentTimeline.outputs)
            {
                if (output.outputTargetType == typeof(Animator))
                {
                    _director.SetGenericBinding(output.sourceObject, animator);
                }
            }
          
        }
        
        private void OpenTimelineEditor()
        {
            // 这里可以实现打开Timeline编辑器的逻辑，Unity没有提供直接打开Timeline编辑器的API，
            // 但你可以通过选择PlayableDirector来间接打开它
            Selection.activeObject = _director;
        }
        
        private void OnChangeTargetActor()
        {
            if (targetActor.gameObject != _previewActorPrefab && _previewContainer)
            {
                //清除pareviewContainer下的预览角色
                ClearPreviewContainer();
                var obj = targetActor.gameObject;
                _previewActorPrefab = obj;
                _previewActor = Instantiate(obj, _previewContainer);
                _previewActor.transform.localPosition = Vector3.zero;
                SetupDefaultBindings();
            }
        }
        private void ClearPreviewContainer()
        {
            
            if (_previewContainer)
            {
                for (int i = _previewContainer.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(_previewContainer.GetChild(i).gameObject);
                }
            }
        }

        #region Draw

        private void OnGUI()
        {
            DrawToolbar();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            GUILayout.Label("技能: " + (_abilityAsset != null ? _abilityAsset.Name : "未选择"), EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("保存", EditorStyles.toolbarButton))
            {
                ExportTimeline();
            }
            
            if (GUILayout.Button("聚焦timeline", EditorStyles.toolbarButton))
            {
                OpenTimelineEditor();
            }
            EditorGUILayout.EndHorizontal();
            
            //绘制预览Actor选择
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            EditorGUILayout.LabelField("预览设置", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            targetActor = (AbilitySystemComponent)EditorGUILayout.ObjectField("预览Actor", targetActor, typeof(AbilitySystemComponent), true);
            //当预览Actor发生变化时，更新预览角色
            if (targetActor != null && targetActor.gameObject != _previewActorPrefab)
            {
                OnChangeTargetActor();
            }
            else if (targetActor == null && _previewActorPrefab != null)
            {
                if (_previewActor)
                {
                    Object.DestroyImmediate(_previewActor);
                    _previewActor = null;
                }
                _previewActorPrefab = null;
            }
            
            //绘制timeline 操作按钮
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Timeline操作", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            Color color = GUI.color;
            GUI.color = _isPlaying ? Color.gray : Color.green;
            //绘制一个timeline播放按钮
            if (GUILayout.Button("播放Timeline", GUILayout.Height(30)))
            {
                if (_director)
                {
                    PreparePlayTime();
                    _director.Play();
                    _isPlaying = true;
                }
            }
            GUI.color = _isPlaying ? Color.gray : Color.red;
            //绘制一个timeline停止按钮
            if (GUILayout.Button("停止Timeline", GUILayout.Height(30)))
            {
                if (_director)
                {
                    _director.Stop();
                    _isPlaying = false;
                }
            }
            GUI.color = color;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
        }

        private void PreparePlayTime()
        {
            //清除 TempContainer 下的所有对象
            for (int i = TempContainer.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(TempContainer.transform.GetChild(i).gameObject);
            }
        }

        [LabelText("预览Actor")]
        [OnValueChanged(nameof(OnChangeTargetActor))]
        public AbilitySystemComponent targetActor;
        
        private void ExportTimeline()
        {
            if (_abilityAsset == null || _currentTimeline == null)
            {
                Debug.LogError("无法导出Timeline，缺少技能资产或当前Timeline。");
                return;
            }
            string error = TimelineAbilityImport.SaveAbilityTimelineAsset(_currentTimeline);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"导出Timeline失败: {error}");
                //弹窗
                EditorUtility.DisplayDialog("导出失败", error, "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("导出成功", $"Timeline已成功导出", "确定");
            }
        }

        #endregion
    }

    public static class TimelineCreator
    {
        public static TimelineAsset CreateNewTimeline(string path)
        {
            //如果文件夹不存在，创建文件夹
            string directory = Path.GetDirectoryName(path);
            if (Directory.Exists(directory) == false && !string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, path);
            AssetDatabase.SaveAssets();
            return timeline;
        }
    }
}