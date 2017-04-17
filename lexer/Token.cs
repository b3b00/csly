namespace lexer {

public class Token<T>  {

    public T TokenType {get; set;}

    public int Line {get; set;}
    public int Column {get; set;}

    public string Value {get; set;}

    

}

}