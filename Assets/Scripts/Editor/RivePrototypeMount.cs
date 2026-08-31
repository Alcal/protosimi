#if UNITY_EDITOR
using ManosLimpias.Core;
using ManosLimpias.UI;
using ManosLimpias.UI.Rive;
using Rive;
using Rive.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManosLimpias.Editor
{
    /// <summary>
    /// Mounts simi_prototype main + intro widgets on GameplayCanvas.
    /// </summary>
    public static class RivePrototypeMount
    {
        const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
        const string RivAssetPath = SimiPrototypeArtboards.FilePath;

        [MenuItem("ManosLimpias/Mount Simi Prototype Rive")]
        public static void MountOnGameplay()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != GameplayScenePath)
            {
                scene = EditorSceneManager.OpenScene(GameplayScenePath);
            }

            AssetDatabase.ImportAsset(RivAssetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
            var asset = AssetDatabase.LoadAssetAtPath<Asset>(RivAssetPath);
            if (asset == null)
            {
                Debug.LogError($"Rive asset missing at {RivAssetPath}. Wait for Unity to import, then retry.");
                return;
            }

            var canvas = GameObject.Find("GameplayCanvas");
            if (canvas == null)
            {
                Debug.LogError("GameplayCanvas not found in Gameplay scene.");
                return;
            }

            var systems = GameObject.Find("Systems");
            if (systems == null)
            {
                Debug.LogError("Systems not found in Gameplay scene.");
                return;
            }

            var panelTf = canvas.transform.Find("Rive Panel");
            GameObject panelObj = panelTf != null ? panelTf.gameObject : null;
            if (panelObj == null)
            {
                panelObj = new GameObject("Rive Panel", typeof(RectTransform), typeof(RivePanel), typeof(RiveCanvasRenderer));
                panelObj.transform.SetParent(canvas.transform, false);
                Stretch(panelObj.GetComponent<RectTransform>());
            }
            else
            {
                if (panelObj.GetComponent<RivePanel>() == null)
                    panelObj.AddComponent<RivePanel>();
                if (panelObj.GetComponent<RiveCanvasRenderer>() == null)
                    panelObj.AddComponent<RiveCanvasRenderer>();
                Stretch(panelObj.GetComponent<RectTransform>());
            }

            panelObj.transform.SetAsLastSibling();

            var renderer = panelObj.GetComponent<RiveCanvasRenderer>();
            var soRenderer = new SerializedObject(renderer);
            var initialPanel = soRenderer.FindProperty("m_initialRivePanel");
            if (initialPanel != null)
                initialPanel.objectReferenceValue = panelObj.GetComponent<RivePanel>();
            soRenderer.ApplyModifiedPropertiesWithoutUndo();

            var mainWidget = FindOrCreateWidget(panelObj.transform, "MainRive");
            var introWidget = FindOrCreateWidget(panelObj.transform, "IntroRive");
            var stepIconWidget = FindOrCreateWidget(panelObj.transform, "StepIconRive");
            mainWidget.transform.SetSiblingIndex(0);
            stepIconWidget.transform.SetSiblingIndex(1);
            introWidget.transform.SetSiblingIndex(2);

            ConfigureWidget(
                mainWidget,
                asset,
                MainProgress.Artboard,
                MainProgress.StateMachine,
                HitTestBehavior.Opaque,
                Fit.Contain,
                RiveWidget.DataBindingMode.Manual);

            ConfigureWidget(
                stepIconWidget,
                asset,
                StepIcon.Artboard,
                StepIcon.StateMachine,
                HitTestBehavior.None,
                Fit.Contain,
                RiveWidget.DataBindingMode.Manual);
            PlaceStepIcon(stepIconWidget.GetComponent<RectTransform>());

            ConfigureWidget(
                introWidget,
                asset,
                Intro.Artboard,
                Intro.StateMachine,
                HitTestBehavior.Opaque,
                Fit.Contain,
                RiveWidget.DataBindingMode.AutoBindDefault);

            introWidget.gameObject.SetActive(true);

            var presenter = systems.GetComponent<SimiPrototypePresenter>();
            if (presenter == null)
                presenter = systems.AddComponent<SimiPrototypePresenter>();
            var binder = systems.GetComponent<RiveHudBinder>();
            if (binder == null)
                binder = systems.AddComponent<RiveHudBinder>();
            var faucet = mainWidget.GetComponent<Faucet>();
            if (faucet == null)
                faucet = mainWidget.gameObject.AddComponent<Faucet>();
            faucet.Bind(mainWidget);

            var soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("flow").objectReferenceValue = systems.GetComponent<GameFlowController>();
            soPresenter.FindProperty("mainWidget").objectReferenceValue = mainWidget;
            soPresenter.FindProperty("introWidget").objectReferenceValue = introWidget;
            soPresenter.FindProperty("stepIconWidget").objectReferenceValue = stepIconWidget;
            soPresenter.FindProperty("hudBinder").objectReferenceValue = binder;
            soPresenter.FindProperty("faucet").objectReferenceValue = faucet;
            soPresenter.ApplyModifiedPropertiesWithoutUndo();

            var soBinder = new SerializedObject(binder);
            soBinder.FindProperty("hud").objectReferenceValue = systems.GetComponent<HudPresenter>();
            soBinder.FindProperty("mainWidget").objectReferenceValue = mainWidget;
            soBinder.FindProperty("stepIconWidget").objectReferenceValue = stepIconWidget;
            soBinder.ApplyModifiedPropertiesWithoutUndo();

            var soFlow = new SerializedObject(systems.GetComponent<GameFlowController>());
            soFlow.FindProperty("faucet").objectReferenceValue = faucet;
            soFlow.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Mounted simi_prototype main + intro on GameplayCanvas.");
        }

        static RiveWidget FindOrCreateWidget(Transform panel, string name)
        {
            var existing = panel.Find(name);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
                if (go.GetComponent<RiveWidget>() == null)
                    go.AddComponent<RiveWidget>();
            }
            else
            {
                go = new GameObject(name, typeof(RectTransform), typeof(RiveWidget));
                go.transform.SetParent(panel, false);
            }

            Stretch(go.GetComponent<RectTransform>());
            return go.GetComponent<RiveWidget>();
        }

        static void ConfigureWidget(
            RiveWidget widget,
            Asset asset,
            string artboard,
            string stateMachine,
            HitTestBehavior hitTest,
            Fit fit,
            RiveWidget.DataBindingMode bindingMode)
        {
            var so = new SerializedObject(widget);
            so.FindProperty("m_asset").objectReferenceValue = asset;
            so.FindProperty("m_artboardName").stringValue = artboard;
            so.FindProperty("m_stateMachineName").stringValue = stateMachine;
            so.FindProperty("m_hitTestBehavior").enumValueIndex = (int)hitTest;
            so.FindProperty("m_fit").enumValueIndex = (int)fit;
            so.FindProperty("m_dataBindingMode").enumValueIndex = (int)bindingMode;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        static void PlaceStepIcon(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.82f, 0.31f);
            rect.anchorMax = new Vector2(0.98f, 0.55f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }
    }
}
#endif
