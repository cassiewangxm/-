using UnityEngine;

namespace RuntimeSceneGizmo
{
    using Controller;

    public class CameraMovement : MonoBehaviour
	{
#pragma warning disable 0649
		[SerializeField]
		private float sensitivity = 0.5f;
#pragma warning restore 0649

		private Vector3 prevMousePos;
		private Transform mainCamParent;

        public Camera Camera;
        private CameraController CameraController;

        private void Awake()
		{
			mainCamParent = Camera.main.transform.parent;
            CameraController = Camera.GetComponent<CameraController>();
        }

		private void Update()
		{
			if( Input.GetMouseButtonDown( 1 ) )
				prevMousePos = Input.mousePosition;
			else if( Input.GetMouseButton( 1 ) )
			{
				Vector3 mousePos = Input.mousePosition;
				Vector2 deltaPos = ( mousePos - prevMousePos ) * sensitivity;

				Vector3 rot = mainCamParent.localEulerAngles;
				while( rot.x > 180f )
					rot.x -= 360f;
				while( rot.x < -180f )
					rot.x += 360f;

				rot.x = Mathf.Clamp( rot.x - deltaPos.y, -89.8f, 89.8f );
				rot.y += deltaPos.x;
				rot.z = 0f;

                if (!CameraController.IsNavigation)
                {
                    rot.x = (rot.x < 15.0f) ? 15.0f : rot.x; 
                }
                else
                {
                    rot.x = (rot.x < 0.0f) ? 0.0f : rot.x;
                }

				mainCamParent.localEulerAngles = rot;
				prevMousePos = mousePos;
			}
		}
	}
}