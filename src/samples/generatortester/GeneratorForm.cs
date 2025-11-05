using interactiveCLI.forms;

namespace generatortester;


    [Form]
    public partial class GeneratorForm
    {
        
        public string[] SamplesDataSource() => ["counter", "factorial", "fibonacci", "quit"];

        [Input("Sample : ")]
        [DataSource(nameof(SamplesDataSource))]
        public string Sample { get; set; } = "";
        
        

    }
