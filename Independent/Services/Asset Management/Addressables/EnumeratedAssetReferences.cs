using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace QuizCanners.IsItGame
{
    public class EnumeratedAssetReferenceAbstract: ScriptableObject 
    {
        [SerializeField] internal List<EnumeratedReference> references;

        [Serializable]
        public class EnumeratedReference : AddressableBase, IPEGI_ListInspect
        {
           
            public AssetReference Reference;
          
            [NonSerialized] protected AsyncOperationHandle _handle;
            protected override AsyncOperationHandle GetHandle() => _handle;
            public Task GetAsync<T>(Action<T> onExit = null) where T : Object => GetAsync_Internal(onExit);
                
            protected override void StartLoad()
            {
                if (!DirectReference)
                {
                    _handle = GetReference().LoadAssetAsync<Object>();
                }
            }

            protected override AssetReference GetReference() => Reference;

            #region Inspector
            public static Object inspectedDataSource;
            public static Type inspectedEnum;

            public void InspectInList(ref int edited, int ind)
            {
                string name = Enum.ToObject(inspectedEnum, ind).ToString();

                if (Name.IsNullOrEmpty())
                {
                    if ("Set name".Click())
                        Name = name;
                }
                else if ((name.Equals(Name) == false) && icon.Refresh.Click("Refresh Name"))
                    Name = name;

                Name.write(90);

                if (DirectReference && Reference.AssetGUID.IsNullOrEmpty() == false && icon.Clear.Click())
                    Reference = null;
                
                if (!DirectReference || Reference.IsValid())
                {
                    Name.edit_Property(
                        () => Reference,
                        nameof(references),
                        inspectedDataSource);
                }

                pegi.edit(ref DirectReference, width: 100);
            }

            #endregion
        }
    }

    public class EnumeratedAssetReferences<T,G> : EnumeratedAssetReferenceAbstract, IPEGI where T : struct, IComparable, IFormattable, IConvertible where G : Object
    {
        public EnumeratedReference GetReference(T key) 
        {
            var reff = references.TryGet(Convert.ToInt32(key));
            return reff;
        }

        public Task GetAssync(T key, Action<G> onExit) 
        {
            var reff = references.TryGet(Convert.ToInt32(key));

            if (reff!= null) 
            {
                return reff.GetAsync(onExit: onExit);
            } else 
            {
                try
                {
                    onExit?.Invoke(null);
                } catch(Exception ex) 
                {
                    Debug.LogException(ex);
                }
                return Task.CompletedTask;
            }
        }

        #region Inspector

        private int _inspectedReference = -1;
        public void Inspect()
        {
            EnumeratedReference.inspectedEnum = typeof(T);
            EnumeratedReference.inspectedDataSource = this;

            (typeof(T).ToPegiStringType() + "s").edit_List(references, ref _inspectedReference);
        }

        #endregion
    }

}
