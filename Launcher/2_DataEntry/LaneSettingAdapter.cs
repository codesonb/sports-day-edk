using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EDKv5;
using EDKv5.Protocols;
using System.Windows;
using System.ComponentModel;

namespace Launcher
{
    sealed class LaneSettingAdapter : DependencyObject, ILaneSetting
    {
        private static DependencyProperty LaneProperty =
            DependencyProperty.Register("Lane", typeof(short), typeof(ILaneSetting));
        private static DependencyProperty PIDProperty =
            DependencyProperty.Register("PID", typeof(string), typeof(ILaneSetting));
        private static DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(ILaneSetting));
        private static DependencyProperty HouseColorProperty =
            DependencyProperty.Register("HouseColor", typeof(int), typeof(ILaneSetting));
        private static DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(uint), typeof(LaneSettingAdapter));
        private static DependencyProperty RankProperty =
            DependencyProperty.Register("Rank", typeof(short), typeof(LaneSettingAdapter));
        private static DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(string), typeof(LaneSettingAdapter));
        private static DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(ResultState), typeof(LaneSettingAdapter));

        public static ICompetitionResultType ResultType;

        public LaneSettingAdapter(ILaneSetting lane)
        {
            this._lane = lane;
            this.ResultObject = ResultType.CreateResult(lane);
            SetValue(ResultProperty, ResultObject.Result);
            SetValue(RankProperty, ResultObject.Rank);
        }

        ILaneSetting _lane;
        public CompetitionResult ResultObject { get; private set; }

        public short Lane {
            get { return _lane.Lane; }
        }
        public string PID
        {
            get { return _lane.PID; }
        }
        public string Name
        {
            get { return _lane.Name; }
        }
        public int HouseColor
        {
            get { return _lane.HouseColor; }
        }
        public uint Value
        {
            get { return _lane.Value; }
            set
            {
                _lane.Value = value;
                SetValue(ValueProperty, value);
            }
        }
        public short Rank
        {
            get { return ResultObject.Rank; }
            set { ResultObject.Rank = value; SetValue(RankProperty, value); }
        }
        public ResultState State
        {
            get { return _lane.State; }
            set { _lane.State = value; SetValue(StateProperty, value); }
        }

        //-----------------------------------
        public string Result
        {
            get { return ResultObject.Result; }
            set
            {
                string _old = ResultObject.Result;
                ResultObject.Result = value;
                SetValue(ResultProperty, ResultObject.Result);
            }
        }

        public bool IsSuppressInput(string original, char nextChar)
        {
            return ResultObject.IsSuppressInput(original, nextChar);
        }


    }
}
