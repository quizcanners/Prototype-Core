using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = "Quiz Canners/Dungeons & Dragons/" + FILE_NAME)]
    public class DnDPrototypesScriptableObject : ScriptableObject, IPEGI
    {
        public const string FILE_NAME = "Prototypes of Dungeons & Dragons";

        public CharactersDictionary Characters = new CharactersDictionary();
        public MonstersDictionary Monsters = new MonstersDictionary();
        public PartiesDictionary Parties = new PartiesDictionary();
        public RandomElementsRollTablesDictionary RollTables = new RandomElementsRollTablesDictionary();

        #region Inspector

        private readonly CollectionMetaData _characterListMeta =   new CollectionMetaData("Characters",     showCopyPasteOptions: true);
        private readonly CollectionMetaData _monsterListMeta =     new CollectionMetaData("Monsters",       showCopyPasteOptions: true);
        private readonly CollectionMetaData _partiesListMeta =     new CollectionMetaData("Parties",        showCopyPasteOptions: true);
        private readonly CollectionMetaData _rollTablesListMeta =  new CollectionMetaData("Roll Tables",    showCopyPasteOptions: true);

        private int _inspectedCategory = -1;

        public void Inspect()
        {
            if (!Service.Get <DnD_Service>()) 
            {
                "Instance is Null. Have {0} in the scene to initialize internal singleton".F(nameof(DnD_Service)).writeWarning();
                return;
            }

            pegi.nl();

            int categoryIndex = -1;

            _characterListMeta.enter_Dictionary(Characters, ref _inspectedCategory, ++categoryIndex).nl();
            _monsterListMeta.enter_Dictionary(Monsters, ref _inspectedCategory, ++categoryIndex).nl();
            _partiesListMeta.enter_Dictionary(Parties, ref _inspectedCategory, ++categoryIndex).nl();
            _rollTablesListMeta.enter_Dictionary(RollTables, ref _inspectedCategory, ++categoryIndex).nl();

        }
        #endregion
    }



[PEGI_Inspector_Override(typeof(DnDPrototypesScriptableObject))] 
  internal class DungeonsAndDragonsCharactersScriptableObjectDrawer : PEGI_Inspector_Override { }


}