using Sirenix.OdinInspector;

namespace VSEngine.GAS
{
    public class CueLabelAttribute : LabelTextAttribute
    {
        public CueLabelAttribute(string text) : base(text)
        {
        }

        public CueLabelAttribute(SdfIconType icon) : base(icon)
        {
        }

        public CueLabelAttribute(string text, bool nicifyText) : base(text, nicifyText)
        {
        }

        public CueLabelAttribute(string text, SdfIconType icon) : base(text, icon)
        {
        }

        public CueLabelAttribute(string text, bool nicifyText, SdfIconType icon) : base(text, nicifyText, icon)
        {
        }
    }
}