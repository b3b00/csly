using System;
using System.Collections.Generic;
using System.Text;
using sly.lexer;

namespace sly.parser.parser
{


    public class Group<IN,OUT>
    {
        public List<GroupItem<IN,OUT>> Items;

        public Dictionary<string,GroupItem<IN,OUT>> ItemsByName;

        public int Count => Items.Count;

        public Group() {
            Items = new List<GroupItem<IN,OUT>>();
            ItemsByName = new Dictionary<string,GroupItem<IN,OUT>>();
        }

        

        public OUT Value(int i) {
            return Items[i].Value;
        }

        public Token<IN> Token(int i) {
            return Items[i].Token;
        }


        public OUT Value(string name) {
            return ItemsByName.ContainsKey(name) ? ItemsByName[name].Value : default(OUT);
        }

        public Token<IN> Token(string name) {
            return ItemsByName.ContainsKey(name) ? ItemsByName[name].Token : null;
        }



  


    }
}
