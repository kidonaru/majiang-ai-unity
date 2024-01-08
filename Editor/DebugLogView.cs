using Debug = UnityEngine.Debug;

namespace Majiang.AI.Editor
{
    public class DebugLogView : Majiang.View
    {
        public void kaiju()
        {
            Debug.Log("開局");
        }

        public void update(Message paipuLog = null)
        {
            Debug.Log($"update: {paipuLog}");
        }

        public void redraw()
        {
        }

        public void summary(Paipu paipu = null)
        {
            var new_paipu = (Paipu) paipu.Clone();
            new_paipu.log = null;
            Debug.Log($"終局: {paipu}");
        }

        public void say(string type, int lunban)
        {
            Debug.Log($"{type}: {lunban}");
        }
    }
}