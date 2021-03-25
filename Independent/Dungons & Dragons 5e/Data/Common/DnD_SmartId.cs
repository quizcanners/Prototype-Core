using QuizCanners.Inspect;
using QuizCanners.Utils;
namespace Dungeons_and_Dragons
{
    public abstract class DnD_SmartId<TValue> : SmartStringIdGeneric<TValue> where TValue: IGotName, new()
    {
        protected DnDPrototypesScriptableObject Data
             => Service.TryGetValue<DnD_Service, DnDPrototypesScriptableObject>(s => s.DnDPrototypes);

    }
}
