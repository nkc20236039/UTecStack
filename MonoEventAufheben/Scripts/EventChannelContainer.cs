using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System;

namespace UToyStack.DOTSUtility.MonoEventAufheben
{
    public class EventChannelContainer<T> : IEventChannelStorable<T>
    {
        // イベントキュー
        private readonly ConcurrentQueue<T> _eventQueue = new();

        // コールバックリスト
        private readonly List<Action<T>> _subscribers = new();
        private readonly object _subscribersLock = new();   // ロックにのみ使用

        // 処理中のメッセージを管理するフラグ
        private int _isProcessing = 0;

        /// <summary>
        /// イベントの登録を行います
        /// </summary>
        /// <param name="callback"></param>
        public void Subscribe(Action<T> callback)
        {
            lock (_subscribersLock)
            {
                _subscribers.Add(callback);
            }
        }

        /// <summary>
        /// イベントの登録解除をします
        /// </summary>
        /// <param name="callback"></param>
        public void Unsubscribe(Action<T> callback)
        {
            lock (_subscribersLock)
            {
                _subscribers.Remove(callback);
            };
        }

        /// <summary>
        /// イベントを発信します
        /// </summary>
        /// <param name="message">同時に送るメッセージ</param>
        public void Dispatch(T message = default)
        {
            // メッセージをキューに追加
            _eventQueue.Enqueue(message);

            // 既に処理中だったら終了
            if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0) { return; }

            // メッセージ処理のトリガー
            try
            {
                ProcessMessageQueue();
            }
            finally
            {
                // 処理完了フラグをリセット
                Interlocked.Exchange(ref _isProcessing, 0);
            }
        }

        private void ProcessMessageQueue()
        {
            while (_eventQueue.TryDequeue(out T message))
            {
                // コピーを作成してスレッドセーフに反復
                Action<T>[] subscribersCopy;
                lock (_subscribersLock)
                {
                    subscribersCopy = _subscribers.ToArray();

                    // すべてのサブスクライバーにメッセージを送信
                    foreach (var subscriber in subscribersCopy)
                    {
                        try
                        {
                            // メインスレッドでコールバックを実行
                            MonoEventAufheben.Instance.Enqueue(() => subscriber(message));
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"イベント実行中にエラーが発生しました\n{ex}");
                        }
                    }
                }
            }
        }
    }
}