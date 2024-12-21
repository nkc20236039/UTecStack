using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System;

namespace UToyStack.DOTSUtility.MonoEventAufheben
{
    public class EventChannelContainer<T> : IEventChannelStorable<T>
    {
        // �C�x���g�L���[
        private readonly ConcurrentQueue<T> _eventQueue = new();

        // �R�[���o�b�N���X�g
        private readonly List<Action<T>> _subscribers = new();
        private readonly object _subscribersLock = new();   // ���b�N�ɂ̂ݎg�p

        // �������̃��b�Z�[�W���Ǘ�����t���O
        private int _isProcessing = 0;

        /// <summary>
        /// �C�x���g�̓o�^���s���܂�
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
        /// �C�x���g�̓o�^���������܂�
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
        /// �C�x���g�𔭐M���܂�
        /// </summary>
        /// <param name="message">�����ɑ��郁�b�Z�[�W</param>
        public void Dispatch(T message = default)
        {
            // ���b�Z�[�W���L���[�ɒǉ�
            _eventQueue.Enqueue(message);

            // ���ɏ�������������I��
            if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0) { return; }

            // ���b�Z�[�W�����̃g���K�[
            try
            {
                ProcessMessageQueue();
            }
            finally
            {
                // ���������t���O�����Z�b�g
                Interlocked.Exchange(ref _isProcessing, 0);
            }
        }

        private void ProcessMessageQueue()
        {
            while (_eventQueue.TryDequeue(out T message))
            {
                // �R�s�[���쐬���ăX���b�h�Z�[�t�ɔ���
                Action<T>[] subscribersCopy;
                lock (_subscribersLock)
                {
                    subscribersCopy = _subscribers.ToArray();

                    // ���ׂẴT�u�X�N���C�o�[�Ƀ��b�Z�[�W�𑗐M
                    foreach (var subscriber in subscribersCopy)
                    {
                        try
                        {
                            // ���C���X���b�h�ŃR�[���o�b�N�����s
                            MonoEventAufheben.Instance.Enqueue(() => subscriber(message));
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"�C�x���g���s���ɃG���[���������܂���\n{ex}");
                        }
                    }
                }
            }
        }
    }
}