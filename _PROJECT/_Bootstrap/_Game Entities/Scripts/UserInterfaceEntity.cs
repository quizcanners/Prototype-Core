using QuizCanners.Inspect;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class UserInterfaceEntity : IsItGameClassBase, IPEGI
    {
        public IigEnum_UiView SelectedView;
       

        public InputFieldState InputFieldData = new InputFieldState();

        public void Inspect() 
        {
            "Selected View".editEnum(ref SelectedView).nl();
        }

        public class InputFieldState : IPEGI
        {
            public string Header;
            public string InputText;
            public bool Approved;
            public Action OnCloseAction;
            public Action<string> OnApprove;
            private Func<string, bool> Validator;

            public bool IsValid() => Validator == null || Validator(InputText);

            public void Approve()
            {
                try
                {
                    OnApprove?.Invoke(InputText);
                } catch (Exception ex) 
                {
                    Debug.LogException(ex);
                }

                Close();
            }
            public void Close() 
            {
                OnCloseAction?.Invoke();
            }
            public void Set(string header, Action<string> onValidate, Action onClose = null, Func<string, bool> validator = null) 
            {
                Header = header;
                InputText = "";
                Approved = false;
                OnCloseAction = onClose;
                OnApprove = onValidate;
                Validator = validator;
            }

            #region Inspector
            public void Inspect()
            {
                if (OnCloseAction != null && "Close".Click().nl())
                    Close();

                Header.edit(ref InputText);

                if (IsValid() && icon.Done.Click())
                    Approve();
            }
            #endregion
        }
    }
}