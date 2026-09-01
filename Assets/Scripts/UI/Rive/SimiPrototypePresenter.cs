using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using ManosLimpias.Core;
using ManosLimpias.UI;
using Rive;
using Rive.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Mounts simi_prototype <c>main</c> + <c>intro</c> and dismisses intro when Jugar fires.
    /// </summary>
    public class SimiPrototypePresenter : MonoBehaviour
    {
        public GameFlowController flow;
        public RiveWidget mainWidget;
        public RiveWidget introWidget;
        public RiveWidget stepIconWidget;
        public RiveHudBinder hudBinder;
        public Faucet faucet;

        static readonly string[] TriggerPaths =
        {
            BubbleButtonViewModel.ButtonTrig,
            BubbleButtonViewModel.Name + "/" + BubbleButtonViewModel.ButtonTrig,
            SimiPrototypeArtboards.Button + "/" + BubbleButtonViewModel.ButtonTrig,
        };

        static readonly string[] BoolPaths =
        {
            BubbleButtonViewModel.ButtonBool,
            BubbleButtonViewModel.Name + "/" + BubbleButtonViewModel.ButtonBool,
            SimiPrototypeArtboards.Button + "/" + BubbleButtonViewModel.ButtonBool,
        };

        bool _dismissed;
        bool _introBound;
        ViewModelInstanceTriggerProperty _buttonTrig;
        ViewModelInstanceBooleanProperty _buttonBool;
        int _agentFrame;

        void OnEnable()
        {
            if (introWidget != null)
            {
                introWidget.OnWidgetStatusChanged += OnIntroStatusChanged;
                introWidget.OnRiveEventReported += OnIntroEventReported;
                if (introWidget.Status == WidgetStatus.Loaded)
                    BindIntro();
            }

            if (mainWidget != null)
            {
                mainWidget.OnWidgetStatusChanged += OnMainStatusChanged;
                if (mainWidget.Status == WidgetStatus.Loaded)
                    BindGameplay();
            }

            if (stepIconWidget != null)
            {
                stepIconWidget.OnWidgetStatusChanged += OnStepIconStatusChanged;
                if (stepIconWidget.Status == WidgetStatus.Loaded)
                    BindGameplay();
            }

            // #region agent log
            AgentSnapshot("OnEnable", "A");
            // #endregion
        }

        void LateUpdate()
        {
            // #region agent log
            _agentFrame++;
            if (_agentFrame == 30)
                AgentSnapshot("frame30", "C");
            // #endregion
        }

        void OnDisable()
        {
            UnbindIntro();
            if (introWidget != null)
            {
                introWidget.OnWidgetStatusChanged -= OnIntroStatusChanged;
                introWidget.OnRiveEventReported -= OnIntroEventReported;
            }

            if (mainWidget != null)
                mainWidget.OnWidgetStatusChanged -= OnMainStatusChanged;
            if (stepIconWidget != null)
                stepIconWidget.OnWidgetStatusChanged -= OnStepIconStatusChanged;
        }

        void OnIntroStatusChanged()
        {
            // #region agent log
            AgentSnapshot("introStatus", "B");
            // #endregion
            if (introWidget != null && introWidget.Status == WidgetStatus.Loaded)
                BindIntro();
        }

        void OnMainStatusChanged()
        {
            // #region agent log
            AgentSnapshot("mainStatus", "B");
            // #endregion
            if (mainWidget != null && mainWidget.Status == WidgetStatus.Loaded)
                BindGameplay();
        }

        void OnStepIconStatusChanged()
        {
            // #region agent log
            AgentSnapshot("stepStatus", "B");
            // #endregion
            if (stepIconWidget != null && stepIconWidget.Status == WidgetStatus.Loaded)
                BindGameplay();
        }

        void BindGameplay()
        {
            faucet?.Bind(mainWidget);
            hudBinder?.Bind(mainWidget, stepIconWidget);
        }

        void BindIntro()
        {
            if (_introBound || introWidget == null) return;
            var instance = introWidget.StateMachine?.ViewModelInstance;
            if (instance == null) return;

            _introBound = true;
            foreach (var path in TriggerPaths)
            {
                _buttonTrig = RiveStateMachineInputs.GetViewModelProperty<ViewModelInstanceTriggerProperty>(instance, path);
                if (_buttonTrig != null)
                {
                    _buttonTrig.OnTriggered += DismissIntro;
                    break;
                }
            }

            foreach (var path in BoolPaths)
            {
                _buttonBool = RiveStateMachineInputs.GetViewModelProperty<ViewModelInstanceBooleanProperty>(instance, path);
                if (_buttonBool != null)
                {
                    _buttonBool.OnValueChanged += OnButtonBoolChanged;
                    break;
                }
            }
        }

        void UnbindIntro()
        {
            if (_buttonTrig != null)
            {
                _buttonTrig.OnTriggered -= DismissIntro;
                _buttonTrig = null;
            }

            if (_buttonBool != null)
            {
                _buttonBool.OnValueChanged -= OnButtonBoolChanged;
                _buttonBool = null;
            }

            _introBound = false;
        }

        void OnIntroEventReported(ReportedEvent report)
        {
            if (report != null && report.Name == ButtonArtboard.EventButtonPress)
                DismissIntro();
        }

        void OnButtonBoolChanged(bool value)
        {
            if (value)
                DismissIntro();
        }

        public void DismissIntro()
        {
            if (_dismissed) return;
            _dismissed = true;
            UnbindIntro();
            if (introWidget != null)
                introWidget.gameObject.SetActive(false);
            flow?.DismissIntro();
        }

        // #region agent log
        void AgentSnapshot(string when, string hypothesisId)
        {
            var panel = mainWidget != null ? mainWidget.RivePanel : introWidget != null ? introWidget.RivePanel : null;
            var rt = panel != null ? panel.RenderTexture : null;
            var shader = Shader.Find("Rive/UI/Default");
            var cam = Camera.main;
            var canvasRenderer = panel != null ? panel.GetComponent<RiveCanvasRenderer>() : null;
            var mat = canvasRenderer != null ? canvasRenderer.CustomMaterial : null;
            var rect = panel != null ? panel.GetComponent<RectTransform>() : null;
            var data = "{\"when\":\"" + J(when) +
                       "\",\"native\":\"" + J(NativeStatus()) +
                       "\",\"gfx\":\"" + J(SystemInfo.graphicsDeviceType + "/" + SystemInfo.graphicsDeviceName) +
                       "\",\"colorSpace\":\"" + QualitySettings.activeColorSpace +
                       "\",\"shader\":" + (shader != null ? "true" : "false") +
                       ",\"intro\":\"" + J(WidgetInfo(introWidget)) +
                       "\",\"main\":\"" + J(WidgetInfo(mainWidget)) +
                       "\",\"step\":\"" + J(WidgetInfo(stepIconWidget)) +
                       "\",\"rt\":\"" + (rt == null ? "none" : rt.width + "x" + rt.height + " fmt=" + rt.graphicsFormat + " created=" + rt.IsCreated()) +
                       "\",\"mat\":\"" + J(mat != null ? mat.shader != null ? mat.shader.name : "noshader" : "nomaterial") +
                       "\",\"screen\":\"" + Screen.width + "x" + Screen.height +
                       "\",\"panelRect\":\"" + (rect == null ? "none" : rect.rect.width + "x" + rect.rect.height) +
                       "\",\"camHdr\":" + (cam != null && cam.allowHDR ? "true" : "false") +
                       ",\"platform\":\"" + J(Application.platform.ToString()) + "\"}";
            AgentLog(hypothesisId, "SimiPrototypePresenter.cs:" + when, "rive-webgl-snapshot", data);
        }

        static string WidgetInfo(RiveWidget w)
        {
            if (w == null) return "null";
            return w.Status + " sm=" + (w.StateMachine != null) + " ab=" + (w.Artboard != null) + " go=" + w.gameObject.activeInHierarchy;
        }

        static string NativeStatus()
        {
            try
            {
                var t = Type.GetType("Rive.NativeLibrary, Rive.Runtime");
                var m = t?.GetMethod("GetRendererStatus", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                return m == null ? "no-method" : Convert.ToString(m.Invoke(null, null));
            }
            catch (Exception e)
            {
                return "ex:" + e.GetType().Name;
            }
        }

        static string J(string s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");

        void AgentLog(string hypothesisId, string location, string message, string dataJson)
        {
            var payload = "{\"sessionId\":\"d09a31\",\"runId\":\"pre-fix\",\"hypothesisId\":\"" + hypothesisId +
                          "\",\"location\":\"" + J(location) + "\",\"message\":\"" + J(message) +
                          "\",\"data\":" + dataJson + ",\"timestamp\":" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}";
            Debug.Log("[DBG-d09a31] " + payload);
            try { System.IO.File.AppendAllText("/home/alex/Projects/Games/protosimi/.cursor/debug-d09a31.log", payload + "\n"); }
            catch { }
            StartCoroutine(AgentPost(payload));
        }

        IEnumerator AgentPost(string payload)
        {
            var req = new UnityWebRequest("http://127.0.0.1:7684/ingest/5dac28ca-0ce1-4bc4-a707-c13dfbef4f95", "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Debug-Session-Id", "d09a31");
            yield return req.SendWebRequest();
        }
        // #endregion
    }
}
