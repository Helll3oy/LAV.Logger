namespace LAV.Logger
{
    public readonly record struct EventId(int Id, string Name = null)
    {
        public int Id { get; } = Id;
        public string Name { get; } = Name;

        public static implicit operator EventId(int i)
        {
            return new EventId(i);
        }

        public override string ToString()
        {
            return Name ?? Id.ToString();
        }
    }
}
