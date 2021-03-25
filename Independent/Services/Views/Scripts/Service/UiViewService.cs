using QuizCanners.Inspect;
using QuizCanners.IsItGame.SpecialEffects;
using QuizCanners.Lerp;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace QuizCanners.IsItGame.UI
{
    public class UiViewService : IsItGameServiceBase, Service.ILoadingProgressForInspector
    {
        [SerializeField] private GameObject _loadingScreen;
        [SerializeField] private Image _blurOverlayTransition;
        [SerializeField] private Image _blurHexagonalTransition;
        [SerializeField] private ScreenBlurController _screenBlur;

        [SerializeField] private UiViewEnumeratedScriptableObject _views;
        [SerializeField] private RectTransform _root;

        private GameObject _currentViewInstance;
        private bool _currentViewIsAddressable;
        private IigEnum_UiView _targetView = IigEnum_UiView.None;
        private IigEnum_UiView _currentView = IigEnum_UiView.None;
        private ScreenChangeState _screenChangeState = ScreenChangeState.Standby;
        private UiTransitionType _currentTransitionType;
        private UiTransitionType _targetTransitionType;

        private readonly List<IigEnum_UiView> _viewsStack = new List<IigEnum_UiView>();

        private Image TransitionGraphic => _currentTransitionType switch
                {
                    UiTransitionType.Hexagonal => _blurHexagonalTransition,
                    UiTransitionType.CrossFade => _blurOverlayTransition,
                    _ => _blurOverlayTransition,
                };
    
        public void Show(IigEnum_UiView view, bool clearStack, UiTransitionType transition = UiTransitionType.CrossFade) 
        {
            if (clearStack)
                _viewsStack.Clear();
            else
            {
                var ind = _viewsStack.IndexOf(view);
                if (ind != -1)
                    _viewsStack.RemoveRange(ind, _viewsStack.Count - ind);
                else if (_targetView != IigEnum_UiView.None)
                    _viewsStack.Add(_targetView);
            }

            _targetView = view;
            _targetTransitionType = transition;
        }

        public void Hide(IigEnum_UiView view, UiTransitionType transition = UiTransitionType.CrossFade)
        {
            if (_targetView == view) 
            {
                _targetTransitionType = transition;
                _targetView = _viewsStack.TryTake(_viewsStack.Count-1, defaultValue: IigEnum_UiView.None); 
            } else 
            {
                if (_viewsStack.Contains(view))
                    _viewsStack.Remove(view);
                
            }
        }

        public void ShowError(string text) 
        {
            _targetView = IigEnum_UiView.ErrorSorry;
            _screenChangeState = ScreenChangeState.ReadyToChangeView;
            Debug.LogError(text);
        }

        public bool IsLoading(ref string state, ref float progress01)
        {

            if (_screenChangeState == ScreenChangeState.LoadingNextView) 
            {
                state = "LoadingNextView";
                progress01 = handle.PercentComplete;
                return true;
            }

            return false;
        }

        AsyncOperationHandle<GameObject> handle;

        private void LateUpdate()
        {
            switch (_screenChangeState)
            {
                case ScreenChangeState.Standby:

                    IigEnum_UiView newView = _targetView;
                    if (TryEnterIfStateChanged() && GameStateMachine.TryChange(ref newView))
                    {
                        // State defines the Root view, while other views can be added to stack, defaulting back to Root View.
                        if (newView != IigEnum_UiView.None && !_viewsStack.Contains(newView))
                        {
                            Show(newView, clearStack: true, transition: UiTransitionType.Blur);
                        }
                    }
                    
                    if (_targetView != _currentView) 
                    {
                        _screenChangeState = ScreenChangeState.RequestedScreenShot;
                        _currentTransitionType = _targetTransitionType;

                        ScreenBlurController.ProcessCommand processCommand = ScreenBlurController.ProcessCommand.Nothing;

                        processCommand = _currentTransitionType switch
                        {
                            UiTransitionType.WipeAway =>    ScreenBlurController.ProcessCommand.WashAway,
                            _ =>                            ScreenBlurController.ProcessCommand.Blur,
                        };

                        _screenBlur.RequestUpdate(
                            onFirstRendered: () => 
                                {
                                    TransitionGraphic.TrySetAlpha(1);
                                    _screenChangeState = ScreenChangeState.ReadyToChangeView;
                                }, 
                            afterScreenGrab: processCommand);
                    }
                    break;
                case ScreenChangeState.RequestedScreenShot: break;
                case ScreenChangeState.ReadyToChangeView:

                    _screenChangeState = ScreenChangeState.LoadingNextView;
                    _currentView = _targetView;

                    DestroyInstance();

                    if (_currentView == IigEnum_UiView.None) 
                    {
                        _screenChangeState = ScreenChangeState.ViewIsSetUp;
                        break;
                    }

                    var reff = _views.GetReference(_currentView);

                    if (reff == null) 
                    {
                        ShowError("Reference {0} not found".F(_currentView));
                        return;
                    }

                    if (reff.Reference.AssetGUID.IsNullOrEmpty() == false) 
                    {
                        handle = reff.Reference.InstantiateAsync(_root);
                        handle.Completed += result =>
                        {
                            if (result.Status == AsyncOperationStatus.Succeeded)
                            {
                                _currentViewInstance = result.Result;
                                _currentViewIsAddressable = true;
                                _screenChangeState = ScreenChangeState.ViewIsSetUp;
                            } else
                            {
                                ShowError("Couldn't load the view");
                            }
                        };
                    } else 
                    {
                        if (!reff.DirectReference) 
                        {
                            ShowError("No References for {0} found".F(_currentView));
                            return;
                        }

                        _currentViewInstance = Instantiate(reff.DirectReference, _root) as GameObject;
                        _currentViewIsAddressable = false;
                        _screenChangeState = ScreenChangeState.ViewIsSetUp;
                    }

                    break;

                case ScreenChangeState.LoadingNextView: break;
                case ScreenChangeState.ViewIsSetUp:

                    if (_loadingScreen)
                        _loadingScreen.SetActive(false);

                    if (!TransitionGraphic.IsLerpingAlphaBySpeed(0, 2.5f)) 
                        _screenChangeState = ScreenChangeState.Standby;
                    break;
            }
        }

        #region Inspector

        private IigEnum_UiView _debugType = IigEnum_UiView.None;
        public override void Inspect()
        {
            base.Inspect();

            "Transition Type".editEnum(ref _targetTransitionType).nl();

            "Target view".editEnum(ref _debugType);

            if (icon.Add.Click())
                Show(_debugType, clearStack: false, _targetTransitionType);

            if (icon.Play.Click())
                Show(_debugType, clearStack: true, _targetTransitionType);

            pegi.nl();

            if (_targetView!= IigEnum_UiView.None) 
            {
                if (icon.Clear.Click())
                    Hide(_targetView);
                _targetView.ToString().write();
                pegi.nl();
            }

            for (int i=_viewsStack.Count-1; i>=0; i--) 
            {
                var v = _viewsStack[i];
;               if (icon.Close.Click())
                   Hide(v);

                v.ToString().write();

                pegi.nl();
            }

            if (_views)
            {
                "Enumerated Views ".write(); _views.ClickHighlight();
                pegi.nl();
            }

            "Screen Change State: {0}".F(_screenChangeState).nl();

            "Current View".editEnum(ref _currentView).nl();

            if (_currentViewInstance && "Destroy {0}".F(_currentViewInstance.name).Click().nl())
                Addressables.Release(_currentViewInstance);
        }

    

        public void InspectCurrentView() 
        {
            if (!_currentViewInstance)
                "No views".nl();
            else
            {
                _currentViewInstance.name.nl(PEGI_Styles.ListLabel);
                pegi.Try_Nested_Inspect(_currentViewInstance.GetComponent<IPEGI>());
            }
        }

        #endregion

        protected void Awake()
        {
            if (_loadingScreen)
                _loadingScreen.SetActive(true);

            _blurOverlayTransition.TrySetAlpha(0);
            _blurHexagonalTransition.TrySetAlpha(0);
        }

        private void DestroyInstance() 
        {
            if (_currentViewInstance)
            {
                if (_currentViewIsAddressable)
                    Addressables.Release(_currentViewInstance);
                else
                    _currentViewInstance.DestroyWhatever();

                _currentViewInstance = null;
            }
        }

        protected void OnDisable()
        {
            DestroyInstance();

            _targetView = IigEnum_UiView.None;
            _currentView = IigEnum_UiView.None;
            _screenChangeState = ScreenChangeState.Standby;
        }

        protected enum ScreenChangeState { Standby, RequestedScreenShot, ReadyToChangeView, LoadingNextView, ViewIsSetUp }
    }

    [PEGI_Inspector_Override(typeof(UiViewService))] internal class UiViewServiceDrawer : PEGI_Inspector_Override { }

    public enum UiTransitionType { Blur, CrossFade, Hexagonal, WipeAway }

    public static class UiViewsExtensions 
    {
        public static void Show(this IigEnum_UiView view, bool clearStack, UiTransitionType transition = UiTransitionType.CrossFade)
            => Service.Try<UiViewService>(s=> s.Show(view, clearStack, transition));

        public static void Hide(this IigEnum_UiView view, UiTransitionType transition = UiTransitionType.CrossFade)
            => Service.Try<UiViewService>(s => s.Hide(view, transition));
    }
}
