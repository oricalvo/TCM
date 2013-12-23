using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace TCM.WinForm.MVVM
{
    public class TextBoxBinder : ControlBinder
    {
        private TextBox textBox;
        private string propName;

        public TextBoxBinder(TextBox textBox, string propName)
            : base(textBox, propName)
        {
            this.textBox = textBox;
            this.propName = propName;
        }

        public override void Attach(object viewModel)
        {
            base.Attach(viewModel);

            if (this.RootBinder.AutoModelUpdate)
            {
                this.textBox.LostFocus += new EventHandler(textBox_LostFocus);
            }
        }

        public override void Detach()
        {
            if (this.RootBinder.AutoModelUpdate)
            {
                this.textBox.LostFocus -= new EventHandler(textBox_LostFocus);
            }
        }

        public override void UpdateModel()
        {
            string text = this.textBox.Text.Trim();
            if(text == string.Empty)
            {
                //
                //  Allow empty if the bound property is nullable
                //
                Type propType = GetPropertyType();
                if(propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable))
                {
                    SetModelProp(this.propName, null);
                    return;
                }
            }

            SetModelProp(this.propName, this.textBox.Text);
        }

        public override void UpdateView()
        {
            this.textBox.Text = Convert.ToString(GetModelProp(this.propName));
        }

        void textBox_LostFocus(object sender, EventArgs e)
        {
            if (this.IsUpdateViewSuspended)
            {
                return;
            }

            UpdateModel();
        }
    }
}
