using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.NodeNotes
{

    public enum ResultType
    {
        Set, Add, Subtract //, SetTimeReal, SetTimeGame//, SetTagBool, SetTagInt 
        
    }
    
    public static class ResultExtensionFunctions {

      

        public static string GetText(this ResultType type) {
            return type switch
            {
              //  ResultType.SetBool => "Set",
                ResultType.Set => "=",
                ResultType.Add => "+",
                ResultType.Subtract => "-",
              //  ResultType.SetTimeReal => "Set To Real_Time_Now",
               // ResultType.SetTimeGame => "Set To Game_Now_Time",
                //case ResultType.SetTagBool:     return "Set Bool Tag";
                //case ResultType.SetTagInt:      return "Set Int Tag";
                _ => type.ToString(),
            };
        }
        
        public static void Apply(this List<Result> results) {
            
            if (results.Count <= 0) return;
            
            foreach (var r in results)
                r.Apply();
        }
    }
    
    public class Result : TriggerIndexBase, IIntTriggerIndex
    {
        
        public ResultType type;
        public int updateValue;

        #region Encode & Decode
        public override void Decode(string tg, CfgData data) {
            switch (tg) {
                case "ty": type = (ResultType)data.ToInt(); break;
                case "val": updateValue = data.ToInt(); break;
                case "ind": data.ToDelegate(DecodeIndex); break;
            }
        }
        
        public override CfgEncoder Encode()=> new CfgEncoder()
                .Add_IfNotZero("ty", (int)type)
                .Add_IfNotZero("val", updateValue)
                .Add("ind", EncodeIndex);

        #endregion

      //  public void Apply(TriggerValues to) => type.Apply(updateValue, this, to);


        public void Apply()
        {
            switch (type)
            {
               // case ResultType.SetBool: Values[this] = (updateValue > 0); break;
                case ResultType.Set: Values[this] = updateValue; break;
                case ResultType.Add: Values[this] += updateValue; break;
                case ResultType.Subtract: Values[this] -= updateValue; break;
                    //  case ResultType.SetTimeReal:    dest.SetInt(so, LogicMGMT.RealTimeNow());                           break;
                    //  case ResultType.SetTimeGame:    dest.SetInt(so, (int)Time.time);                                    break;
                    //   case ResultType.SetTagBool:     so.SetTagBool(dest.groupIndex, dest.triggerIndex, updateValue > 0); break;
                    //   case ResultType.SetTagInt:      so.SetTagEnum(dest.groupIndex, dest.triggerIndex, updateValue);     break;
            }
        }

        public int GetValue() => Values[this];

        //  public override bool IsBoolean => type == ResultType.SetBool;// || type == ResultType.SetTagBool);

        /* public Result()  {
             if (TriggerGroup.Browsed != null)
                 groupId = TriggerGroup.Browsed.IndexForInspector;
         }*/

        #region Inspector
        /* public override string GetNameForInspector() =>
             "{0} : {1} {2} ".F(base.GetNameForInspector(), type.GetText(), 
             //IsBoolean ? (updateValue != 0).ToString() : updateValue.ToString()
             Trigger.Usage.GetResultName(this)
             );*/

        /*  public override bool PEGI_inList_Sub(int ind, ref int inspecte) {

              var changed = pegi.ChangeTrackStart();

              FocusedField_PEGI(ind, "Res");
              Trigger.Usage.Inspect(this);

              return changed;
          }*/

        #endregion

    }

}