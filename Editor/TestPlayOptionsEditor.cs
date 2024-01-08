using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Majiang.AI.Editor
{
    [CustomEditor(typeof(TestPlayOptions))]
    public class TestPlayOptionsEditor : UnityEditor.Editor
    {
        private bool isRunning = false;
        private Stopwatch stopwatch;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (!isRunning)
            {
                if (GUILayout.Button("Run"))
                {
                    isRunning = true;
                    stopwatch = Stopwatch.StartNew();
                    TestPlayOptions options = (TestPlayOptions)target;
                    Task.Run(() =>
                    {
                        try
                        {
                            TestPlay.Run(options);
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e);
                        }
                        finally
                        {
                            stopwatch.Stop();
                            isRunning = false;
                        }
                    });
                }
            }
            else
            {
                GUILayout.Label("Running...");
                GUILayout.Label("Elapsed Time: " + stopwatch.Elapsed.TotalSeconds + " seconds");
            }
        }
    }
}