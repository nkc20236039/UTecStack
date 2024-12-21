using System;

namespace UToyStack.DOTSUtility.MonoEventAufheben
{
    /// <summary>
    /// �C�x���g�̓o�^�A�o�^�����A���M���s���`�����l�����������܂��B
    /// </summary>
    public interface IEventChannelStorable<T>
    {
        /// <summary>
        /// �C�x���g�̓o�^���s���܂�
        /// </summary>
        void Subscribe(Action<T> callback);
        /// <summary>
        /// �C�x���g�̓o�^�������s���܂�
        /// </summary>
        void Unsubscribe(Action<T> callback);
        /// <summary>
        /// �C�x���g�̔��M���s���܂�
        /// </summary>
        /// <param name="message">�����ɑ��郁�b�Z�[�W</param>
        void Dispatch(T message = default);
    }
}