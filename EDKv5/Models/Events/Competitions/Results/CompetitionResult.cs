using System;
using System.Linq;
using System.Collections.Generic;

namespace EDKv5
{
    public abstract class CompetitionResult : IComparable<CompetitionResult>, ICloneable
    {
        // public no-argument constructor

        // property
        public Participant Participant { get; internal set; }
        public short Rank { get; set; }
        public short ComputedRank { get; set; }
        public short Lane { get; set; }

        // public function
        public bool IsSuppressInput(string original, char nextChar)
        {
            if (State != ResultState.Rank)
                return true;

            if (original.Length >= 6)
                return true;

            switch (nextChar)
            {
                case '+':       // TODO:: make it configurable in the future
                case '-':
                case '*':
                    return (0 < original.Length);
                case '.':
                    return !(original.Length > 0 && this.isAllowDecimal()) || original.Contains('.');
                default:
                    return nextChar < '0' || nextChar > '9';
            }
        }
        public object Clone()
        {
            Type type = this.GetType();
            var obj = (CompetitionResult)type.GetConstructor(new Type[0]).Invoke(new object[0]);
            obj._val = this._val;
            obj.Participant = this.Participant;
            obj.Lane = this.Lane;
            return obj;
        }

        // abstracts
        protected virtual bool isAllowDecimal() { return false; }

        // private special
        private uint _val;
        public uint Value
        {
            get { return _val; }
            internal set { _val = value; }
        }

        protected byte h
        {
            get
            {
                uint k = _val & 0x00FF0000;
                k = k >> 16;
                return (byte)k;
            }
            set
            {
                uint k = value;
                k = k << 16;

                _val = _val & 0xFF00FFFF;
                _val = _val | k;
            }
        }
        protected byte m
        {
            get
            {
                uint k = _val & 0x0000FF00;
                k = k >> 8;
                return (byte)k;
            }
            set
            {
                uint k = value;
                k = k << 8;

                _val = _val & 0xFFFF00FF;
                _val = _val | k;
            }
        }
        protected byte s
        {
            get
            {
                uint k = _val & 0x000000FF;
                return (byte)k;
            }
            set
            {
                uint k = value;
                _val = _val & 0xFFFFFF00;
                _val = _val | k;
            }
        }
        protected ushort n
        {
            get
            {
                uint k = _val & 0x0000FFFF;
                return (ushort)k;
            }
            set
            {
                uint k = value;
                _val = _val & 0xFFFF0000;
                _val = _val | k;
            }
        }

        public ResultState State { get; internal set; }

        public string Result
        {
            get
            {
                if (State == ResultState.Rank)
                    return _ext_result;
                else
                    return State.ToString();
            }
            set
            {
                _val = 0;
                switch (value)
                {
                    case "+":
                        State = ResultState.Leave;
                        break;
                    case "*":
                        State = ResultState.Disqualified;
                        break;
                    case "-":
                        State = ResultState.Absent;
                        break;
                    default:
                        State = ResultState.Rank;
                        _ext_result = value;
                        break;
                }
            }
        }

        protected abstract string _ext_result { get; set; }
        public static void ComputeRanks(IEnumerable<CompetitionResult> oriResults)
        {
            //=== calculate ranks

            // prepare data structure & references for sorting
            List<CompetitionResult> results = new List<CompetitionResult>(oriResults);

            // According to MSDN, https://msdn.microsoft.com/en-us/library/b0zbh7b6(v=vs.110).aspx
            // - If the partition size is fewer than 16 elements, it uses an insertion sort algorithm.
            // - If the number of partitions exceeds 2 * LogN, where N is the range of the input array, it uses a Heapsort algorithm.
            // - Otherwise, it uses a Quicksort algorithm.
            results.Sort();

            // assign rankings - for same results and disqualification
            byte rank = 1;
            byte gap = 1;
            uint last = 0;
            int idx = 0;
            for (; idx < results.Count; idx++)      // seek the first rank
            {
                if (ResultState.Rank != results[idx].State || 0 == results[idx].Value)  // reject zero values
                    results[idx].ComputedRank = 0;
                else
                {
                    results[idx].ComputedRank = 1;
                    last = results[idx].Value;
                    break;
                }
            }
            for (++idx; idx < results.Count; idx++) // skip rank 1 and continue
            {
                if (ResultState.Rank != results[idx].State || 0 == results[idx].Value)  // reject zero values
                {   // check for Field events
                    results[idx].ComputedRank = 0;
                }
                else
                {
                    if (last == results[idx].Value)
                    {
                        gap++;
                    }
                    else
                    {
                        rank += gap;
                        gap = 1;
                    }
                    results[idx].ComputedRank = rank;
                    last = results[idx].Value;
                }
            }

        } // end void ComputeRanks
        public virtual int CompareTo(CompetitionResult obj)
        {
            return _val.CompareTo(obj._val);
        }
    }
}
