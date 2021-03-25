using System;
using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.NodeNotes
{
    
    public class Dialogue : ScriptableObject
    {
        public string _serializedBranches;

        [NonSerialized] public InteractionBranch interactionBranch = new InteractionBranch();

        protected int LogicVersion => Service.TryGetValue<TriggerValuesService, int>(s => s.Version, defaultValue: _questVersion);

        #region Options MGMT

        private string SingleText
        {
            set
            {
                OptText.Clear();
                //TODO: This sets a Single Text
            }
        }

        private readonly List<string> OptText = new List<string>();
        private readonly List<Interaction> PossibleInteractions = new List<Interaction>();
        private readonly List<DialogueChoice> PossibleOptions = new List<DialogueChoice>();

        private bool CheckOptions(Interaction ia)
        {

            ClearTexts();
            var cnt = 0;
            foreach (var dio in ia.choices)
                if (dio.conditions.CheckConditions())
                {
                    OptText.Add(dio.text.NameForInspector);
                    PossibleOptions.Add(dio);
                    cnt++;
                }

            _questVersion = LogicVersion;

            return cnt > 0;
        }

        private void CollectInteractions(LogicBranch<Interaction> gr)
        {

            if (!gr.IsTrue()) return;

            foreach (var si in gr.elements)
            {

                si.ResetSentences();

                if (!si.IsTrue())
                    continue;

                OptText.Add(si.texts.NameForInspector);
                PossibleInteractions.Add(si);
            }

            foreach (var sgr in gr.subBranches)
                CollectInteractions(sgr);
        }

        private void BackToInteractionSelection()
        {
            ClearTexts();

            CollectInteractions(interactionBranch);

            if (PossibleInteractions.Count != 0)
            {
                _questVersion = LogicVersion;

                _interactionStage = 0;

                if (!continuationReference.IsNullOrEmpty())
                {
                    foreach (var ie in PossibleInteractions)
                        if (ie.ReferenceName.SameAs(continuationReference))
                        {
                            _interaction = ie;
                            _interactionStage++;
                            SelectOption(0);
                            break;
                        }
                }

                var lst = new List<string>();
                
                if (_interactionStage != 0) return;
                
                foreach (var interaction in PossibleInteractions)
                    lst.Add(interaction.texts.NameForInspector);

                //View.Options = lst;

            }
            //else
            //  Exit();
        }

        private static int _interactionStage;
        private static Interaction _interaction;
        private static DialogueChoice _option;

        private static int _questVersion;

        private void DistantUpdate()
        {

            if (_questVersion == LogicVersion) return;

            switch (_interactionStage)
            {

                case 0: BackToInteractionSelection(); break;
                case 1: SingleText = _interaction.texts.NameForInspector; break;
                case 3: CheckOptions(_interaction); break;
                case 5: SingleText = _option.text2.NameForInspector; break;
            }

            _questVersion = LogicVersion;
        }

        private void ClearTexts()
        {
            OptText.Clear();
            PossibleInteractions.Clear();
            PossibleOptions.Clear();
        }

        private string continuationReference;

        public void SelectOption(int no)
        {
            switch (_interactionStage)
            {
                case 0:
                    _interactionStage++; _interaction = PossibleInteractions.TryGet(no);
                    goto case 1;
                case 1:
                    continuationReference = null;

                    if (_interaction == null)
                        SingleText = "No Possible Interactions.";
                    else
                    {
                        if (_interaction.texts.GotNextText)
                        {
                            SingleText = _interaction.texts.GetNext();
                            break;
                        }

                        _interactionStage++;


                        goto case 2;
                    }

                    break;
                case 2:
                    _interactionStage++;
                    if (!CheckOptions(_interaction)) goto case 4; break;
                case 3:
                    _option = PossibleOptions[no];
                    _option.results.Apply();
                    _interaction.finalResults.Apply();
                    continuationReference = _option.nextOne;
                    goto case 5;

                case 4:
                    _interaction.finalResults.Apply(); BackToInteractionSelection(); break;
                case 5:
                    if (_option.text2.GotNextText)
                    {
                        SingleText = _option.text2.GetNext();
                        _interactionStage = 5;
                    }
                    else
                        goto case 6;

                    break;

                case 6:
                    BackToInteractionSelection();
                    break;
            }
        }

        #endregion

        #region Encode & Decode
        public CfgEncoder Encode() => new CfgEncoder()
            .Add("inBr", interactionBranch);
        
        public void Decode(string tg, CfgData data) {
            switch (tg) {
                case "inBr": data.Decode(out interactionBranch);  break;
            }
        }
        #endregion

        #region Inspector

        public static Dialogue inspected;
        private int _inspectdStuff = -1; 
        protected bool InspectGameNode() {

            var changed = false;

            inspected = this;

            pegi.nl();
            
            if ("Play In Inspector".isEntered(ref _inspectdStuff, 0).nl()) {

                "Playing {0} Dialogue".F(name).write();

                if (icon.Refresh.Click("Restart dialogue", 20))
                    BackToInteractionSelection();
                else {
                    DistantUpdate();
                    pegi.nl();
                    for (var i = 0; i < OptText.Count; i++)
                        if (OptText[i].ClickText(13).nl()) {
                            SelectOption(i);
                            DistantUpdate();
                        }

                }
            }

            if ("Interactions tree".isEntered(ref _inspectdStuff, 1).nl_ifNotEntered())
                interactionBranch.Nested_Inspect().nl();

            "Interaction stage: {0}".F(_interactionStage).nl();
            
            return changed;
        }

        #endregion

    }
}
