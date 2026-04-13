using UnityEditor;
using UnityEngine;

namespace VSEngine.GAS
{
    public class GasDefine
    {
        //GAS设置 文件夹名称
        public const string GAS_ASSET_FOLDER_NAME = "GAS_Setting";
        //GAS设置 文件夹路径
        public static string GAS_ASSET_PATH => $"Assets/{GAS_ASSET_FOLDER_NAME}";
        //GAS设置 资源路径
        public static string GAS_SYSTEM_ASSET_PATH => $"{GAS_ASSET_PATH}/GASSettingAsset.asset";

#if UNITY_EDITOR
        
        public static void CheckGasAssetFolder()
        {
            if (!AssetDatabase.IsValidFolder(GasDefine.GAS_ASSET_PATH))
            {
                AssetDatabase.CreateFolder("Assets", GasDefine.GAS_ASSET_FOLDER_NAME);
                Debug.Log($"[XY] {GasDefine.GAS_ASSET_FOLDER_NAME} folder created!");
            }
        }
#endif
        
    }
}