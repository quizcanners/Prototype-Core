using QuizCanners.Inspect;
using UnityEngine;

namespace QuizCanners.IsItGame.UI
{
    public class ViewChangingButton : MonoBehaviour, IPEGI
    {
        [SerializeField] private IigEnum_UiView _targetView;
        [SerializeField] private UiTransitionType _transition;
        [SerializeField] private bool _clearStack;

        public void ChangeView() => _targetView.Show(clearStack: _clearStack, _transition);

        public void Inspect()
        {
            pegi.nl();
            "View".editEnum(60, ref _targetView).nl();
            "Transition".editEnum(80, ref _transition).nl();
            "Clear Stack".toggleIcon(ref _clearStack).nl();
        }
    }

    [PEGI_Inspector_Override(typeof(ViewChangingButton))] internal class ViewChangingButtonDrawer : PEGI_Inspector_Override { }
}
