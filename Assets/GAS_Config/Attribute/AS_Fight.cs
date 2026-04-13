using UnityEngine;
using VSEngine.GAS;

[System.Serializable]
public class AS_Fight : AttributeSet
{
    public AttributeBase Hp = new AttributeBase("AS_Fight", "Hp");
    public AttributeBase Mp = new AttributeBase("AS_Fight", "Mp");
    public AttributeBase Atk = new AttributeBase("AS_Fight", "Atk");
    public AttributeBase HpMax = new AttributeBase("AS_Fight", "HpMax");
    public AttributeBase MpMax = new AttributeBase("AS_Fight", "MpMax");
    public AttributeBase Armor = new AttributeBase("AS_Fight", "Armor");
    public AttributeBase mr = new AttributeBase("AS_Fight", "mr");
    public AttributeBase Level = new AttributeBase("AS_Fight", "Level");

    protected override AttributeBase GetInternal(string attName)
    {
        if (attName.Equals("Hp"))
        {
            return Hp;
        }
        if (attName.Equals("Mp"))
        {
            return Mp;
        }
        if (attName.Equals("Atk"))
        {
            return Atk;
        }
        if (attName.Equals("HpMax"))
        {
            return HpMax;
        }
        if (attName.Equals("MpMax"))
        {
            return MpMax;
        }
        if (attName.Equals("Armor"))
        {
            return Armor;
        }
        if (attName.Equals("mr"))
        {
            return mr;
        }
        if (attName.Equals("Level"))
        {
            return Level;
        }
        return null;
    }

    public override string[] AttributeNames => new string[]
    {
        "Hp",
        "Mp",
        "Atk",
        "HpMax",
        "MpMax",
        "Armor",
        "mr",
        "Level"
    };
}