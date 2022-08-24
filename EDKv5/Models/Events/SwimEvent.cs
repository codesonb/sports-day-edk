namespace EDKv5
{
    class SwimEvent : TrackEvent
    {
        public SwimEvent(string ID, string name, bool assignLane, bool useLongTime) : base(ID, name, assignLane, useLongTime) { }

        public override bool IsSwim { get { return true; } }
    }
}
