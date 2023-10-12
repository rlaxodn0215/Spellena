using UnityEngine;


namespace Player
{
    public class MouseControl : MonoBehaviour
    {
        [Header("Settings")]
        public Vector2 clampInDegrees = new Vector2(360, 180);
        public bool eraseCursor = true;

        public Vector2 Sensitivity = new Vector2(6, 6);

        [Header("First Person")]
        public GameObject characterBody;

        private Vector2 targetDirection;
        private Vector2 targetCharacterDirection;

        private Vector2 mouseAbsolute;
        private Vector2 smoothMouse;

        private Vector2 mouseDelta;

        [HideInInspector]
        public bool scoped;

        void Start()
        {
            // 카메라의 회전 값 저장
            targetDirection = transform.localRotation.eulerAngles;

            // 플레이어의 회전 값 저장
            if (characterBody)
                targetCharacterDirection = characterBody.transform.localRotation.eulerAngles;

            // 커서 고정
            if (eraseCursor)
                EraseCursor();

        }

        public void EraseCursor()
        {
            // 커서를 숨기고 고정시킨다.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            // Allow the script to clamp based on a desired target value.
            var targetOrientation = Quaternion.Euler(targetDirection);
            var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

            // Get raw mouse input for a cleaner reading on more sensitive mice.
            mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(Sensitivity.x, Sensitivity.y));

            // Interpolate mouse movement over time to apply smoothing delta.
            smoothMouse.x = Mathf.Lerp(smoothMouse.x, mouseDelta.x, 1f / Sensitivity.x);
            smoothMouse.y = Mathf.Lerp(smoothMouse.y, mouseDelta.y, 1f / Sensitivity.y);

            // Find the absolute mouse movement value from point zero.
            mouseAbsolute += smoothMouse;

            // Clamp and apply the local x value first, so as not to be affected by world transforms.
            if (clampInDegrees.x < 360)
                mouseAbsolute.x = Mathf.Clamp(mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

            // Then clamp and apply the global y value.
            if (clampInDegrees.y < 360)
                mouseAbsolute.y = Mathf.Clamp(mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

            transform.localRotation = Quaternion.AngleAxis(-mouseAbsolute.y, targetOrientation * Vector3.right) * targetOrientation;

            // If there's a character body that acts as a parent to the camera
            if (characterBody)
            {
                var yRotation = Quaternion.AngleAxis(mouseAbsolute.x, Vector3.up);
                characterBody.transform.localRotation = yRotation * targetCharacterOrientation;
            }
            else
            {
                var yRotation = Quaternion.AngleAxis(mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
                transform.localRotation *= yRotation;
            }
        }
    }

}