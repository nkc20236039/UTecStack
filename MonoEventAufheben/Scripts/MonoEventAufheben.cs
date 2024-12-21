using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace UToyStack.DOTSUtility.MonoEventAufheben
{
    public class MonoEventAufheben : MonoBehaviour
    {
        private static bool isQuitting = false;

        /*------- �V���O���g���C���X�^���X���쐬���邽�߂̏��� -------*/
        private static readonly object lockObject = new();  // ���b�N�ɂ̂ݎg�p ������Ȃ��Ă悢
        private static MonoEventAufheben instance;
        public static MonoEventAufheben Instance
        {
            get
            {
                if (instance == null)   // �I�[�o�[�w�b�h������邽�߁A�����ł��C���X�^���X���m�F����
                {
                    // �C���X�^���X�����݂��Ȃ���΍쐬�����݂�
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
        /*----- �V���O���g���C���X�^���X���쐬���邽�߂̏��� �I�� -----*/

        // ���b�Z�[�W�`�����l����ێ����鎫��
        private readonly Dictionary<Type, object> _channels = new();
        private readonly object _channelsLock = new();  // ���b�N�ɂ̂ݎg�p

        // �A�N�V�����L���[
        private readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();

        private void LateUpdate()
        {
            if (isQuitting) { return; }

            // ���C���X���b�h�ŃA�N�V���������s
            while (_actionQueue.TryDequeue(out Action action))
            {
                action?.Invoke();
            }
        }

        /// <summary>
        /// �`�����l�����擾���܂�
        /// </summary>
        public IEventChannelStorable<T> GetChannel<T>()
        {
            var type = typeof(T);

            lock (_channelsLock)
            {
                if (!_channels.TryGetValue(type, out var channel))
                {
                    // �`�����l�������݂��Ȃ���ΐV�K�쐬
                    channel = new EventChannelContainer<T>();
                    _channels[type] = channel;
                }

                return (IEventChannelStorable<T>)channel;
            }
        }

        /// <summary>
        /// �A�N�V�������L���[�ɒǉ����܂�
        /// </summary>
        internal void Enqueue(Action action)
        {
            _actionQueue.Enqueue(action);
        }


        /*----- ���S�[�u -----*/
        private void OnDestroy()
        {
            // ������C���X�^���X���������Ă���N���X���j�����ꂽ�Ƃ��ɐÓI�ϐ����N���A����
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