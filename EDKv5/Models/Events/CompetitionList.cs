using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDKv5
{
    public abstract partial class Event
    {
#if TEST
        public
#endif
        sealed class CompetitionList
        {
            // multiton
            static _I_CompetitionList TemporaryState = new _Temporary_CompetitionListState();
            static _I_CompetitionList AssignedState = new _Assigned_CompetitionListState();


            // field
            _I_CompetitionList _state = TemporaryState;
            internal List<Participant> _ls = new List<Participant>();
            internal Competition[] _cmp;


            public Competition[] Competitions
            {
                get
                {
                    if (null != _cmp) return _cmp; else return new Competition[0];
                }
                set
                {
                    if (null == value || value.Length <= 0)
                    {
                        if (_state != TemporaryState)
                        {
                            _state = TemporaryState;

                            _ls = new List<Participant>();
                            if (null != _cmp)
                            {
                                foreach (Competition cmp in _cmp)
                                    _ls.AddRange(cmp.Participants);
                            }
                        }
                    }
                    else if(_state != AssignedState)
                    {
                        _state = AssignedState;
                    }
                    _cmp = value;
                }
            }
            public bool IsClosed { get { return _state.IsClosed; } }

            public Participant[] Participants
            {
                get
                {
                    return _ls.ToArray();
                    //if (null == _cmp)
                    //    return _ls.ToArray();
                    //else
                    //{
                    //    List<Participant> ls = new List<Participant>();
                    //    foreach (Competition cp in Competitions)
                    //        ls.AddRange(cp.Participants);
                    //    return ls.ToArray();
                    //}
                }
            }

            public void Add(Participant participant) { _state.Add(this, participant); }
            public bool Contains(Participant participant) { return _state.Contains(this, participant); }
            public void ClearParticipants()
            {
                _state = TemporaryState;
                _cmp = null;
                _ls = new List<Participant>();
            }
        }
    }
}
