﻿// TSS - Unity visual tweener plugin
// © 2018 ObelardO aka Vladislav Trubitsyn
// obelardos@gmail.com
// https://obeldev.ru/tss
// MIT License

using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TSS.Base
{
    [DisallowMultipleComponent]
    public class TSSBehaviour : MonoBehaviour
    {
        #region Properties

        private static TSSBehaviour _instance;
        public static TSSBehaviour instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject gameObject = new GameObject() { name = "TSS Behaviour", hideFlags = HideFlags./*HideAnd*/DontSave };
                    _instance = gameObject.AddComponent<TSSBehaviour>();
                    SceneManager.sceneUnloaded += Clear;
                    SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => SceneLoaded(scene, mode);
                    if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                }

                return _instance;
            }
        }

        private static bool _clearLists = false;
        public bool clearListsOnSceneLoad
        {
            set { _clearLists = value; }
            get { return _clearLists; }
        }

        private static List<TSSItem> updatingItems = new List<TSSItem>();
        private static List<TSSItem> fixedUpdatingItems = new List<TSSItem>();
        private static List<TSSItem> lateUpdateingItems = new List<TSSItem>();
        private static List<TSSCore> cores = new List<TSSCore>();

        #endregion

        #region Public methods

        public static void Load()
        {
            if (instance == null) return;
        }

        public static void Clear(Scene scene)
        {
            if (!_clearLists) return;
            updatingItems.Clear();
            fixedUpdatingItems.Clear();
            lateUpdateingItems.Clear();
            cores.Clear();
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {

            //TSSCore[] crs = FindObjectsOfType<TSSCore>();

            //Debug.Log(crs.Length);

            foreach (var core in cores) { core.SelectDefaultState(); }
        }

        public static void AddItem(TSSItem item)
        {
            switch (item.updatingType)
            {
                case ItemUpdateType.update:
                    if (!updatingItems.Contains(item)) updatingItems.Add(item); break;

                case ItemUpdateType.fixedUpdate:
                    if (!fixedUpdatingItems.Contains(item)) fixedUpdatingItems.Add(item); break;

                case ItemUpdateType.lateUpdate:
                    if (!lateUpdateingItems.Contains(item)) lateUpdateingItems.Add(item); break;
            }
        }

        public static void RemoveItem(TSSItem item)
        {
            switch (item.updatingType)
            {
                case ItemUpdateType.update: updatingItems.Remove(item); break;
                case ItemUpdateType.fixedUpdate: fixedUpdatingItems.Remove(item); break;
                case ItemUpdateType.lateUpdate: lateUpdateingItems.Remove(item); break;
            }
        }

        public static void AddCore(TSSCore core)
        {
            cores.Add(core);
        }

        public static void RemoveCore(TSSCore core)
        {
            cores.Remove(core);
        }

        #endregion

        #region Unity methods

        private void Awake()
        {
            if (_instance != null) DestroyImmediate(this);
        }

        private void Update()
        {
            for (int i = 0; i < updatingItems.Count; i++)
                UpdateItem(updatingItems[i], updatingItems[i].timeScaled ? Time.deltaTime : Time.unscaledDeltaTime);
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < fixedUpdatingItems.Count; i++)
                UpdateItem(fixedUpdatingItems[i], fixedUpdatingItems[i].timeScaled ? Time.fixedDeltaTime : Time.fixedUnscaledDeltaTime);
        }

        private void LateUpdate()
        {
            for (int i = 0; i < lateUpdateingItems.Count; i++)
                UpdateItem(lateUpdateingItems[i], lateUpdateingItems[i].timeScaled ? Time.deltaTime : Time.unscaledDeltaTime);
        }

        #endregion

        #region Updating

        private void UpdateItem(TSSItem item, float deltaTime)
        {
            item.deltaTime = deltaTime;

            if (item.path != null) item.path.UpdatePath();

            switch (item.state)
            {
                case ItemState.opening:
                    if (item.stateChgTime >= 0)
                    {
                        item.stateChgTime -= deltaTime;
                        if (item.stateChgTime > 0) break;
                        for (int i = 0; i < item.tweens.Count; i++) item.tweens[i].blendTime = 0;
                        if (item.stateChgBranchMode && !item.openChildBefore) TSSItemBase.OpenChilds(item);
                    }
                    else if (item.time < 1)
                    {
                        item.time += deltaTime / item.openDuration;
                    }
                    else if (item.childStateCounts[(int)ItemState.opened] == item.childCountWithoutLoops)
                    {
                        item.time = 1;
                        item.state = ItemState.opened;
                    }

                    item.UpdateInput();

                    for (int i = 0; i < item.tweens.Count; i++) item.tweens[i].Update();
                    break;

                case ItemState.closing:
                    if (item.stateChgTime >= 0)
                    {
                        item.stateChgTime -= deltaTime;
                        if (item.stateChgTime > 0) break;
                        for (int i = 0; i < item.tweens.Count; i++) item.tweens[i].blendTime = 0;
                        if (item.stateChgBranchMode && !item.closeChildBefore) TSSItemBase.CloseChilds(item);
                    }
                    else if (item.time > 0)
                    {
                        item.time -= deltaTime / item.closeDuration;
                    }
                    else if (item.childStateCounts[(int)ItemState.closed] == item.childCountWithoutLoops)
                    {
                        item.time = 0;
                        item.state = ItemState.closed;
                        if (!item.loopActivated) TSSBehaviour.RemoveItem(item);
                    }

                    for (int i = 0; i < item.tweens.Count; i++) item.tweens[i].Update();
                    break;

                case ItemState.closed:

                    if (!item.loopActivated || !Application.isPlaying) break;

                    if (item.currentLoops > 0 || item.loops < 0)
                    {
                        TSSItemBase.Activate(item, item.activationOpen);
                        item.loopActivated = true;
                        item.stateChgTime = 0;
                        break;
                    }

                    item.loopActivated = false;

                    return;

                case ItemState.opened:

                    item.UpdateInput();

                    item.UpdateMedia();

                    if (item.loops == 0 || !Application.isPlaying) break;

                    if (!item.loopActivated) { item.loopActivated = true; item.currentLoops = item.loops; }

                    if (item.currentLoops > 0 || item.loops < 0)
                    {
                        float time = item.time;

                        TSSItemBase.Activate(item, item.loopMode);
                        item.loopActivated = true;
                        if (item.loops > 0) item.currentLoops--;
                        if (item.loopMode == ActivationMode.closeImmediately ||
                            item.loopMode == ActivationMode.closeBranchImmediately)
                        {
                            item.time = time - 1;
                            UpdateItem(item, deltaTime);
                        }
                    }

                    break;
            }

            item.UpdateButton(deltaTime);
        }

        #endregion
    }
}
