using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 namespace AnimGenerator
{


    public class ActSplitList
    {
        public List<ActSplitElement> act_split_list;

        public ActSplitList(List<ActSplitElement> act_split_list)
        {
            this.act_split_list = act_split_list;
        }
        public ActSplitList()
        {
            this.act_split_list = new List<ActSplitElement>();
        }

        public ActSplitElement getActElementByTypeName(string actType)
        {
            foreach (ActSplitElement actSplitElement in act_split_list)
            {
                if (actSplitElement.actType == actType)
                {
                    return actSplitElement;
                }
            }
            return null;
        }

        public ActSplitElement getActElementByRawname(string actRawname)
        {
            foreach (ActSplitElement actSplitElement in act_split_list)
            {
                if (actSplitElement.actRawNameList.Contains(actRawname))
                {
                    return actSplitElement;
                }
            }
            return null;
        }

        public bool containsRawActName(string actRawname)
        {
            foreach (ActSplitElement actSplitElement in act_split_list)
            {
                if (actSplitElement.actRawNameList.Contains(actRawname))
                {
                    return true;
                }
            }
            return false;
        }

    }

    public class ActSplitElement
    {
        public string actType { set; get; }
        public string defaultPlanner { set; get; }
        public List<string> actRawNameList { set; get; }

        public List<SplitCondition> SplitConditionList { set; get; }

        public ActSplitElement(string actType, string defaultPlanner, List<string> actRawNameList, List<SplitCondition> splitConditionList)
        {
            this.actType = actType;
            this.defaultPlanner = defaultPlanner;
            this.actRawNameList = actRawNameList;
            SplitConditionList = splitConditionList;
        }
    }

    public class SplitCondition
    {
        public string condition { set; get; }
        public List<AtomMove> atomMoveList { set; get; }

        public SplitCondition(string condition, List<AtomMove> atomMoveList)
        {
            this.condition = condition;
            this.atomMoveList = atomMoveList;
        }
    }

    public class AtomMove
    {
        public string subAnimation { set; get; }
        public string subAtomMove { set; get; }
        public string subPointFlag { set; get; }
        public string objAnimation { set; get; }
        public string objAtomMove { set; get; }
        public string objPointFlag { set; get; }
        public int seqNum { set; get; }
        public string duration { set; get; }

        public AtomMove(string subAnimation, string subAtomMove, string subPointFlag, string objAnimation, string objAtomMove, string objPointFlag, int seqNum, string duration)
        {
            this.subAnimation = subAnimation;
            this.subAtomMove = subAtomMove;
            this.subPointFlag = subPointFlag;
            this.objAnimation = objAnimation;
            this.objAtomMove = objAtomMove;
            this.objPointFlag = objPointFlag;
            this.seqNum = seqNum;
            this.duration = duration;
        }
    }


}
