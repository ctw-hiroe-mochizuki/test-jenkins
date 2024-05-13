using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class JenkinsBuild
{
	// todo : MACの時は確かこのパスの書き方だとダメだった記憶があるので後ほど調べる
	private const string BUILD_PATH = "C:/Users/hiroe.mochizuki/BeastProjects/jenkinsTest/TestBuild";

	private const string ARG_PATH_KEY = "-filePath";
	private const string ARG_BUILD_NUMBER_KEY = "-buidNumber";
	
	[MenuItem("Build/ApplicationBuild/Android")]
	public static void BuildAndroid()
	{
		//AndroidにSwitch Platform
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

		// ビルドに含めるシーン名のリスト作成
		var sceneNameArray = CreateBuildTargetScenes().ToArray();
		// 仮なので適当に…
		PlayerSettings.applicationIdentifier = "com.jenkins.test";
		PlayerSettings.productName = "jenkins test";
		PlayerSettings.companyName = "test jenkins";

		//Splash Screenをオフにする(Personalだと動かないよ）
		PlayerSettings.SplashScreen.show = false;
		PlayerSettings.SplashScreen.showUnityLogo = false;

		//AppBundleは使用しない（本番ビルドのときだけ使うイメージ）
		EditorUserBuildSettings.buildAppBundle = false;
		
		var path = BUILD_PATH;
		var number = 0;
		var args = Environment.GetCommandLineArgs();
		// コマンドラインの欲しい情報を取得する
		for (int i = 0; i < args.Length; i++)
		{
			switch (args[i])
			{
				case ARG_PATH_KEY:
					path = args[i + 1];
					break;
				case ARG_BUILD_NUMBER_KEY:
					number = int.Parse(args[i + 1]);
					break;
			}
			Debug.Log($">>> args[ {i} ] : {args[i]}");
		}

		// パスの組み立て(ビルド番号を添えて)
		var outputPath = $"{path}/test_{number}.apk";
		
		Debug.Log($">>> outputPath = {outputPath}");

		// apk作成
		var result = BuildPipeline.BuildPlayer(sceneNameArray, outputPath , BuildTarget.Android, BuildOptions.Development);
		// リザルトログ
		ResultLog(result);
	}

	#region Util
	/// <summary>
	/// シーンリストの取得
	/// </summary>
	/// <returns></returns>
	private static IEnumerable<string> CreateBuildTargetScenes()
	{
		foreach (var scene in EditorBuildSettings.scenes)
		{
			if (scene.enabled)
				yield return scene.path;
		}
	}

	/// <summary>
	/// リザルトログ
	/// </summary>
	/// <param name="report"></param>
	private static void ResultLog(BuildReport report)
	{
		var message = report.steps.SelectMany(e => e.messages).ToLookup(e => e.type, e => e.content);
		
		var summary = report.summary;
		switch (summary.result)
		{
			case BuildResult.Succeeded:
				Debug.Log($">>> Build Succeeded : {summary.totalSize} byte");
				EditorApplication.Exit(0);
				break;
			case BuildResult.Failed:
				Debug.LogError(">>> Build Failed" + string.Join("\n\t", message[LogType.Error].ToArray()));
				break;
			case BuildResult.Cancelled:
				Debug.LogError(">>> Build Cancelled");
				break;
			case BuildResult.Unknown:
			default:
				Debug.LogError(">>> Build Unknown");
				break;
		}
		EditorApplication.Exit(1);
	}

	#endregion
}
