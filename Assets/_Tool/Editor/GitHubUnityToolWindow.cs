// Assets/Editor/GitHubUnityTool/GitHubUnityToolWindow.cs

#if UNITY_EDITOR

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Vastav.EditorTools.GitHubUnityTool
{
    public class GitHubUnityToolWindow : EditorWindow
    {
        private string repoRoot;
        private string commitMessage = "Updated Unity project";
        private string outputLog = "";
        private Vector2 scroll;

        private bool autoRefreshStatus = true;
        private bool showAdvanced = false;

        private const string WindowTitle = "GitHub Tool";

        [MenuItem("Tools/GitHub/GitHub Tool")]
        public static void Open()
        {
            GitHubUnityToolWindow window = GetWindow<GitHubUnityToolWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(650f, 500f);
            window.Show();
        }

        private void OnEnable()
        {
            repoRoot = GetUnityProjectRoot();

            if (autoRefreshStatus)
            {
                RefreshStatus();
            }
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawRepositoryInfo();
            DrawMainActions();
            DrawCommitSection();
            DrawAdvancedSection();
            DrawOutput();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("GitHub Unity Tool", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Use Git directly inside Unity without switching to GitHub Desktop every time.");
            }

            EditorGUILayout.Space(6);
        }

        private void DrawRepositoryInfo()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Repository", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.SelectableLabel(repoRoot, EditorStyles.textField, GUILayout.Height(18));

                    if (GUILayout.Button("Refresh", GUILayout.Width(90)))
                    {
                        repoRoot = GetUnityProjectRoot();
                        RefreshStatus();
                    }
                }

                autoRefreshStatus = EditorGUILayout.ToggleLeft("Auto refresh status when window opens", autoRefreshStatus);

                if (!IsGitRepository(repoRoot))
                {
                    EditorGUILayout.HelpBox(
                        "This Unity project is not inside a Git repository. Initialize Git first or open a Unity project that is already connected with GitHub.",
                        MessageType.Warning
                    );
                }
            }

            EditorGUILayout.Space(6);
        }

        private void DrawMainActions()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Status", GUILayout.Height(32)))
                    {
                        RefreshStatus();
                    }

                    if (GUILayout.Button("Pull", GUILayout.Height(32)))
                    {
                        RunGitCommand("pull");
                    }

                    if (GUILayout.Button("Push", GUILayout.Height(32)))
                    {
                        RunGitCommand("push");
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Fetch", GUILayout.Height(28)))
                    {
                        RunGitCommand("fetch --all --prune");
                    }

                    if (GUILayout.Button("Current Branch", GUILayout.Height(28)))
                    {
                        RunGitCommand("branch --show-current");
                    }

                    if (GUILayout.Button("Open GitHub Desktop", GUILayout.Height(28)))
                    {
                        OpenGitHubDesktop();
                    }
                }
            }

            EditorGUILayout.Space(6);
        }

        private void DrawCommitSection()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Commit and Push", EditorStyles.boldLabel);

                commitMessage = EditorGUILayout.TextField("Commit Message", commitMessage);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add All", GUILayout.Height(32)))
                    {
                        RunGitCommand("add -A");
                    }

                    if (GUILayout.Button("Commit", GUILayout.Height(32)))
                    {
                        CommitChanges();
                    }

                    if (GUILayout.Button("Add, Commit and Push", GUILayout.Height(32)))
                    {
                        AddCommitPush();
                    }
                }

                EditorGUILayout.HelpBox(
                    "Recommended flow: Status, Pull, Add, Commit and Push. Pull before pushing when working with other people.",
                    MessageType.Info
                );
            }

            EditorGUILayout.Space(6);
        }

        private void DrawAdvancedSection()
        {
            showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced", true);

            if (!showAdvanced)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Extra Commands", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Log", GUILayout.Height(28)))
                    {
                        RunGitCommand("log --oneline -10");
                    }

                    if (GUILayout.Button("Diff", GUILayout.Height(28)))
                    {
                        RunGitCommand("diff --stat");
                    }

                    if (GUILayout.Button("Remote", GUILayout.Height(28)))
                    {
                        RunGitCommand("remote -v");
                    }
                }

                EditorGUILayout.Space(4);

                if (GUILayout.Button("Open Repository Folder", GUILayout.Height(28)))
                {
                    EditorUtility.RevealInFinder(repoRoot);
                }
            }

            EditorGUILayout.Space(6);
        }

        private void DrawOutput()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);

                    if (GUILayout.Button("Clear", GUILayout.Width(80)))
                    {
                        outputLog = "";
                    }
                }

                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(180));

                GUIStyle logStyle = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = true,
                    richText = false
                };

                EditorGUILayout.TextArea(outputLog, logStyle, GUILayout.ExpandHeight(true));

                EditorGUILayout.EndScrollView();
            }
        }

        private void RefreshStatus()
        {
            RunGitCommand("status --short --branch");
        }

        private void CommitChanges()
        {
            if (string.IsNullOrWhiteSpace(commitMessage))
            {
                AppendLog("Commit failed: Commit message is empty.");
                return;
            }

            string safeMessage = EscapeGitArgument(commitMessage);
            RunGitCommand("commit -m \"" + safeMessage + "\"");
        }

        private void AddCommitPush()
        {
            if (string.IsNullOrWhiteSpace(commitMessage))
            {
                AppendLog("Commit failed: Commit message is empty.");
                return;
            }

            RunGitCommand("add -A");

            string safeMessage = EscapeGitArgument(commitMessage);
            CommandResult commitResult = RunGitCommand("commit -m \"" + safeMessage + "\"", false);

            AppendLog("> git commit -m \"" + safeMessage + "\"");
            AppendLog(commitResult.FullOutput);

            if (commitResult.ExitCode != 0)
            {
                AppendLog("Push skipped because commit failed. This can happen when there are no changes to commit.");
                return;
            }

            RunGitCommand("push");
        }

        private CommandResult RunGitCommand(string arguments, bool appendAutomatically = true)
        {
            if (!IsGitRepository(repoRoot))
            {
                CommandResult failedResult = new CommandResult
                {
                    ExitCode = -1,
                    FullOutput = "Command failed: This Unity project is not inside a Git repository."
                };

                if (appendAutomatically)
                    AppendLog(failedResult.FullOutput);

                return failedResult;
            }

            CommandResult result = RunProcess("git", arguments, repoRoot);

            if (appendAutomatically)
            {
                AppendLog("> git " + arguments);
                AppendLog(result.FullOutput);
            }

            return result;
        }

        private void OpenGitHubDesktop()
        {
            AppendLog("Trying to open GitHub Desktop...");

            CommandResult commandResult = RunProcess("github", QuotePath(repoRoot), repoRoot);

            if (commandResult.ExitCode == 0)
            {
                AppendLog("> github " + QuotePath(repoRoot));
                AppendLog(commandResult.FullOutput);
                return;
            }

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string[] possiblePaths =
            {
        Path.Combine(localAppData, "GitHubDesktop", "GitHubDesktop.exe"),
        Path.Combine(localAppData, "GitHubDesktop", "app-3.4.20", "GitHubDesktop.exe"),
        Path.Combine(localAppData, "GitHubDesktop", "app-3.4.21", "GitHubDesktop.exe"),
        Path.Combine(localAppData, "GitHubDesktop", "app-3.4.22", "GitHubDesktop.exe"),
        Path.Combine(localAppData, "GitHubDesktop", "app-3.4.23", "GitHubDesktop.exe"),
        Path.Combine(localAppData, "GitHubDesktop", "app-3.4.24", "GitHubDesktop.exe"),
        Path.Combine(localAppData, "GitHubDesktop", "app-3.4.25", "GitHubDesktop.exe")
    };

            for (int i = 0; i < possiblePaths.Length; i++)
            {
                string githubDesktopPath = possiblePaths[i];

                if (!File.Exists(githubDesktopPath))
                    continue;

                CommandResult exeResult = RunProcess(githubDesktopPath, QuotePath(repoRoot), repoRoot);

                AppendLog("> " + githubDesktopPath + " " + QuotePath(repoRoot));
                AppendLog(exeResult.FullOutput);

                if (exeResult.ExitCode == 0)
                    return;
            }

            string updateExePath = Path.Combine(localAppData, "GitHubDesktop", "Update.exe");

            if (File.Exists(updateExePath))
            {
                CommandResult updateResult = RunProcess(updateExePath, "--processStart GitHubDesktop.exe --process-start-args " + QuotePath(repoRoot), repoRoot);

                AppendLog("> " + updateExePath);
                AppendLog(updateResult.FullOutput);

                if (updateResult.ExitCode == 0)
                    return;
            }

            AppendLog("Could not open GitHub Desktop. Open Command Prompt and test this command: github " + QuotePath(repoRoot));
            AppendLog("If that command does not work, GitHub Desktop is not available in PATH.");
        }

        private static string QuotePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "\"\"";

            return "\"" + path.Replace("\"", "\\\"") + "\"";
        }

        private static CommandResult RunProcess(string fileName, string arguments, string workingDirectory)
        {
            CommandResult result = new CommandResult();

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;

                    StringBuilder outputBuilder = new StringBuilder();
                    StringBuilder errorBuilder = new StringBuilder();

                    process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs args)
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            outputBuilder.AppendLine(args.Data);
                        }
                    };

                    process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs args)
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            errorBuilder.AppendLine(args.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    result.ExitCode = process.ExitCode;

                    string output = outputBuilder.ToString();
                    string error = errorBuilder.ToString();

                    string finalOutput = "";

                    if (!string.IsNullOrWhiteSpace(output))
                        finalOutput += output;

                    if (!string.IsNullOrWhiteSpace(error))
                        finalOutput += error;

                    result.FullOutput = string.IsNullOrWhiteSpace(finalOutput)
                        ? "Command completed with no output."
                        : CleanOutput(finalOutput);
                }
            }
            catch (Exception exception)
            {
                result.ExitCode = -1;
                result.FullOutput = CleanOutput(exception.Message);
            }

            return result;
        }

        private static string CleanOutput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            StringBuilder cleanBuilder = new StringBuilder(input.Length);

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '\r')
                    continue;

                if (c == '\n' || c == '\t')
                {
                    cleanBuilder.Append(c);
                    continue;
                }

                if (c == 65533)
                    continue;

                if (c == 65279)
                    continue;

                if (!char.IsControl(c))
                {
                    cleanBuilder.Append(c);
                }
            }

            return cleanBuilder.ToString().Trim();
        }

        private static string GetUnityProjectRoot()
        {
            string assetsPath = Application.dataPath;
            DirectoryInfo assetsDirectory = new DirectoryInfo(assetsPath);

            return assetsDirectory.Parent != null
                ? assetsDirectory.Parent.FullName
                : assetsPath;
        }

        private static bool IsGitRepository(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            string gitFolder = Path.Combine(path, ".git");
            return Directory.Exists(gitFolder) || File.Exists(gitFolder);
        }

        private static string EscapeGitArgument(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private void AppendLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            string cleanMessage = CleanOutput(message);

            outputLog += cleanMessage.TrimEnd() + "\n\n";
            Repaint();

            Debug.Log("[GitHub Tool] " + cleanMessage);
        }

        private struct CommandResult
        {
            public int ExitCode;
            public string FullOutput;
        }
    }
}

#endif