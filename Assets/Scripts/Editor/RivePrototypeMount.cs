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
    /// Mounts simi_prototype background + anchored component widgets on GameplayCanvas.
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
                scene = EditorSceneManager.OpenScene(GameplayScenePath);

            AssetDatabase.ImportAsset(RivAssetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
            var asset = AssetDatabase.LoadAssetAtPath<Asset>(RivAssetPath);
            if (asset == null)
            {
                Debug.LogError($"Rive asset missing at {RivAssetPath}. Wait for Unity to import, then retry.");
                return;
            }

            LogFileInventory(asset);

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

            RenameIfExists(panelObj.transform, "MainRive", Background.WidgetName);
            RenameIfExists(panelObj.transform, "StepIconRive", StepIcon.WidgetNames[0]);

            var backgroundWidget = FindOrCreateWidget(panelObj.transform, Background.WidgetName);
            var faucetWidget = FindOrCreateWidget(panelObj.transform, Faucet.WidgetName);
            var handsWidget = FindOrCreateWidget(panelObj.transform, "HandsRive");
            var soapWidget = FindOrCreateWidget(panelObj.transform, "SoapRive");
            var towelWidget = FindOrCreateWidget(panelObj.transform, "TowelRive");
            var characterWidget = FindOrCreateWidget(panelObj.transform, "CharacterRive");
            var stepWidgets = new RiveWidget[StepIcon.WidgetNames.Length];
            for (int i = 0; i < stepWidgets.Length; i++)
                stepWidgets[i] = FindOrCreateWidget(panelObj.transform, StepIcon.WidgetNames[i]);
            var progressWidget = FindOrCreateWidget(panelObj.transform, ProgressBar.WidgetName);
            var introWidget = FindOrCreateWidget(panelObj.transform, "IntroRive");

            var oldFaucetOnBackground = backgroundWidget.GetComponent<Faucet>();
            if (oldFaucetOnBackground != null)
                Object.DestroyImmediate(oldFaucetOnBackground);

            ConfigureWidget(
                backgroundWidget,
                asset,
                Background.Artboard,
                Background.StateMachine,
                HitTestBehavior.None,
                Fit.Contain,
                RiveWidget.DataBindingMode.Manual);
            Stretch(backgroundWidget.GetComponent<RectTransform>());

            ConfigureSlot(faucetWidget, asset, Faucet.Artboard, Faucet.StateMachine, HitTestBehavior.None);
            ConfigureSlot(handsWidget, asset, Hands.Artboard, Hands.StateMachine, HitTestBehavior.None);
            ConfigureSlot(soapWidget, asset, Soap.Artboard, Soap.StateMachine, HitTestBehavior.None);
            ConfigureSlot(towelWidget, asset, Towel.Artboard, Towel.StateMachine, HitTestBehavior.None);
            ConfigureSlot(characterWidget, asset, Character.Artboard, Character.StateMachine, HitTestBehavior.None);
            for (int i = 0; i < stepWidgets.Length; i++)
                ConfigureSlot(stepWidgets[i], asset, StepIcon.Artboard, StepIcon.StateMachine, HitTestBehavior.None);

            ConfigureWidget(
                progressWidget,
                asset,
                ProgressBar.Artboard,
                ProgressBar.StateMachine,
                HitTestBehavior.None,
                Fit.Fill,
                RiveWidget.DataBindingMode.AutoBindDefault);

            ConfigureWidget(
                introWidget,
                asset,
                Intro.Artboard,
                Intro.StateMachine,
                HitTestBehavior.Opaque,
                Fit.Contain,
                RiveWidget.DataBindingMode.AutoBindDefault);
            Stretch(introWidget.GetComponent<RectTransform>());
            introWidget.gameObject.SetActive(true);

            int sibling = 0;
            backgroundWidget.transform.SetSiblingIndex(sibling++);
            handsWidget.transform.SetSiblingIndex(sibling++);
            soapWidget.transform.SetSiblingIndex(sibling++);
            towelWidget.transform.SetSiblingIndex(sibling++);
            faucetWidget.transform.SetSiblingIndex(sibling++);
            characterWidget.transform.SetSiblingIndex(sibling++);
            for (int i = 0; i < stepWidgets.Length; i++)
                stepWidgets[i].transform.SetSiblingIndex(sibling++);
            progressWidget.transform.SetSiblingIndex(sibling++);
            introWidget.transform.SetSiblingIndex(sibling++);

            var mount = panelObj.GetComponent<RiveAnchorMount>();
            if (mount == null)
                mount = panelObj.AddComponent<RiveAnchorMount>();

            var slots = new[]
            {
                new RiveAnchorMount.Slot { anchorName = BackgroundAnchors.Hands, widget = handsWidget },
                new RiveAnchorMount.Slot { anchorName = BackgroundAnchors.Soap, widget = soapWidget },
                new RiveAnchorMount.Slot { anchorName = BackgroundAnchors.Towel, widget = towelWidget },
                new RiveAnchorMount.Slot { anchorName = BackgroundAnchors.Faucet, widget = faucetWidget },
                new RiveAnchorMount.Slot { anchorName = BackgroundAnchors.Character, widget = characterWidget },
                new RiveAnchorMount.Slot { anchorName = BackgroundAnchors.Step1, widget = stepWidgets[0] },
                new RiveAnchorMount.Slot { anchorName = BackgroundAnchors.Step2, widget = stepWidgets[1] },
                new RiveAnchorMount.Slot { anchorName = BackgroundAnchors.Step3, widget = stepWidgets[2] },
                new RiveAnchorMount.Slot { anchorName = BackgroundAnchors.Step4, widget = stepWidgets[3] },
                new RiveAnchorMount.Slot { anchorName = BackgroundAnchors.ProgressBar, widget = progressWidget },
            };
            mount.Bind(backgroundWidget, slots);

            var presenter = systems.GetComponent<SimiPrototypePresenter>();
            if (presenter == null)
                presenter = systems.AddComponent<SimiPrototypePresenter>();
            var binder = systems.GetComponent<RiveHudBinder>();
            if (binder == null)
                binder = systems.AddComponent<RiveHudBinder>();
            var faucet = faucetWidget.GetComponent<Faucet>();
            if (faucet == null)
                faucet = faucetWidget.gameObject.AddComponent<Faucet>();
            faucet.Bind(faucetWidget);
            faucet.SetEnabled(false);

            var hands = handsWidget.GetComponent<Hands>();
            if (hands == null)
                hands = handsWidget.gameObject.AddComponent<Hands>();
            hands.Bind(handsWidget, faucetWidget);
            hands.SetDraggable(false);
            mount.TryApply();
            AssignHitboxes(hands, handsWidget, faucetWidget);

            var soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("flow").objectReferenceValue = systems.GetComponent<GameFlowController>();
            soPresenter.FindProperty("backgroundWidget").objectReferenceValue = backgroundWidget;
            soPresenter.FindProperty("introWidget").objectReferenceValue = introWidget;
            soPresenter.FindProperty("progressBarWidget").objectReferenceValue = progressWidget;
            soPresenter.FindProperty("faucetWidget").objectReferenceValue = faucetWidget;
            soPresenter.FindProperty("handsWidget").objectReferenceValue = handsWidget;
            soPresenter.FindProperty("hudBinder").objectReferenceValue = binder;
            soPresenter.FindProperty("faucet").objectReferenceValue = faucet;
            soPresenter.FindProperty("hands").objectReferenceValue = hands;
            soPresenter.FindProperty("anchorMount").objectReferenceValue = mount;
            AssignWidgetArray(soPresenter.FindProperty("stepIconWidgets"), stepWidgets);
            soPresenter.ApplyModifiedPropertiesWithoutUndo();

            var flow = systems.GetComponent<GameFlowController>();
            var soBinder = new SerializedObject(binder);
            soBinder.FindProperty("hud").objectReferenceValue = flow != null ? flow.hud : null;
            soBinder.FindProperty("progressBarWidget").objectReferenceValue = progressWidget;
            AssignWidgetArray(soBinder.FindProperty("stepIconWidgets"), stepWidgets);
            soBinder.ApplyModifiedPropertiesWithoutUndo();

            var soFlow = new SerializedObject(flow);
            soFlow.FindProperty("faucet").objectReferenceValue = faucet;
            soFlow.FindProperty("hands").objectReferenceValue = hands;
            soFlow.FindProperty("riveHud").objectReferenceValue = binder;
            soFlow.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Mounted simi_prototype background + anchored widgets on GameplayCanvas.");
        }

        [MenuItem("ManosLimpias/Bake Hands And Faucet Hitboxes")]
        public static void BakeHandsAndFaucetHitboxes()
        {
            var handsGo = GameObject.Find("HandsRive");
            var faucetGo = GameObject.Find("FaucetRive");
            if (handsGo == null || faucetGo == null)
            {
                Debug.LogError("[RivePrototypeMount] HandsRive or FaucetRive not found.");
                return;
            }

            var panel = handsGo.transform.parent != null
                ? handsGo.transform.parent.GetComponent<RiveAnchorMount>()
                : null;
            panel?.TryApply();

            var handsWidget = handsGo.GetComponent<RiveWidget>();
            var faucetWidget = faucetGo.GetComponent<RiveWidget>();
            var hands = handsGo.GetComponent<Hands>();
            if (hands == null)
                hands = handsGo.AddComponent<Hands>();
            hands.Bind(handsWidget, faucetWidget);
            AssignHitboxes(hands, handsWidget, faucetWidget);

            EditorSceneManager.MarkSceneDirty(handsGo.scene);
            EditorSceneManager.SaveScene(handsGo.scene);
            Debug.Log("[RivePrototypeMount] Baked Hands/Faucet layout and authored hitbox children.");
        }

        static void AssignHitboxes(Hands hands, RiveWidget handsWidget, RiveWidget faucetWidget)
        {
            var hitbox1 = RiveNodeHitbox.FindOrCreate(handsWidget, Hands.Hitbox1, Hands.Hitbox1Normalized);
            var hitbox2 = RiveNodeHitbox.FindOrCreate(handsWidget, Hands.Hitbox2, Hands.Hitbox2Normalized);
            var water = RiveNodeHitbox.FindOrCreate(faucetWidget, Hands.WaterSqspot, Hands.WaterNormalized);

            var soHands = new SerializedObject(hands);
            soHands.FindProperty("hitbox1").objectReferenceValue = hitbox1;
            soHands.FindProperty("hitbox2").objectReferenceValue = hitbox2;
            soHands.FindProperty("waterHitbox").objectReferenceValue = water;
            soHands.FindProperty("widget").objectReferenceValue = handsWidget;
            soHands.FindProperty("faucetWidget").objectReferenceValue = faucetWidget;
            soHands.ApplyModifiedPropertiesWithoutUndo();
        }

        static void LogFileInventory(Asset asset)
        {
            using var file = File.Load(asset);
            if (file == null)
            {
                Debug.LogWarning("[RivePrototypeMount] Could not load Rive file for inventory.");
                return;
            }

            Debug.Log($"[RivePrototypeMount] Artboards={file.ArtboardCount} ViewModels={file.ViewModelCount}");
            for (uint i = 0; i < file.ArtboardCount; i++)
            {
                var artboard = file.Artboard(i);
                if (artboard == null)
                    continue;
                var smNames = new System.Text.StringBuilder();
                for (uint j = 0; j < artboard.StateMachineCount; j++)
                {
                    if (j > 0) smNames.Append(", ");
                    smNames.Append(artboard.StateMachineName(j));
                }

                var vm = artboard.DefaultViewModel != null ? artboard.DefaultViewModel.Name : "none";
                Debug.Log($"[RivePrototypeMount] artboard '{file.ArtboardName(i)}' {artboard.Width}x{artboard.Height} sm=[{smNames}] defaultVM={vm}");
            }

            for (int i = 0; i < file.ViewModelCount; i++)
            {
                var vm = file.GetViewModelAtIndex(i);
                if (vm == null) continue;
                var props = new System.Text.StringBuilder();
                var properties = vm.Properties;
                for (int p = 0; p < properties.Count; p++)
                {
                    if (p > 0) props.Append(", ");
                    props.Append(properties[p].Name).Append(':').Append(properties[p].Type);
                }
                Debug.Log($"[RivePrototypeMount] viewModel '{vm.Name}' props=[{props}]");
            }
        }

        static void ConfigureSlot(
            RiveWidget widget,
            Asset asset,
            string artboard,
            string stateMachine,
            HitTestBehavior hitTest)
        {
            ConfigureWidget(
                widget,
                asset,
                artboard,
                stateMachine,
                hitTest,
                Fit.Fill,
                RiveWidget.DataBindingMode.Manual);
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

            return go.GetComponent<RiveWidget>();
        }

        static void RenameIfExists(Transform panel, string oldName, string newName)
        {
            if (oldName == newName)
                return;
            var existing = panel.Find(oldName);
            if (existing == null)
                return;
            if (panel.Find(newName) != null)
                return;
            existing.name = newName;
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
            so.FindProperty("m_artboardName").stringValue = artboard ?? string.Empty;
            so.FindProperty("m_stateMachineName").stringValue = stateMachine ?? string.Empty;
            so.FindProperty("m_hitTestBehavior").enumValueIndex = (int)hitTest;
            so.FindProperty("m_fit").enumValueIndex = (int)fit;
            so.FindProperty("m_dataBindingMode").enumValueIndex = (int)bindingMode;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AssignWidgetArray(SerializedProperty property, RiveWidget[] widgets)
        {
            if (property == null)
                return;
            property.arraySize = widgets.Length;
            for (int i = 0; i < widgets.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = widgets[i];
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
    }
}
#endif
