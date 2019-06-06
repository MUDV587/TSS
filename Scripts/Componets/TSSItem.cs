﻿// TSS - Unity visual tweener plugin
// © 2018 ObelardO aka Vladislav Trubitsyn
// obelardos@gmail.com
// https://obeldev.ru/tss
// MIT License

using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;
using TSS.Base;

namespace TSS
{
    [Serializable, DisallowMultipleComponent, AddComponentMenu("TSS/Item")]
    #if UNITY_2018_3_OR_NEWER
        [ExecuteAlways]
    #else
        [ExecuteInEditMode]
    #endif
    public class TSSItem : MonoBehaviour
    {
        #region Properties

        /// <summary>Values container</summary>
        [HideInInspector] public TSSItemValues values;

        [HideInInspector] public int ID = 0;
        [HideInInspector, NonSerialized] public float time = 0;
        [HideInInspector, NonSerialized] public bool behaviourCatched;

        /// <summary>Update, FixedUpdate or LateUpdate</summary>
        [HideInInspector] public ItemUpdateType updatingType { get { return values.updatingType; } set { values.updatingType = value; } }
        /// <summary>Use time scaling</summary>
        [HideInInspector] public bool timeScaled { get { return values.timeScaled; } set { values.timeScaled = value; } }

        /// <summary>Activation mode for starting (activated once at awake, default is CloseBranchImmediately)</summary>
        [HideInInspector] public ActivationMode activationStart { get { return values.startAction; } set { values.startAction = value; } }
        /// <summary>Activation mode for opening (activated at Open() calling, default is OpenBranch)</summary>
        [HideInInspector] public ActivationMode activationOpen { get { return values.activations[1]; } set { values.activations[1] = value; } }
        /// <summary>Activation mode for closing (activated at Close() calling, default is CloseBranch)</summary>
        [HideInInspector] public ActivationMode activationClose { get { return values.activations[0]; } set { values.activations[0] = value; } }

        /// <summary>Time in seconds which item waits before start opening</summary>
        [HideInInspector] public float openDelay { set { values.delays[1] = value; } get { return values.delays[1]; } }

        /// <summary>Time in seconds which item waits before start closing</summary>
        [HideInInspector] public float closeDelay{ set { values.delays[0] = value; } get { return values.delays[0]; } }

        /// <summary>Time in seconds for which item opens</summary>
        [HideInInspector] public float openDuration { set { values.durations[1] = value; } get { return values.durations[1]; } }
        /// <summary>Time in seconds for which item closes</summary>
        [HideInInspector] public float closeDuration { set { values.durations[0] = value; } get { return values.durations[0]; } }

        /// <summary>Item's child open and close delays are controlled by this item</summary>
        [HideInInspector] public bool childChainMode { set { values.childChainMode = value; } get { return values.childChainMode; } }
        /// <summary>Item's child open and close delays are ignoring on a halfway</summary>
        [HideInInspector] public bool brakeChainDelay { set { values.brakeChainDelay = value; } get { return values.brakeChainDelay; } }
        /// <summary>Item's parent control this item's open and close delays/summary>
        [HideInInspector] public bool parentChainMode { get { if (parent == null) return false; else return parent.childChainMode; } }

        /// <summary>Item's child opens with own open delay without waiting this item open delay</summary>
        [HideInInspector] public bool openChildBefore { set { values.childBefore[1] = value; } get { return values.childBefore[1]; } }
        /// <summary>Item's child closes with own close delay without waiting this item close delay</summary>
        [HideInInspector] public bool closeChildBefore { set { values.childBefore[0] = value; } get { return values.childBefore[0]; } }

        /// <summary>Time in seconds which the next element in child chain wait before opening</summary>
        [HideInInspector] public float chainOpenDelay { set { values.chainDelays[1] = value; } get { return values.chainDelays[1]; } }
        /// <summary>Time in seconds which the next element in child chain wait before closing</summary>
        [HideInInspector] public float chainCloseDelay { set { values.chainDelays[0] = value; } get { return values.chainDelays[0]; } }

        /// <summary>Time in seconds which item's child chain waits before start opening</summary>
        [HideInInspector] public float firstChildOpenDelay { set { values.firstChildDelay[1] = value; } get { return values.firstChildDelay[1]; } }
        /// <summary>Time in seconds which item's child chain waits before start closing</summary>
        [HideInInspector] public float firstChildCloseDelay { set { values.firstChildDelay[0] = value; } get { return values.firstChildDelay[0]; } }

        /// <summary>Order of opening child items (auto control child open delays)</summary>
        [HideInInspector] public ChainDirection chainOpenDirection { set { values.chainDirections[1] = value; this.UpdateItemDelaysInChain((int)ItemKey.opened); } get { return values.chainDirections[1]; } }
        /// <summary>Order of closing child items (auto control child close delays)</summary>
        [HideInInspector] public ChainDirection chainCloseDirection { set { values.chainDirections[0] = value; this.UpdateItemDelaysInChain((int)ItemKey.closed); } get { return values.chainDirections[0]; } }

        /// <summary>Interpolating rotation mode</summary>
        [HideInInspector] public RotationMode rotationMode { set { values.rotationMode = value; } get { return values.rotationMode; } }
        /// <summary>Direct or Instance</summary>
        [HideInInspector] public MaterialMode materialMode { set { values.materialMode = value; RefreshMaterial(); } get { return values.materialMode; } }

        /// <summary>Which axis controlled by path rotation</summary>
        [HideInInspector] public Vector3 rotationMask { set { values.rotationMask = value; } get { return values.rotationMask; } }
        [HideInInspector] public bool rotationMaskX { set { values.rotationMask.x = value ? 1 : 0; } get { return values.rotationMask.x == 1; } }
        [HideInInspector] public bool rotationMaskY { set { values.rotationMask.y = value ? 1 : 0; } get { return values.rotationMask.y == 1; } }
        [HideInInspector] public bool rotationMaskZ { set { values.rotationMask.z = value ? 1 : 0; } get { return values.rotationMask.z == 1; } }

        /// <summary>Path alignment vector (for eliminate sharp turns, Up for 3D and Forward for 2D usually)</summary>
        [HideInInspector] public PathNormal pathNormal { set { values.pathNormal = value; } get { return values.pathNormal; } }

        /// <summary>Control interactable components</summary>
        [HideInInspector] public bool interactions { set { values.interactions = value; } get { return values.interactions; } }
        /// <summary>Control components with raycast target</summary>
        [HideInInspector] public bool blockRaycasting { set { values.blockRaycasting = value; } get { return values.blockRaycasting; } }

        /// <summary>Control AudioSource component</summary>
        [HideInInspector] public bool soundControl { set { values.soundControl = value; } get { return values.soundControl; } }
        /// <summary>Restart AudioSource playing at opening</summary>
        [HideInInspector] public bool soundRestart { set { values.soundRestart = value; } get { return values.soundRestart; } }

        /// <summary>Control VideoPlauer component</summary>
        [HideInInspector] public bool videoControl { set { values.videoControl = value; } get { return values.videoControl; } }
        /// <summary>Restart VideoPlauer playing at opening</summary>
        [HideInInspector] public bool videoRestart { set { values.videoRestart = value; } get { return values.videoRestart; } }

        /// <summary>Length of random symbols in text interpolation</summary>
        [HideInInspector] public int randomWave { set { values.randomWaveLength = value; } get { return values.randomWaveLength; } }
        /// <summary>Format for number to text converting</summary>
        [HideInInspector] public string floatFormat { set { values.floatFormat = value; } get { return values.floatFormat; } }

        /// <summary>Don't consider child in inheriting</summary>
        [HideInInspector] public bool ignoreChilds { set { values.ignoreChilds = value; Refresh(); } get { return values.ignoreChilds; } }
        /// <summary>Don't consider parent in inheriting</summary>
        [HideInInspector] public bool ignoreParent { set { values.ignoreParent = value; Refresh(); } get { return values.ignoreParent; } }

        /// <summary>Time in seconds which button animation is playing</summary>
        [HideInInspector] public float buttonDuration { set { values.buttonDuration = value; } get { return values.buttonDuration; } }
        [HideInInspector] private float _buttonEvaluation;
        /// <summary>Button animation evaluation</summary>
        [HideInInspector] public float buttonEvaluation { set { _buttonEvaluation = value; for (int i = 0; i < childItems.Count; i++) childItems[i].buttonEvaluation = value; } get { return _buttonEvaluation; } }
        /// <summary>what item state to use as the pressed button state</summary>
        [HideInInspector] public ButtonDirection buttonDirection { set { values.buttonDirection = value; } get { return values.buttonDirection; } }

        /// <summary>Item evaluation</summary>
        [HideInInspector, NonSerialized] public float evaluation;
        /// <summary>Item delteTime affected by updating type and time scaling</summary>
        [HideInInspector, NonSerialized] public float deltaTime;

        /// <summary>Count of item loops. Loop activation will start after every opening (-1 as infinity loop)</summary>
        [HideInInspector] public int loops { set { values.loops = value; Refresh(); } get { return values.loops; } }
        /// <summary>Loop activation mode what start after opening</summary>
        [HideInInspector] public ActivationMode loopMode { set { values.loopMode = value; Refresh(); } get { return values.loopMode; } }
        /// <summary>Current item activation inwoked by loop</summary>
        [HideInInspector] public bool loopActivated;
        /// <summary>Number of loops remaining</summary>
        [HideInInspector] public int currentLoops;
        /// <summary>Count of child items without any loops</summary>
        [HideInInspector] public int childCountWithoutLoops;

        /// <summary>Event invoked after item completely closed</summary>
        [HideInInspector] public UnityEvent OnClosed;
        /// <summary>Event invoked at item start opeing</summary>
        [HideInInspector] public UnityEvent OnOpening;
        /// <summary>Event invoked after item completely opened</summary>
        [HideInInspector] public UnityEvent OnOpened;
        /// <summary>Event invoked at item start closing</summary>
        [HideInInspector] public UnityEvent OnClosing;

        /// <summary>Item current state</summary>
        [HideInInspector, NonSerialized] public ItemState _state;
        public ItemState state
        {
            set
            {
                if (_state != value)
                {
                    bool enable = false;

                    if (value == ItemState.opening || value == ItemState.opened)
                    {
                        enable = true;
                    }
                    else if (value != ItemState.opened)
                    {
                        enable = false;
                    }

                    if (interactions)
                    {
                        if (colider != null) colider.enabled = enable;
                        if (button != null) button.interactable = enable;
                        if (canvasGroup != null) canvasGroup.interactable = enable;
                    }

                    if (blockRaycasting)
                    {
                        if (rawImage != null) rawImage.raycastTarget = enable;
                        if (image != null) image.raycastTarget = enable;
                        if (canvasGroup != null) canvasGroup.blocksRaycasts = enable;
                        if (text != null) text.raycastTarget = enable;
                    }

                    if (soundControl && audioPlayer != null)
                    {
                        if (enable && !audioPlayer.isPlaying) audioPlayer.Play();
                        else if (value == ItemState.closed) { if (soundRestart) audioPlayer.Stop(); else audioPlayer.Pause(); }
                    }

                    if (videoControl && videoPlayer != null)
                    {
                        if (enable && !videoPlayer.isPlaying) videoPlayer.Play();
                        else if (value == ItemState.closed) { if (videoRestart) videoPlayer.Stop(); else videoPlayer.Pause(); }
                    }

                    if (loops == 0 && !ignoreParent && parent != null && !parent.ignoreChilds && _state != ItemState.slave && parent.childStateCounts[(int)_state] > 0) parent.childStateCounts[(int)_state] -= 1;

                    _state = value;

                    switch (_state)
                    {
                        case ItemState.closed: if (OnClosed != null && OnClosed.GetPersistentEventCount() > 0) OnClosed.Invoke(); break;
                        case ItemState.opening: if (OnOpening != null && OnOpening.GetPersistentEventCount() > 0) OnOpening.Invoke(); break;
                        case ItemState.opened:  if (OnOpened != null && OnOpened.GetPersistentEventCount() > 0) OnOpened.Invoke(); break;
                        case ItemState.closing: if (OnClosing != null && OnClosing.GetPersistentEventCount() > 0) OnClosing.Invoke(); break;
                    }

                    if (loops == 0 && !ignoreParent && parent != null && !parent.ignoreChilds && _state != ItemState.slave) parent.childStateCounts[(int)_state] += 1;
                }
            }
            get { return _state; }
        }

        /// <summary>Item is completely opened</summary>
        [HideInInspector] public bool isOpened { get { return _state == ItemState.opened; } }
        /// <summary>Item is completely closed</summary>
        [HideInInspector] public bool IsClosed { get { return _state == ItemState.closed; } }
        /// <summary>Item is opening</summary>
        [HideInInspector] public bool isOpening { get { return _state == ItemState.opening; } }
        /// <summary>Item is closing</summary>
        [HideInInspector] public bool isClosing { get { return _state == ItemState.closing; } }
        /// <summary>Item's evaluation is controlled from external script</summary>
        [HideInInspector] public bool isSlave { get { return _state == ItemState.slave; } }

        /// <summary>Item's child states</summary>
        [HideInInspector, NonSerialized] public int[] childStateCounts = new int[4];
        [HideInInspector, NonSerialized] public float stateChgTime;
        [HideInInspector, NonSerialized] public bool stateChgBranchMode;

        /// <summary>List of child</summary>
        [HideInInspector] public List<TSSItem> childItems = new List<TSSItem>();
        /// <summary>List of attached tweens</summary>
        [HideInInspector] public List<TSSTween> tweens = new List<TSSTween>();
        /// <summary>parent item</summary>
        [HideInInspector] public TSSItem parent;
        
        [HideInInspector, SerializeField] private TSSProfile _profile;
        /// <summary>Attached profile</summary>
        [HideInInspector, SerializeField] public TSSProfile profile { set { _profile = value; } get { return _profile; } }

        [HideInInspector, NonSerialized] public Transform chtransform;
        [HideInInspector, NonSerialized] public CanvasGroup canvasGroup;
        [HideInInspector, NonSerialized] public Image image;
        [HideInInspector, NonSerialized] public RawImage rawImage;
        [HideInInspector, NonSerialized] public Text text;
        [HideInInspector, NonSerialized] public TSSGradient gradient;
        [HideInInspector, NonSerialized] public RectTransform rect;
        [HideInInspector, NonSerialized] public Button button;
        [HideInInspector, NonSerialized] public Collider colider;
        [HideInInspector, NonSerialized] public AudioSource audioPlayer;
        [HideInInspector, NonSerialized] public VideoPlayer videoPlayer;
        [HideInInspector, NonSerialized] public Material material;
        [HideInInspector, NonSerialized] public SphereCollider sphereCollider;
        [HideInInspector, NonSerialized] public Renderer itemRenderer;
        [HideInInspector, NonSerialized] public Light itemLight;
        [HideInInspector, NonSerialized] public TSSPath path;
        [HideInInspector] public string stringPart;

        #endregion

        #region Runtime activation methods

        /// <summary>Open item with open activation mode (OpenBranch as default)</summary>
        public void Open() { TSSItemBase.Activate(this, activationOpen); }

        /// <summary>Close item with close activation mode (CloseBranch as default)</summary>
        public void Close() { TSSItemBase.Activate(this, activationClose); }

        /// <summary>Open (if closed) or Close (if opened) item with close and open activation modes (OpenBranch and CloseBranch as defaults)</summary>
        public void OpenClose() { if (state == ItemState.closing || state == ItemState.closed) Open(); else Close(); }

        /// <summary>Activate item with specified activation mode</summary>
        /// <param name="mode">activation mode</param>
        public void Activate(ActivationMode mode) { TSSItemBase.Activate(this, mode); }

        /// <summary>Controll item's branch (item with child and child of child of ... etc.) manualy</summary>
        /// <param name="value">time between 0 and 1</param>
        public void EvaluateBranch(float value) { TSSItemBase.EvaluateBranch(this, value); }

        /// <summary>Controll item (only one) manualy</summary>
        /// <param name="value">time between 0 and 1</param>
        public void Evaluate(float value) { TSSItemBase.Evaluate(this, value); }

        /// <summary>Controll item's branch (item with child and child of child of ... etc.) manualy</summary>
        /// <param name="value">time between 0 and 1</param>
        /// <param name="direction">use tween with specified direction</param>
        public void EvaluateBranch(float value, ItemKey direction) { TSSItemBase.EvaluateBranch(this, value, direction); }

        /// <summary>Controll item (only one) manualy</summary>
        /// <param name="value">time between 0 and 1</param>
        /// <param name="direction">use tween with specified direction</param>
        public void Evaluate(float value, ItemKey direction) { TSSItemBase.Evaluate(this, value, direction); }

        /// <summary>Open only this item without activation mode</summary>
        public void OpenSinge() { TSSItemBase.Activate(this, ActivationMode.open); }

        /// <summary>Close only this item without activation mode</summary>
        public void CloseSingle() { TSSItemBase.Activate(this, ActivationMode.close); }

        /// <summary>Open (if closed) or close (if opened) only this item without activation modes</summary>
        public void OpenCloseSingle() { TSSItemBase.Activate(this, ActivationMode.openClose); }

        /// <summary>Open this item's branch without activation mode</summary>
        public void OpenBranch() { TSSItemBase.Activate(this, ActivationMode.openBranch); }

        /// <summary>Close this item branch without activation mode</summary>
        public void CloseBranch() { TSSItemBase.Activate(this, ActivationMode.closeBranch); }

        /// <summary>Open (if closed) or close (if opened) this item's branch without activation modes</summary>
        public void OpenCloseBranch() { TSSItemBase.Activate(this, ActivationMode.openCloseBranch); }

        /// <summary>Open only this item immediately without activation mode</summary>
        public void OpenImmediately() { TSSItemBase.Activate(this, ActivationMode.openImmediately); }

        /// <summary>Close only this item immediately without activation mode</summary>
        public void CloseImmediately() { TSSItemBase.Activate(this, ActivationMode.closeImmediately); }

        /// <summary>Open (if closed) or close (if opened) this item immediately without activation modes</summary>
        public void OpenCloseImmediately() { TSSItemBase.Activate(this, ActivationMode.openCloseImmediately); }

        /// <summary>Open this item's branch immediately without activation mode</summary>
        public void OpenBranchImmediately() { TSSItemBase.Activate(this, ActivationMode.openBranchImmediately); }

        /// <summary>Close this item's branch immediately without activation mode</summary>
        public void CloseBranchImmediately() { TSSItemBase.Activate(this, ActivationMode.closeBranchImmediately); }

        /// <summary>Open (if closed) or close (if opened) this item's branch immediately without activation modes</summary>
        public void OpenCloseBranchImmediately() { TSSItemBase.Activate(this, ActivationMode.openCloseBranchImmediately); }

        #endregion

        #region Refreshing

        /// <summary>Refresh items inheritance and components (automatically only at editor, called once on awake at runtime)</summary>
        public void Refresh()
        {
            if (gameObject == null) return;

            chtransform = transform;

            TSSItem[] childs = GetComponentsInChildren<TSSItem>();

            childItems.Clear();
            childCountWithoutLoops = 0;

            for (int i = 0; i < childs.Length; i++)
            {
                if (childs[i] == this || childs[i].ignoreParent || TSSItemBase.GetItemParentTransform(childs[i]) != chtransform || !childs[i].enabled) continue;

                if (!ignoreChilds)
                {
                    childItems.Add(childs[i]);
                    childs[i].ID = childItems.Count;
                    childs[i].parent = this;
                    if (childs[i].loops == 0) childCountWithoutLoops++;
                }
                else
                {
                    childs[i].ID = 1;
                    childs[i].parent = null;
                }
            }

            Transform parentTransform = TSSItemBase.GetItemParentTransform(this);
            if (ignoreParent) parent = null; else parent = parentTransform == null ? null : parentTransform.GetComponent<TSSItem>();
            if (parent != null) parent.Refresh();

            this.UpdateItemDelaysInChain((int)ItemKey.closed);
            this.UpdateItemDelaysInChain((int)ItemKey.opened);

            canvasGroup = GetComponent<CanvasGroup>();
            image = GetComponent<Image>();
            rawImage = GetComponent<RawImage>();
            text = GetComponent<Text>();
            gradient = GetComponent<TSSGradient>();
            rect = GetComponent<RectTransform>();
            button = GetComponent<Button>();
            colider = GetComponent<Collider>();
            audioPlayer = GetComponent<AudioSource>();
            videoPlayer = GetComponent<VideoPlayer>();
            sphereCollider = GetComponent<SphereCollider>();
            itemLight = GetComponent<Light>();
            itemRenderer = GetComponent<Renderer>();
            path = GetComponent<TSSPath>();

            if (button != null) button.onClick.AddListener(OnClick);

            RefreshMaterial();
            if (path != null) path.Refresh();
        }

        private void RefreshMaterial()
        {
            material = null;

            if (itemRenderer != null)
            {
                if (materialMode == MaterialMode.direct) material = itemRenderer.sharedMaterial;
                else if (Application.isPlaying) material = itemRenderer.material;
                else material = itemRenderer.sharedMaterial;
            }
            else if (image != null && materialMode == MaterialMode.direct)
            {
                material = image.material == null ? image.defaultMaterial : image.material;
            }
            else if (rawImage != null && materialMode == MaterialMode.direct)
            {
                material = rawImage.material == null ? rawImage.defaultMaterial : rawImage.material;
            }
        }

        #endregion

        #region Unity methods

        private void Awake()
        {
            TSSItemBase.AllItems.Add(this);
            TSSItemBase.InitValues(ref values);
            TSSItemBase.DoAllEffects(this, 0);
            state = ItemState.closed;
        }

        private void OnDestroy()
        {
            TSSItemBase.AllItems.Remove(this);
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        private void Reset()
        {
            values = new TSSItemValues();
            TSSItemBase.InitValues(ref values);
        }

        private void OnDrawGizmos()
        {

        }

        #endregion

        #region Update Methods

        private void OnClick()
        {
            if (buttonEvaluation <= 0) buttonEvaluation = buttonDuration;
            for (int i = 0; i < childItems.Count; i++) if (childItems[i].buttonEvaluation <= 0) childItems[i].buttonEvaluation = childItems[i].buttonDuration;
        }

        public void UpdateMedia()
        {
            if (videoControl && videoPlayer != null && !videoPlayer.isLooping && videoPlayer.isPlaying
                && (((videoPlayer.frameCount / videoPlayer.frameRate) - (videoPlayer.frame / videoPlayer.frameRate)) * videoPlayer.playbackSpeed) <= closeDuration + closeDelay) Close();

            if (videoPlayer == null && soundControl && audioPlayer != null && !audioPlayer.loop && audioPlayer.isPlaying
                && (audioPlayer.clip.length - audioPlayer.time) <= closeDuration + closeDelay) Close();
        }

        public void UpdateInput()
        {
            if (button == null || !button.interactable || !Input.anyKeyDown) return;
            for (int i = 0; i < values.onKeyboard.Count; i++) if (Input.GetKeyDown(values.onKeyboard[i])) button.onClick.Invoke();
        }

        public void UpdateButton(float deltaTime)
        {
            if (buttonEvaluation > 0)
            {
                buttonEvaluation -= deltaTime;
                if (buttonEvaluation < 0) buttonEvaluation = 0;

                for (int i = 0; i < tweens.Count; i++)
                {
                    if (!tweens[i].enabled || tweens[i].direction != TweenDirection.Button) continue;
                    TSSItemBase.DoEffect(this, tweens[i].Evaluate(buttonDirection == ButtonDirection.open2Close ?
                        buttonEvaluation / buttonDuration :
                        1 - (buttonEvaluation / buttonDuration), tweens[i].type), tweens[i].effect);
                }
            }
        }

        #endregion
    }
}