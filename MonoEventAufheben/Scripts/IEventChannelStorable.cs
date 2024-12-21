using System;

namespace UToyStack.DOTSUtility.MonoEventAufheben
{
    /// <summary>
    /// イベントの登録、登録解除、発信を行うチャンネルを実装します。
    /// </summary>
    public interface IEventChannelStorable<T>
    {
        /// <summary>
        /// イベントの登録を行います
        /// </summary>
        void Subscribe(Action<T> callback);
        /// <summary>
        /// イベントの登録解除を行います
        /// </summary>
        void Unsubscribe(Action<T> callback);
        /// <summary>
        /// イベントの発信を行います
        /// </summary>
        /// <param name="message">同時に送るメッセージ</param>
        void Dispatch(T message = default);
    }
}