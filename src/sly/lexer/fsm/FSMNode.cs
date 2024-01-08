namespace sly.lexer.fsm
{
    public class FSMNode<N>
    {
        internal FSMNode(N value)
        {
            Value = value;
        }

        internal N Value { get; set; }

        internal int Id { get; set; }

        internal bool IsEnd { get; set; }

        public string Mark { get; internal set; }
        public bool IsLineEnding { get; set; }
        
        public bool IsPushModeNode { get; set; }
        
        public string PushToMode { get; set; }

        public bool IsPopModeNode { get; set; }
        
        public string GraphVizNodeLabel()
        {
            var push = IsPushModeNode ? "Push(" + PushToMode + ")" : "";
            var pop = IsPopModeNode ? "Pop()" : "";
            return (Mark ?? "")+ push+ " "+pop+" #"+Id + (IsEnd ? $"[{Value}]" : "");
        }
    }
}