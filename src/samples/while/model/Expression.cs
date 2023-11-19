using csly.whileLang.compiler;

namespace csly.whileLang.model
{
    public interface Expression : WhileAST
    {
        WhileType Whiletype { get; set; }
    }
}