
using UnityEngine;
using VSEngine.GAS;

public class GASExampleManager : MonoBehaviour
{
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private UIMain uiMainRes;
    // Start is called before the first frame update

    public UIMain UIMain { get; set; }
    
    public XYPlayer LocalPlayer { get; private set; }
    
    public static GASExampleManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        UIMain = Instantiate(uiMainRes, uiRoot.transform);
    }

    private void Update()
    {
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.Update(Time.deltaTime);
        }
    }

    public void SetLocalPlayer(XYPlayer player)
    {
        LocalPlayer = player;
        UIMain.PlayerInfoUpdate();
    }
    // Update is called once per frame
}
