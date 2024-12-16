#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;

[InitializeOnLoad]
public class FolderKeeper : AssetPostprocessor
{
    // キープファイルの名前
    public static readonly string keeperName = ".gitkeep";

    static FolderKeeper()
    {
        // 処理を呼び出す
        SetKeepers();
    }

    // アセット更新時に実行
    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetsPath)
    {
        SetKeepers();
    }

    // メニューにアイテムを追加
    [MenuItem("Tools/Set Keepers")]
    public static void SetKeepers()
    {
        // キープファイルを配置する
        CheckKeeper("Assets");

        // データベースをリフレッシュする
        AssetDatabase.Refresh();
    }

    public static void CheckKeeper(string path)
    {
        // ディレクトリパスの配列
        ReadOnlySpan<string> directories = Directory.GetDirectories(path);
        // ファイルパスの配列
        ReadOnlySpan<string> files = Directory.GetFiles(path);
        // キープファイルの配列
        ReadOnlySpan<string> keepers = Directory.GetFiles(path, keeperName);

        // ディレクトリがあるか
        bool isDirectoryExist = 0 < directories.Length;
        // (キープファイル以外の)ファイルがあるか
        bool isFileExist = 0 < (files.Length - keepers.Length);
        // キープファイルがあるか
        bool isKeeperExist = 0 < keepers.Length;

        if (!isDirectoryExist && !isFileExist)
        {
            // キープファイルがなかったら
            if (!isKeeperExist)
            {
                // キープファイルを作成
                File.Create(path + "/" + keeperName).Close();
            }
            return;
        }
        else
        {
            // キープファイルがあったら
            if (isKeeperExist)
            {
                // キープファイルを削除
                File.Delete(path + "/" + keeperName);
            }
        }

        // さらに深い階層を探索
        foreach (var directory in directories)
        {
            CheckKeeper(directory);
        }
    }
}
#endif