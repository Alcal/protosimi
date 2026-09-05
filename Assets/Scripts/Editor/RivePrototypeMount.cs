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

            var canvasTf = canvas.transform;
            var glowTemplate = AssetDatabase.LoadAssetAtPath<Material>(RiveGlow.TemplateAssetPath);

            var panelObj = FindOrCreatePanel(canvasTf, "Rive Panel", stretch: true);
            var hudPanel = FindOrCreatePanel(canvasTf, RiveGlow.HudPanelName, stretch: true);
            var introPanel = FindOrCreatePanel(canvasTf, RiveGlow.PanelNameFor(Intro.WidgetName), stretch: true);

            RenameIfExists(panelObj.transform, "MainRive", Background.WidgetName);
            RenameIfExists(panelObj.transform, "StepIconRive", StepIcon.WidgetNames[0]);

            var backgroundWidget = FindOrRelocateWidget(canvasTf, panelObj.transform, Background.WidgetName);
            var handsWidget = FindOrRelocateWidget(canvasTf, IsolatedPanel(canvasTf, Hands.WidgetName), Hands.WidgetName);
            var soapWidget = FindOrRelocateWidget(canvasTf, IsolatedPanel(canvasTf, Soap.WidgetName), Soap.WidgetName);
            var towelWidget = FindOrRelocateWidget(canvasTf, IsolatedPanel(canvasTf, Towel.WidgetName), Towel.WidgetName);
            var faucetWidget = FindOrRelocateWidget(canvasTf, IsolatedPanel(canvasTf, Faucet.WidgetName), Faucet.WidgetName);
            var characterWidget = FindOrRelocateWidget(canvasTf, IsolatedPanel(canvasTf, Character.WidgetName), Character.WidgetName);
            var stepWidgets = new RiveWidget[StepIcon.WidgetNames.Length];
            for (int i = 0; i < stepWidgets.Length; i++)
                stepWidgets[i] = FindOrRelocateWidget(canvasTf, hudPanel.transform, StepIcon.WidgetNames[i]);
            var progressWidget = FindOrRelocateWidget(canvasTf, hudPanel.transform, ProgressBar.WidgetName);
            var introWidget = FindOrRelocateWidget(canvasTf, introPanel.transform, Intro.WidgetName);
            DestroyIfExists(canvasTf, SoapBubbles.OverlayName);
            var bubbleSprite = RiveBubbleSpriteExtract.LoadOrExtract();
            var bubbleMat = RiveBubbleSpriteExtract.EnsureMaterial(bubbleSprite);
            EnsureCanvasCamera(canvas.GetComponent<Canvas>());

            ConfigureGlow(handsWidget.transform.parent.gameObject, handsWidget, glowTemplate);
            ConfigureGlow(soapWidget.transform.parent.gameObject, soapWidget, glowTemplate);
            ConfigureGlow(towelWidget.transform.parent.gameObject, towelWidget, glowTemplate);
            ConfigureGlow(faucetWidget.transform.parent.gameObject, faucetWidget, glowTemplate);
            ConfigureGlow(characterWidget.transform.parent.gameObject, characterWidget, glowTemplate);

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
            ArtboardSpace.StretchFill(backgroundWidget.GetComponent<RectTransform>());

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
            ArtboardSpace.StretchFill(introWidget.GetComponent<RectTransform>());
            introPanel.SetActive(true);
            introWidget.gameObject.SetActive(true);

            panelObj.transform.SetAsLastSibling();
            handsWidget.transform.parent.SetAsLastSibling();
            soapWidget.transform.parent.SetAsLastSibling();
            towelWidget.transform.parent.SetAsLastSibling();
            faucetWidget.transform.parent.SetAsLastSibling();
            characterWidget.transform.parent.SetAsLastSibling();
            hudPanel.transform.SetAsLastSibling();
            introPanel.transform.SetAsLastSibling();

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

            var soap = soapWidget.GetComponent<Soap>();
            if (soap == null)
                soap = soapWidget.gameObject.AddComponent<Soap>();
            soap.Bind(soapWidget, hands);
            soap.SetDraggable(false);

            var soapBubbles = handsWidget.GetComponent<SoapBubbles>();
            if (soapBubbles == null)
                soapBubbles = handsWidget.gameObject.AddComponent<SoapBubbles>();

            Canvas.ForceUpdateCanvases();
            mount.TryApply();
            AssignHitboxes(hands, handsWidget, faucetWidget);
            AssignSoapHitbox(soap, soapWidget, hands);
            AssignSoapBubbles(soapBubbles, hands, soap, bubbleSprite, bubbleMat);

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
            soFlow.FindProperty("soap").objectReferenceValue = soap;
            soFlow.FindProperty("soapBubbles").objectReferenceValue = soapBubbles;
            soFlow.FindProperty("riveHud").objectReferenceValue = binder;
            EnsureStageConfigurations(soFlow);
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

            var rootPanel = GameObject.Find("Rive Panel");
            var panel = rootPanel != null ? rootPanel.GetComponent<RiveAnchorMount>() : null;
            panel?.TryApply();

            var handsWidget = handsGo.GetComponent<RiveWidget>();
            var faucetWidget = faucetGo.GetComponent<RiveWidget>();
            var hands = handsGo.GetComponent<Hands>();
            if (hands == null)
                hands = handsGo.AddComponent<Hands>();
            hands.Bind(handsWidget, faucetWidget);
            AssignHitboxes(hands, handsWidget, faucetWidget);

            var soapGo = GameObject.Find("SoapRive");
            if (soapGo != null)
            {
                var soapWidget = soapGo.GetComponent<RiveWidget>();
                var soap = soapGo.GetComponent<Soap>();
                if (soap == null)
                    soap = soapGo.AddComponent<Soap>();
                soap.Bind(soapWidget, hands);
                AssignSoapHitbox(soap, soapWidget, hands);

                var soapBubbles = handsGo.GetComponent<SoapBubbles>();
                if (soapBubbles == null)
                    soapBubbles = handsGo.AddComponent<SoapBubbles>();
                var canvas = handsGo.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    DestroyIfExists(canvas.transform, SoapBubbles.OverlayName);
                    EnsureCanvasCamera(canvas);
                }
                var bubbleSprite = RiveBubbleSpriteExtract.LoadOrExtract();
                var bubbleMat = RiveBubbleSpriteExtract.EnsureMaterial(bubbleSprite);
                AssignSoapBubbles(soapBubbles, hands, soap, bubbleSprite, bubbleMat);
                var flow = GameObject.Find("Systems")?.GetComponent<GameFlowController>();
                if (flow != null)
                {
                    var soFlow = new SerializedObject(flow);
                    soFlow.FindProperty("soapBubbles").objectReferenceValue = soapBubbles;
                    soFlow.ApplyModifiedPropertiesWithoutUndo();
                }
            }

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

        static void AssignSoapHitbox(Soap soap, RiveWidget soapWidget, Hands hands)
        {
            var hitbox = RiveNodeHitbox.FindOrCreate(soapWidget, Soap.Hitbox, Soap.HitboxNormalized);
            var soSoap = new SerializedObject(soap);
            soSoap.FindProperty("hitbox").objectReferenceValue = hitbox;
            soSoap.FindProperty("widget").objectReferenceValue = soapWidget;
            soSoap.FindProperty("hands").objectReferenceValue = hands;
            soSoap.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AssignSoapBubbles(
            SoapBubbles foam,
            Hands hands,
            Soap soap,
            Sprite bubbleSprite,
            Material particleMaterial)
        {
            if (foam == null)
                return;
            foam.bubbleSprite = bubbleSprite;
            foam.particleMaterial = particleMaterial;
            foam.Bind(hands, soap);
            var soFoam = new SerializedObject(foam);
            soFoam.FindProperty("hands").objectReferenceValue = hands;
            soFoam.FindProperty("soap").objectReferenceValue = soap;
            soFoam.FindProperty("bubbleSprite").objectReferenceValue = bubbleSprite;
            soFoam.FindProperty("particleMaterial").objectReferenceValue = particleMaterial;
            soFoam.FindProperty("scrubParticles").objectReferenceValue = foam.scrubParticles;
            soFoam.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureStageConfigurations(SerializedObject soFlow)
        {
            var stages = soFlow.FindProperty("stageConfigurations");
            if (stages == null || !stages.isArray)
                return;

            bool hasOpen = false;
            bool hasSoap = false;
            for (int i = 0; i < stages.arraySize; i++)
            {
                var value = stages.GetArrayElementAtIndex(i).managedReferenceValue;
                if (value is OpenFaucetStage)
                    hasOpen = true;
                else if (value is ApplySoapStage)
                    hasSoap = true;
            }

            if (!hasOpen)
            {
                stages.arraySize++;
                stages.GetArrayElementAtIndex(stages.arraySize - 1).managedReferenceValue =
                    new OpenFaucetStage { stepId = 1 };
            }

            if (!hasSoap)
            {
                stages.arraySize++;
                stages.GetArrayElementAtIndex(stages.arraySize - 1).managedReferenceValue =
                    new ApplySoapStage { stepId = 2 };
            }
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

        static Transform IsolatedPanel(Transform canvas, string widgetName)
        {
            return FindOrCreatePanel(canvas, RiveGlow.PanelNameFor(widgetName), stretch: false).transform;
        }

        static void DestroyIfExists(Transform canvas, string name)
        {
            var existing = FindDeep(canvas, name);
            if (existing != null)
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
        }

        static void EnsureCanvasCamera(Canvas canvas)
        {
            if (canvas == null)
                return;
            var cam = Camera.main;
            if (cam == null)
                return;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 5f;
        }

        static GameObject FindOrCreatePanel(Transform canvas, string panelName, bool stretch)
        {
            var existing = FindDeep(canvas, panelName);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
                if (go.GetComponent<RectTransform>() == null)
                    go.AddComponent<RectTransform>();
                if (go.GetComponent<RivePanel>() == null)
                    go.AddComponent<RivePanel>();
                if (go.GetComponent<RiveCanvasRenderer>() == null)
                    go.AddComponent<RiveCanvasRenderer>();
                if (go.transform.parent != canvas)
                    go.transform.SetParent(canvas, false);
            }
            else
            {
                go = new GameObject(panelName, typeof(RectTransform), typeof(RivePanel), typeof(RiveCanvasRenderer));
                go.transform.SetParent(canvas, false);
            }

            WireCanvasRenderer(go);
            if (stretch)
                ArtboardSpace.StretchFill(go.GetComponent<RectTransform>());
            return go;
        }

        static void WireCanvasRenderer(GameObject panelGo)
        {
            var renderer = panelGo.GetComponent<RiveCanvasRenderer>();
            var so = new SerializedObject(renderer);
            var initialPanel = so.FindProperty("m_initialRivePanel");
            if (initialPanel != null)
                initialPanel.objectReferenceValue = panelGo.GetComponent<RivePanel>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void ConfigureGlow(GameObject panelGo, RiveWidget widget, Material template)
        {
            if (panelGo == null || widget == null)
                return;
            var glow = panelGo.GetComponent<RiveGlow>();
            if (glow == null)
                glow = panelGo.AddComponent<RiveGlow>();
            var so = new SerializedObject(glow);
            so.FindProperty("widget").objectReferenceValue = widget;
            so.FindProperty("template").objectReferenceValue = template;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static RiveWidget FindOrRelocateWidget(Transform canvas, Transform panel, string name)
        {
            var existing = FindDeep(canvas, name);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
                if (go.GetComponent<RiveWidget>() == null)
                    go.AddComponent<RiveWidget>();
                if (existing.parent != panel)
                    existing.SetParent(panel, false);
            }
            else
            {
                go = new GameObject(name, typeof(RectTransform), typeof(RiveWidget));
                go.transform.SetParent(panel, false);
            }

            ArtboardSpace.StretchFill(go.GetComponent<RectTransform>());
            return go.GetComponent<RiveWidget>();
        }

        static Transform FindDeep(Transform root, string name)
        {
            if (root == null)
                return null;
            if (root.name == name)
                return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindDeep(root.GetChild(i), name);
                if (found != null)
                    return found;
            }
            return null;
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

    }
}
#endif
