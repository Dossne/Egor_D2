using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] private HoleBalanceConfig holeBalanceConfig = null;
        [SerializeField] private HoleLevelProgressUI levelProgressUI = null;
        [SerializeField] private HoleBalanceLevel[] fallbackHoleBalanceLevels = new HoleBalanceLevel[]
        {
            new HoleBalanceLevel(1, 0, 10, 1f, 0.9f, 3.2f, -1.5f),
            new HoleBalanceLevel(2, 10, 20, 1.4f, 1.1f, 4f, -1.85f),
            new HoleBalanceLevel(3, 30, 100, 1.9f, 1.25f, 4.5f, -2.1f),
            new HoleBalanceLevel(4, 130, 200, 2.45f, 1.45f, 5.5f, -2.55f),
            new HoleBalanceLevel(5, 330, 300, 2.9f, 1.75f, 6f, -2.8f),
            new HoleBalanceLevel(6, 630, 400, 3.3f, 2f, 6.5f, -3f),
            new HoleBalanceLevel(7, 1030, 500, 3.75f, 2.2f, 7f, -3.25f),
            new HoleBalanceLevel(8, 1530, 600, 4.2f, 2.4f, 7.5f, -3.5f),
            new HoleBalanceLevel(9, 2130, 700, 4.8f, 2.6f, 8.5f, -4f),
            new HoleBalanceLevel(10, 2830, 800, 5.5f, 2.8f, 9.5f, -4.5f),
            new HoleBalanceLevel(11, 3630, 900, 5.9f, 3f, 10f, -4.75f),
            new HoleBalanceLevel(12, 4530, 1000, 6.3f, 3.2f, 10.5f, -5f),
            new HoleBalanceLevel(13, 5530, 1000, 6.7f, 3.4f, 11f, -5.25f),
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
        private float currentHoleSize = 1f;
        private float targetHoleSize = 1f;
        private float movementSpeed = 0f;
        private float currentSpeed = 0f;
        private bool isStopControl = false;
        private int totalPoints = 0;
        private int currentBalanceLevelIndex = -1;

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
            CharacterInforController charControl = ServicesManager.Instance.CharacterContainer.CharacterInforControllers[ServicesManager.Instance.CharacterContainer.SelectedCharacterIndex];
            holeSpriteRenderer.sprite = charControl.HoleSprite;
            DisableIdleHoleEffects();

            //Setup parameters and objects
            isStopControl = true;
            EnsureLevelProgressUI();
            ApplyBalanceByPoints();
        }

        private void Update()
        {
            if (playerState == PlayerState.Player_Living)
            {
                if (!isStopControl)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        currentSpeed = movementSpeed / 2f;
                        firstInputPos = new Vector3(Input.mousePosition.x, 0f, Input.mousePosition.y);
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        //Update the current speed
                        currentSpeed = Mathf.Clamp(currentSpeed + Time.deltaTime, 0f, movementSpeed);

                        //Calculate the movingDir and move the player 
                        Vector3 currentInputPos = new Vector3(Input.mousePosition.x, 0f, Input.mousePosition.y);
                        Vector3 movingDir = (currentInputPos - firstInputPos).normalized;
                        Vector3 playerPos = transform.position;
                        playerPos += movingDir * currentSpeed * Time.deltaTime;
                        transform.position = ClampPositionInsideScreen(playerPos, targetHoleSize * 0.5f);
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                        currentSpeed = movementSpeed / 2f;
                        firstInputPos = Vector3.zero;
                    }


                    //Check for deadly objects and deadly objects
                    listDetectedTarget.Clear();
                    listDetectedDeadly.Clear();
                    float holeDetectionRadius = targetHoleSize * holeDetectionRadiusMultiplier;
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
                    if (currentHoleSize != targetHoleSize)
                    {
                        currentHoleSize = Mathf.Clamp(currentHoleSize + Time.deltaTime, 1f, targetHoleSize);
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
                EffectManager.Instance.CreateTargetObjectExplode(transform.position + Vector3.up * 0.15f, targetHoleSize);
            }
        }

        private int GetBalanceLevelIndexByPoints(int points)
        {
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
                return;
            }

            levelProgressUI = GetComponentInChildren<HoleLevelProgressUI>(true);
            if (levelProgressUI == null)
            {
                levelProgressUI = gameObject.AddComponent<HoleLevelProgressUI>();
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
