using UnityEngine;
using Photon.Pun;


namespace Player
{
    public class ObserverControl : MonoBehaviourPunCallbacks
    {
        [Header("Settings")]
        public Vector2 clampInDegrees = new Vector2(360, 180);
        public bool eraseCursor = true;

        public Vector2 Sensitivity = new Vector2(6, 6);
        public bool isOpposite = false;

        public float moveSpeed = 0.0f;
        public float runSpeedRatio = 1.5f;

        private Vector2 targetDirection;
        private Vector2 targetCharacterDirection;

        private Vector2 mouseAbsolute;
        private Vector2 smoothMouse;

        private Vector2 mouseDelta;
        private SettingManager settingManager;

        private Vector3 moveVec;

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
            if (settingManager.SettingPanel.activeSelf)
            {
                SetData();
                ShowCursor();
            }

            else
            {
                SightControl();
                MoveControl();
                EraseCursor();
            }
        }

        public void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void EraseCursor()
        {
            // 커서를 숨기고 고정시킨다.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void SightControl()
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

            var yRotation = Quaternion.AngleAxis(mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
            transform.localRotation *= yRotation;
        }

        void MoveControl()
        {
            moveVec = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            moveVec.Normalize();

            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.Translate(moveVec * moveSpeed*runSpeedRatio*Time.deltaTime);
            }

            else
            {
                transform.Translate(moveVec * moveSpeed*Time.deltaTime);
            }
        }

        void SetData()
        {
            Sensitivity = new Vector2(settingManager.sensitivityVal * 20, settingManager.sensitivityVal * 20);
            isOpposite = settingManager.isOpposite;
        }

        [PunRPC]
        public void SetEnemyLayer()
        {
            Character[] characters = FindObjectsOfType<Character>();
            if (characters == null) return;

            foreach (Character character in characters)
            {
                if (!character.gameObject.CompareTag(tag))
                {
                    OutlineDrawer[] outlineDrawers = character.gameObject.GetComponentsInChildren<OutlineDrawer>();
                    if (outlineDrawers == null) return;

                    foreach (OutlineDrawer outline in outlineDrawers)
                    {
                        outline.enabled = true;
                    }
                }
            }
        }

        [PunRPC]
        public void ChangeName(string name)
        {
            gameObject.name = name;
        }

        [PunRPC]
        public void SetTag(string team)
        {
            this.tag = team;
        }
    }

}