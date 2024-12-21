using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace UToyStack.DOTSUtility.MonoEventAufheben
{
    public class MonoEventAufheben : MonoBehaviour
    {
        private static bool isQuitting = false;

        /*------- シングルトンインスタンスを作成するための処理 -------*/
        private static readonly object lockObject = new();  // ロックにのみ使用 解放しなくてよい
        private static MonoEventAufheben instance;
        public static MonoEventAufheben Instance
        {
            get
            {
                if (instance == null)   // オーバーヘッドを避けるため、ここでもインスタンスを確認する
                {
                    // インスタンスが存在しなければ作成を試みる
                    TryCreateNewInstance();
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }

        private static void TryCreateNewInstance()
        {
            lock (lockObject)
            {
                if (instance == null && !isQuitting)
                {
                    var singletonObject = new GameObject("[Generated] MonoEventAufheben GameObject");
                    instance = singletonObject.AddComponent<MonoEventAufheben>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
        }
        /*----- シングルトンインスタンスを作成するための処理 終了 -----*/

        // メッセージチャンネルを保持する辞書
        private readonly Dictionary<Type, object> _channels = new();
        private readonly object _channelsLock = new();  // ロックにのみ使用

        // アクションキュー
        private readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();

        private void LateUpdate()
        {
            if (isQuitting) { return; }

            // メインスレッドでアクションを実行
            while (_actionQueue.TryDequeue(out Action action))
            {
                action?.Invoke();
            }
        }

        /// <summary>
        /// チャンネルを取得します
        /// </summary>
        public IEventChannelStorable<T> GetChannel<T>()
        {
            var type = typeof(T);

            lock (_channelsLock)
            {
                if (!_channels.TryGetValue(type, out var channel))
                {
                    // チャンネルが存在しなければ新規作成
                    channel = new EventChannelContainer<T>();
                    _channels[type] = channel;
                }

                return (IEventChannelStorable<T>)channel;
            }
        }

        /// <summary>
        /// アクションをキューに追加します
        /// </summary>
        internal void Enqueue(Action action)
        {
            _actionQueue.Enqueue(action);
        }


        /*----- 安全措置 -----*/
        private void OnDestroy()
        {
            // 万が一インスタンスを所持しているクラスが破棄されたときに静的変数をクリアする
            if (instance == this)
            {
                instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlayMode(UnityEditor.EnterPlayModeOptions options)
        {
            instance = null;
            isQuitting = false;
        }
#endif
    }
}