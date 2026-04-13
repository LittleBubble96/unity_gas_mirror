using System;
using System.Collections.Generic;
using UnityEngine;

namespace VSEngine.GAS
{
    public class DrawGizmosMonoIns : MonoBehaviour
    {
        private static DrawGizmosMonoIns _instance;
        
        public static DrawGizmosMonoIns Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = GameObject.Find("DrawGizmosMonoIns");
                    if (go == null)
                    {
                        go = new GameObject("DrawGizmosMonoIns");
                    }
                    _instance = go.AddComponent<DrawGizmosMonoIns>();
                }

                return _instance;
            }
        }

        private List<DrawGizmosData> _drawGizmosDataList = new List<DrawGizmosData>();
        private int _generateId = 0;
        
        public void AddDrawGizmosData(ref DrawGizmosData data)
        {            
            _generateId++;
            data.Id = _generateId;
            _drawGizmosDataList.Add(data);
        }
        
        public void RemoveDrawGizmosData(int id)
        {
            _drawGizmosDataList.RemoveAll(data => data.Id == id);
        }

        private void OnDrawGizmos()
        {
            for (int i = _drawGizmosDataList.Count - 1; i >= 0; i--)
            {
                var data = _drawGizmosDataList[i];
                data.Range.DrawGizmos(data.Color);
            }
        }
    }
    
    public struct DrawGizmosData
    {
        public int Id;
        public Color Color;
        public RangeStruct Range;
    }
}