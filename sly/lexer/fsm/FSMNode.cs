namespace sly.lexer.fsm
{
    public class FSMNode<N>
    {
        internal FSMNode(N value)
        {
            Value = value;
        }

        internal N Value { get; set; }

        internal int Id { get; set; } = 0;

        internal bool IsEnd { get; set; } = false;

        internal bool IsStart { get; set; } = false;
        public string Mark { get; internal set; }
        public bool IsLineEnding { get; set; }
        
        public bool IsPushModeNode { get; set; }
        
        public string PushToMode { get; set; }

        public bool IsPopModeNode { get; set; }
        
        
    }
}