using Dan;

namespace GenerateEFModels
{
    public partial class FormGenerateEfModels : Form
    {
        public FormGenerateEfModels()
        {
            
            InitializeComponent();
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            ParseDbml parseDbml = new();
            parseDbml.Parse(textBoxDbml.Text);
        }


    }

  
}