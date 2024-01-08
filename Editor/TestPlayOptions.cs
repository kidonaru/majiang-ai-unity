using UnityEngine;

namespace Majiang.AI.Editor
{
    [CreateAssetMenu(menuName = "Majiang/Create TestPlayOptions", fileName = "TestPlayOptions")]
    public class TestPlayOptions : ScriptableObject
    {
        [SerializeField] public int Times = 1;
        [SerializeField] public string Shan;
        [SerializeField] public string OutputPath = "Assets/TestPlayLog.json";
        [SerializeField] public int Skip = 0;
        [SerializeField] public Rule Rule;
    }
}