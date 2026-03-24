using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClawbearGames
{
    public class PlayerController : MonoBehaviour
    {

        public static PlayerController Instance { private set; get; }
        public static event System.Action<PlayerState> PlayerStateChanged = delegate { };


        //[Header("Player Configuration")]

        [Header("Player References")]
        [SerializeField] private string objectLayer = "Object";
        [SerializeField] private string defaultLayer = "Default";
        [SerializeField] private Transform holeParentTrans = null;
        [SerializeField] private Transform fireEffectTrans = null;
        [SerializeField] private SpriteRenderer holeSpriteRenderer = null;
        [SerializeField] private ParticleSystem[] holeFireEffects = null;
        [SerializeField][Range(0.1f, 1f)] private float holeDetectionRadiusMultiplier = 0.75f;
        [SerializeField][Range(1f, 2f)] private float holeSizeMultiplier = 1.25f;
        [SerializeField] private HoleBalanceConfig holeBalanceConfig = null;
        [SerializeField] private HoleBalanceHierarchyConfig holeBalanceHierarchyConfig = null;
        [SerializeField] private HoleLevelProgressUI levelProgressUI = null;
        [SerializeField] private Sprite progressFrameSprite = null;
        [SerializeField] private Sprite progressFillSprite = null;
        [Header("Joystick")]
        [SerializeField] private CanvasGroup joystickCanvasGroup = null;
        [SerializeField] private RectTransform joystickRoot = null;
        [SerializeField] private RectTransform joystickKnob = null;
        [SerializeField] private float joystickRadius = 100f;
        [SerializeField] private HoleBalanceLevel[] fallbackHoleBalanceLevels = new HoleBalanceLevel[]
        {
            new HoleBalanceLevel(1, 0, 10, 1f, 0.9f, 5.2f, -3.2f),
            new HoleBalanceLevel(2, 10, 20, 1.4f, 1.1f, 6f, -3.55f),
            new HoleBalanceLevel(3, 30, 100, 1.9f, 1.25f, 6.5f, -3.8f),
            new HoleBalanceLevel(4, 130, 200, 2.45f, 1.45f, 7.5f, -4.25f),
            new HoleBalanceLevel(5, 330, 300, 2.9f, 1.75f, 8f, -4.5f),
            new HoleBalanceLevel(6, 630, 400, 3.3f, 2f, 8.5f, -4.7f),
            new HoleBalanceLevel(7, 1030, 500, 3.75f, 2.2f, 9f, -4.95f),
            new HoleBalanceLevel(8, 1530, 600, 4.2f, 2.4f, 9.5f, -5.2f),
            new HoleBalanceLevel(9, 2130, 700, 4.8f, 2.6f, 10.5f, -5.7f),
            new HoleBalanceLevel(10, 2830, 800, 5.5f, 2.8f, 11.5f, -6.2f),
            new HoleBalanceLevel(11, 3630, 900, 5.9f, 3f, 12f, -6.45f),
            new HoleBalanceLevel(12, 4530, 1000, 6.3f, 3.2f, 12.5f, -6.7f),
            new HoleBalanceLevel(13, 5530, 1000, 6.7f, 3.4f, 13f, -6.95f),
        };


        public PlayerState PlayerState
        {
            get { return playerState; }
            private set
            {
                if (value != playerState)
                {
                    value = playerState;
                    PlayerStateChanged(playerState);
                }
            }
        }

        private PlayerState playerState = PlayerState.Player_Prepare;
        private List<TargetObjectController> listCurrentTarget = new List<TargetObjectController>();
        private List<DeadlyObjectController> listCurrentDeadly = new List<DeadlyObjectController>();
        private List<TargetObjectController> listDetectedTarget = new List<TargetObjectController>();
        private List<DeadlyObjectController> listDetectedDeadly = new List<DeadlyObjectController>();
        private Vector3 firstInputPos = Vector3.zero;
        private Vector2 joystickInput = Vector2.zero;
        private float currentHoleSize = 1f;
        private float targetHoleSize = 1f;
        private float movementSpeed = 0f;
        private bool isStopControl = false;
        private bool isJoystickHolding = false;
        private int totalPoints = 0;
        private int currentBalanceLevelIndex = -1;
        private readonly List<Text> pointsPopupPool = new List<Text>();
        [SerializeField] private Vector3 pointsPopupWorldOffset = new Vector3(0f, 0.6f, 0f);

        private void OnEnable()
        {
            IngameManager.IngameStateChanged += IngameManager_IngameStateChanged;
        }
        private void OnDisable()
        {
            IngameManager.IngameStateChanged -= IngameManager_IngameStateChanged;
        }
        private void IngameManager_IngameStateChanged(IngameState obj)
        {
            if (obj == IngameState.Ingame_Playing)
            {
                PlayerLiving();
            }
            else if (obj == IngameState.Ingame_CompleteLevel)
            {
                PlayerCompletedLevel();
            }
        }




        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                DestroyImmediate(Instance.gameObject);
                Instance = this;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }



        private void Start()
        {
            //Fire event
            PlayerState = PlayerState.Player_Prepare;
            playerState = PlayerState.Player_Prepare;

            //Add other actions here

            //Setup character
            CharacterContainer characterContainer = ResolveCharacterContainer();
            if (characterContainer == null)
            {
                Debug.LogError("PlayerController: ServicesManager or CharacterContainer is not assigned in scene.");
                enabled = false;
                return;
            }

            CharacterInforController[] characters = characterContainer.CharacterInforControllers;
            if (characters == null || characters.Length == 0)
            {
                Debug.LogError("PlayerController: CharacterContainer has no characters assigned.");
                enabled = false;
                return;
            }

            int selectedCharacterIndex = characterContainer.SelectedCharacterIndex;
            if (selectedCharacterIndex < 0 || selectedCharacterIndex >= characters.Length)
            {
                Debug.LogError($"PlayerController: Selected character index {selectedCharacterIndex} is out of range.");
                enabled = false;
                return;
            }

            CharacterInforController charControl = characters[selectedCharacterIndex];
            holeSpriteRenderer.sprite = charControl.HoleSprite;
            // Preserve the character hole sprite colors (e.g. blue ring skins) instead of forcing black tint.
            holeSpriteRenderer.color = Color.white;
            DisableIdleHoleEffects();
            EnsureHoleCenterIsBlack();

            //Setup parameters and objects
            isStopControl = true;
            EnsureLevelProgressUI();
            ApplyBalanceByPoints();
            SetJoystickVisible(false);
        }

        private CharacterContainer ResolveCharacterContainer()
        {
            if (ServicesManager.Instance != null && ServicesManager.Instance.CharacterContainer != null)
            {
                return ServicesManager.Instance.CharacterContainer;
            }

            return FindObjectOfType<CharacterContainer>(true);
        }

        private void Update()
        {
            if (playerState == PlayerState.Player_Living)
            {
                if (!isStopControl)
                {
                    HandleJoystickInput();

                    if (joystickInput.sqrMagnitude > 0.0001f)
                    {
                        Vector3 movingDir = new Vector3(joystickInput.x, 0f, joystickInput.y);
                        Vector3 playerPos = transform.position;
                        playerPos += movingDir * movementSpeed * Time.deltaTime;
                        float effectiveHoleSize = targetHoleSize * holeSizeMultiplier;
                        transform.position = ClampPositionInsideScreen(playerPos, effectiveHoleSize * 0.5f);
                    }


                    //Check for deadly objects and deadly objects
                    listDetectedTarget.Clear();
                    listDetectedDeadly.Clear();
                    float effectiveDetectionSize = targetHoleSize * holeSizeMultiplier;
                    float holeDetectionRadius = effectiveDetectionSize * holeDetectionRadiusMultiplier;
                    Collider[] delectedColliders = Physics.OverlapSphere(transform.position, holeDetectionRadius);
                    foreach (Collider collider in delectedColliders)
                    {
                        if (collider.CompareTag("Object"))
                        {
                            TargetObjectController targetObject = PoolManager.Instance.FindTargetObject(collider.transform);
                            if (targetObject != null && !listDetectedTarget.Contains(targetObject))
                            {
                                listDetectedTarget.Add(targetObject);
                                targetObject.OnEnterPlayer(objectLayer);
                                if (!listCurrentTarget.Contains(targetObject)) { listCurrentTarget.Add(targetObject); }
                            }

                            DeadlyObjectController deadlyObject = PoolManager.Instance.FindDeadlyObject(collider.transform);
                            if(deadlyObject != null && !listDetectedDeadly.Contains(deadlyObject))
                            {
                                listDetectedDeadly.Add(deadlyObject);
                                deadlyObject.OnEnterPlayer(objectLayer);
                                if (!listCurrentDeadly.Contains(deadlyObject)) { listCurrentDeadly.Add(deadlyObject); }
                            }
                        }
                    }

                    //Target objects fall out of the hole -> re-set to default layer
                    foreach (TargetObjectController target in listCurrentTarget)
                    {
                        if (!listDetectedTarget.Contains(target))
                        {
                            target.OnExitPlayer(defaultLayer);
                        }
                    }

                    //deadly objects fall out of the hole -> re-set to default layer
                    foreach (DeadlyObjectController deadly in listCurrentDeadly)
                    {
                        if (!listDetectedDeadly.Contains(deadly))
                        {
                            deadly.OnExitPlayer(defaultLayer);
                        }
                    }

                    //Update hole size based on targetHoleSize
                    currentHoleSize = holeParentTrans.localScale.x;
                    float effectiveTargetHoleSize = targetHoleSize * holeSizeMultiplier;
                    if (currentHoleSize != effectiveTargetHoleSize)
                    {
                        currentHoleSize = Mathf.Clamp(currentHoleSize + Time.deltaTime, 1f, effectiveTargetHoleSize);
                        holeParentTrans.localScale = new Vector3(currentHoleSize, 1f, currentHoleSize);
                        HoleBalanceLevel currentLevel = GetBalanceLevelByIndex(Mathf.Max(currentBalanceLevelIndex, 0));
                        CameraParentController.Instance.UpdateDistance(currentLevel.CameraY, currentLevel.CameraZ);
                    }
                }
            }
        }



        /// <summary>
        /// Call PlayerState.Player_Living event and handle other actions.
        /// </summary>
        private void PlayerLiving()
        {
            //Fire event
            PlayerState = PlayerState.Player_Living;
            playerState = PlayerState.Player_Living;

            //Add other actions here
            if (IngameManager.Instance.IsRevived)
            {
                StartCoroutine(CRHandleActionsAfterRevived());
            }
            else
            {
                isStopControl = false;
                ResetJoystickState();
            }
        }


        /// <summary>
        /// Call PlayerState.Player_Died event and handle other actions.
        /// </summary>
        public void PlayerDied()
        {
            //Fire event
            PlayerState = PlayerState.Player_Died;
            playerState = PlayerState.Player_Died;

            //Add other actions here
            ServicesManager.Instance.SoundManager.PlaySound(ServicesManager.Instance.SoundManager.PlayerDied);
            ServicesManager.Instance.ShareManager.CreateScreenshot();
            CameraParentController.Instance.Shake();
            isStopControl = true;
            ResetJoystickState();
        }



        /// <summary>
        /// Fire Player_CompletedLevel event and handle other actions.
        /// </summary>
        private void PlayerCompletedLevel()
        {
            //Fire event
            PlayerState = PlayerState.Player_CompletedLevel;
            playerState = PlayerState.Player_CompletedLevel;

            //Add others action here
            ServicesManager.Instance.ShareManager.CreateScreenshot();
        }


        /// <summary>
        /// Coroutine handle actions after player revived.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CRHandleActionsAfterRevived()
        {
            yield return new WaitForSeconds(0.5f);
            isStopControl = false;
            ResetJoystickState();
        }

        private void HandleJoystickInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isJoystickHolding = true;
                firstInputPos = new Vector3(Input.mousePosition.x, 0f, Input.mousePosition.y);
                SetJoystickVisible(true);
                UpdateJoystickVisual(new Vector2(firstInputPos.x, firstInputPos.z), Vector2.zero);
            }
            else if (Input.GetMouseButton(0) && isJoystickHolding)
            {
                Vector2 rootPos = new Vector2(firstInputPos.x, firstInputPos.z);
                Vector2 currentInputPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                Vector2 delta = currentInputPos - rootPos;
                Vector2 clampedDelta = Vector2.ClampMagnitude(delta, joystickRadius);
                joystickInput = clampedDelta / Mathf.Max(1f, joystickRadius);
                UpdateJoystickVisual(rootPos, clampedDelta);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                ResetJoystickState();
            }
        }

        private void ResetJoystickState()
        {
            isJoystickHolding = false;
            firstInputPos = Vector3.zero;
            joystickInput = Vector2.zero;
            SetJoystickVisible(false);
            UpdateJoystickVisual(Vector2.zero, Vector2.zero);
        }

        private void SetJoystickVisible(bool isVisible)
        {
            if (joystickCanvasGroup == null)
            {
                return;
            }

            joystickCanvasGroup.alpha = isVisible ? 1f : 0f;
            joystickCanvasGroup.blocksRaycasts = false;
            joystickCanvasGroup.interactable = false;
        }

        private void UpdateJoystickVisual(Vector2 rootScreenPos, Vector2 knobOffset)
        {
            if (joystickRoot != null)
            {
                joystickRoot.position = rootScreenPos;
            }

            if (joystickKnob != null)
            {
                joystickKnob.position = rootScreenPos + knobOffset;
            }
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////Public functions



        /// <summary>
        /// Set the movement speed for the player.
        /// </summary>
        /// <param name="speed"></param>
        public void SetMovementSpeed(float speed)
        {
            movementSpeed = speed;
        }



        /// <summary>
        /// Update the size of the hole with amount. 
        /// </summary>
        /// <param name="amount"></param>
        public void UpdateHoleSize(float amount)
        {
            targetHoleSize = Mathf.Clamp(targetHoleSize + amount, 1f, 100f);
        }

        /// <summary>
        /// Handle score and growth when a target object is consumed.
        /// </summary>
        /// <param name="points"></param>
        public void OnTargetObjectConsumed(int points)
        {
            totalPoints += Mathf.Max(0, points);
            ShowPointsPopup(points);
            ApplyBalanceByPoints();
        }

        private void ApplyBalanceByPoints()
        {
            int levelIndex = GetBalanceLevelIndexByPoints(totalPoints);
            HoleBalanceLevel currentLevel = GetBalanceLevelByIndex(levelIndex);
            targetHoleSize = currentLevel.HeroScale;
            movementSpeed = currentLevel.HeroMoveSpeed;
            CameraParentController.Instance.UpdateDistance(currentLevel.CameraY, currentLevel.CameraZ);

            UpdateLevelProgressUi(levelIndex);

            if (levelIndex != currentBalanceLevelIndex)
            {
                currentBalanceLevelIndex = levelIndex;
            }
        }

        private int GetBalanceLevelIndexByPoints(int points)
        {
            if (holeBalanceHierarchyConfig != null && holeBalanceHierarchyConfig.HasLevels)
            {
                return holeBalanceHierarchyConfig.GetLevelIndexByPoints(points);
            }

            if (holeBalanceConfig != null)
            {
                return holeBalanceConfig.GetLevelIndexByPoints(points);
            }

            int result = 0;
            for (int i = 0; i < fallbackHoleBalanceLevels.Length; i++)
            {
                if (points >= fallbackHoleBalanceLevels[i].PointsSum)
                {
                    result = i;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private Vector3 ClampPositionInsideScreen(Vector3 desiredPosition, float holeRadius)
        {
            Camera cameraRef = Camera.main;
            if (cameraRef == null)
            {
                return desiredPosition;
            }

            Vector3 viewport = cameraRef.WorldToViewportPoint(desiredPosition);
            Vector3 viewportRight = cameraRef.WorldToViewportPoint(desiredPosition + cameraRef.transform.right * holeRadius);
            Vector3 viewportUp = cameraRef.WorldToViewportPoint(desiredPosition + cameraRef.transform.up * holeRadius);

            float viewportRadiusX = Mathf.Abs(viewportRight.x - viewport.x);
            float viewportRadiusY = Mathf.Abs(viewportUp.y - viewport.y);

            float clampedX = Mathf.Clamp(viewport.x, viewportRadiusX, 1f - viewportRadiusX);
            float clampedY = Mathf.Clamp(viewport.y, viewportRadiusY, 1f - viewportRadiusY);
            Ray ray = cameraRef.ViewportPointToRay(new Vector3(clampedX, clampedY, 0f));
            Plane movePlane = new Plane(Vector3.up, new Vector3(0f, desiredPosition.y, 0f));
            if (movePlane.Raycast(ray, out float enterDistance))
            {
                return ray.GetPoint(enterDistance);
            }

            return desiredPosition;
        }

        private HoleBalanceLevel GetBalanceLevelByIndex(int levelIndex)
        {
            if (holeBalanceHierarchyConfig != null && holeBalanceHierarchyConfig.HasLevels)
            {
                return holeBalanceHierarchyConfig.GetLevelByIndex(levelIndex);
            }

            if (holeBalanceConfig != null)
            {
                return holeBalanceConfig.GetLevelByIndex(levelIndex);
            }

            levelIndex = Mathf.Clamp(levelIndex, 0, fallbackHoleBalanceLevels.Length - 1);
            return fallbackHoleBalanceLevels[levelIndex];
        }

        private void UpdateLevelProgressUi(int levelIndex)
        {
            if (levelProgressUI == null)
            {
                return;
            }

            HoleBalanceLevel currentLevel = GetBalanceLevelByIndex(levelIndex);
            int nextPoints = currentLevel.NextPointsSum;
            int currentPoints = Mathf.Clamp(totalPoints, currentLevel.PointsSum, nextPoints);
            float normalized = Mathf.InverseLerp(currentLevel.PointsSum, nextPoints, currentPoints);
            levelProgressUI.SetProgress(currentLevel.Level, normalized);
        }

        private void EnsureLevelProgressUI()
        {
            if (levelProgressUI != null)
            {
                levelProgressUI.ConfigureSprites(progressFrameSprite, progressFillSprite);
                return;
            }

            levelProgressUI = GetComponentInChildren<HoleLevelProgressUI>(true);
            if (levelProgressUI == null)
            {
                levelProgressUI = gameObject.AddComponent<HoleLevelProgressUI>();
            }

            levelProgressUI.ConfigureSprites(progressFrameSprite, progressFillSprite);
        }

        private void EnsureHoleCenterIsBlack()
        {
            Transform holeBody = holeParentTrans != null ? holeParentTrans.Find("Hole_Body") : null;
            if (holeBody == null)
            {
                return;
            }

            MeshRenderer holeBodyRenderer = holeBody.GetComponent<MeshRenderer>();
            if (holeBodyRenderer == null || holeBodyRenderer.material == null)
            {
                return;
            }

            if (holeBodyRenderer.material.HasProperty("_Color"))
            {
                holeBodyRenderer.material.SetColor("_Color", Color.black);
            }

            if (holeBodyRenderer.material.HasProperty("_BaseColor"))
            {
                holeBodyRenderer.material.SetColor("_BaseColor", Color.black);
            }

            Vector3 bodyLocalPos = holeBody.localPosition;
            if (bodyLocalPos.y < -0.02f)
            {
                bodyLocalPos.y = -0.02f;
                holeBody.localPosition = bodyLocalPos;
            }
        }

        private void DisableIdleHoleEffects()
        {
            if (fireEffectTrans != null)
            {
                fireEffectTrans.gameObject.SetActive(false);
            }

            for (int i = 0; i < holeFireEffects.Length; i++)
            {
                if (holeFireEffects[i] != null)
                {
                    holeFireEffects[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    holeFireEffects[i].gameObject.SetActive(false);
                }
            }
        }

        private void ShowPointsPopup(int points)
        {
            if (points <= 0)
            {
                return;
            }

            Text popupText = CreatePointsPopupText();
            if (popupText == null)
            {
                return;
            }

            StartCoroutine(CRShowPointsPopup(popupText, points));
        }

        private IEnumerator CRShowPointsPopup(Text popupText, int points)
        {
            RectTransform popupRect = popupText.rectTransform;
            popupText.text = "+" + points.ToString();
            popupText.enabled = true;

            float t = 0f;
            const float duration = 1f;
            Color color = popupText.color;
            color.a = 1f;
            popupText.color = color;
            Camera cameraRef = Camera.main;

            while (t < duration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / duration);

                if (cameraRef == null)
                {
                    cameraRef = Camera.main;
                }

                if (cameraRef != null)
                {
                    Vector3 worldPos = transform.position + pointsPopupWorldOffset + Vector3.up * (normalized * 0.8f);
                    Vector3 screenPos = cameraRef.WorldToScreenPoint(worldPos);
                    popupRect.position = screenPos;
                    popupText.enabled = screenPos.z > 0f;
                }

                color.a = (normalized < 0.75f) ? 1f : Mathf.Lerp(1f, 0f, (normalized - 0.75f) / 0.25f);
                popupText.color = color;
                yield return null;
            }

            popupText.enabled = false;
            color.a = 1f;
            popupText.color = color;
        }

        private Text CreatePointsPopupText()
        {
            Canvas popupCanvas = FindObjectOfType<Canvas>();
            if (popupCanvas == null)
            {
                return null;
            }

            GameObject popupObject = new GameObject("PointsPopup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            popupObject.transform.SetParent(popupCanvas.transform, false);
            Text popupText = popupObject.GetComponent<Text>();
            popupText.alignment = TextAnchor.MiddleCenter;
            popupText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            popupText.fontSize = 42;
            popupText.fontStyle = FontStyle.Bold;
            popupText.color = Color.white;
            popupText.raycastTarget = false;
            pointsPopupPool.Add(popupText);
            return popupText;
        }



        /// <summary>
        /// Handle actions when the player collected a deadly object.
        /// </summary>
        public void OnCollectedDeadlyObject()
        {
            isStopControl = true;
            PlayerDied();
            IngameManager.Instance.HandlePlayerDied();
        }



        /// <summary>
        /// Create the cash effect with amount.
        /// </summary>
        /// <param name="amount"></param>
        public void CreateCashEffect(int amount)
        {
            EffectManager.Instance.CreateCashEffect(transform.position + Vector3.down * 0.5f, amount);
        }

    }
}
