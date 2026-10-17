using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace DecirQueNo.EditorTools
{
    // Runs the PlayMode bot tests from the menu or from a script and writes the outcome to
    // BotReports/summary.txt. Survives the domain reload that entering Play Mode causes.
    // Menu: DecirQueNo/Tests/Run PlayMode bots
    [InitializeOnLoad]
    public static class BotTestRunner
    {
        private static readonly TestRunnerApi Api;

        static BotTestRunner()
        {
            Api = ScriptableObject.CreateInstance<TestRunnerApi>();
            Api.RegisterCallbacks(new Callbacks());
        }

        public static string SummaryPath =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BotReports", "summary.txt"));

        [MenuItem("DecirQueNo/Tests/Run PlayMode bots")]
        public static void Run()
        {
            string folder = Path.GetDirectoryName(SummaryPath);
            Directory.CreateDirectory(folder);
            File.WriteAllText(SummaryPath, "RUNNING");

            Api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.PlayMode }));
        }

        private class Callbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"RESULT {result.ResultState} passed={result.PassCount} failed={result.FailCount} skipped={result.SkipCount}");
                Append(sb, result);
                Directory.CreateDirectory(Path.GetDirectoryName(SummaryPath));
                File.WriteAllText(SummaryPath, sb.ToString());
            }

            private static void Append(StringBuilder sb, ITestResultAdaptor node)
            {
                if (!node.HasChildren)
                {
                    sb.AppendLine($"{node.TestStatus} {node.Test.Name}");
                    if (node.TestStatus == TestStatus.Failed)
                    {
                        sb.AppendLine("    " + node.Message?.Replace("\n", " "));
                    }

                    return;
                }

                foreach (ITestResultAdaptor child in node.Children)
                {
                    Append(sb, child);
                }
            }
        }
    }
}
