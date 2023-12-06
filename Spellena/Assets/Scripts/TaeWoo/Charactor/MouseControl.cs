using UnityEngine;


namespace Player
{
    public class MouseControl : MonoBehaviour
    {
        [Header("Settings")]
        public Vector2 clampInDegrees = new Vector2(360, 180);
        public bool eraseCursor = true;

        public Vector2 Sensitivity = new Vector2(6, 6);
        public bool isOpposite = false;

        [Header("First Person")]
        public GameObject characterBody;

        private Vector2 targetDirection;
        private Vector2 targetCharacterDirection;

        private Vector2 mouseAbsolute;
        private Vector2 smoothMouse;

        private Vector2 mouseDelta;
        private SettingManager settingManager;
        private bool showSettings = false;

        [HideInInspector]
        public bool scoped;

        void Start()
        {
            Init();
        }

        void Init()
        {
            // 카메라의 회전 값 저장
            targetDirection = transform.localRotation.eulerAngles;

            // 플레이어의 회전 값 저장
            if (characterBody)
                targetCharacterDirection = characterBody.transform.localRotation.eulerAngles;

            // 커서 고정
            if (eraseCursor)
                EraseCursor();

            LinkSettingManager();
        }

        void LinkSettingManager()
        {
            GameObject temp = GameObject.Find("SettingManager");

            if (temp == null)
            {
                Debug.LogError("SettingManager을 찾을 수 없습니다.");
                return;
            }

            settingManager = temp.GetComponent<SettingManager>();

            if (settingManager == null)
            {
                Debug.LogError("SettingManager의 Component을 찾을 수 없습니다.");
                return;
            }
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                showSettings = !showSettings;

                if (showSettings)
                {
                    ShowCursor();
                }

                else
                {
                    EraseCursor();
                }
            }

            if(showSettings)
            {
                SetData();
            }

            else
            {
                Control();
            }
        }

        public void EraseCursor()
        {
            // 커서를 숨기고 고정시킨다.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Control()
        {
            // Allow the script to clamp based on a desired target value.
            var targetOrientation = Quaternion.Euler(targetDirection);
            var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

            // Get raw mouse input for a cleaner reading on more sensitive mice.

            if (isOpposite)
            {
                mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y"));
            }

            else
            {
                mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            }

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

        void SetData()
        {
            Sensitivity = new Vector2(settingManager.sensitivityVal * 20, settingManager.sensitivityVal * 20);
            isOpposite = settingManager.isOpposite;      
        }

        public float ChangeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f)
                angle -= 360f;
            else if (angle < -180f)
                angle += 360f;
            return angle;
        }

        public void SetNewDirection()
        {
            Init();
            mouseAbsolute = Vector2.zero;
            smoothMouse = Vector2.zero;
            mouseDelta = Vector2.zero;
        }

        public void ApplyPos(float xPos, float yPos)
        {
            float newAbsoluteY = -ChangeAngle(yPos);
            if (newAbsoluteY < -60)
                newAbsoluteY = -60;
            if (newAbsoluteY > 60)
                newAbsoluteY = 60;
            mouseAbsolute.x = ChangeAngle(xPos);
            mouseAbsolute.y = newAbsoluteY;
        }
    }

}