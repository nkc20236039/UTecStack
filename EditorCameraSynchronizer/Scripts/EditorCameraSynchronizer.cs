#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace UTecStack
{
    [RequireComponent(typeof(Camera))]
    public class EditorCameraSynchronizer : MonoBehaviour
    {
        private Camera gameCamera;
        private Camera sceneCamera;

        private void Start()
        {
            gameCamera = GetComponent<Camera>();
            if (SceneView.lastActiveSceneView != null)
            {
                sceneCamera = SceneView.lastActiveSceneView.camera;
            }
        }

        void Update()
        {
            // シーンビューが無ければ実行しない
            if (SceneView.lastActiveSceneView == null) { return; }
            if (SceneView.lastActiveSceneView.camera != sceneCamera)
            {
                // カメラのインスタンスが異なれば新しく代入
                sceneCamera = SceneView.lastActiveSceneView.camera;
            }

            gameCamera.transform.position = sceneCamera.transform.position;
            gameCamera.transform.rotation = sceneCamera.transform.rotation;
            gameCamera.fieldOfView = sceneCamera.fieldOfView;
        }
    }
}
#endif