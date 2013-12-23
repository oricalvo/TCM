using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace TCM.WinForm.MVVM
{
    public class RadioGroupBinder : ControlBinder
    {
        private RadioButton[] buttons;

        public RadioGroupBinder(RadioButton[] buttons, string propName)
            : base(buttons, propName)
        {
            this.buttons = buttons;
        }

        public override void Attach(object viewModel)
        {
            base.Attach(viewModel);

            Type propType = this.GetPropertyType();
            if (!propType.IsEnum)
            {
                throw new Exception("RadioGroupBinder supports only enum property");
            }

            Array values = Enum.GetValues(propType);
            if (values.Length != this.buttons.Length)
            {
                throw new Exception("Number of radio buttons mus be the same as the number of enum values");
            }

            for (int i = 0; i < values.Length; i++)
            {
                object val = values.GetValue(i);
                RadioButton button = this.buttons[i];

                button.Tag = val;
            }

            if (this.RootBinder.AutoModelUpdate)
            {
                foreach (RadioButton button in this.buttons)
                {
                    button.CheckedChanged += button_CheckedChanged;
                }
            }
        }

        void button_CheckedChanged(object sender, EventArgs e)
        {
            if (this.IsUpdateViewSuspended)
            {
                return;
            }

            UpdateModel();
        }

        public override void Detach()
        {
            if (this.RootBinder.AutoModelUpdate)
            {
                foreach (RadioButton button in this.buttons)
                {
                    button.CheckedChanged -= button_CheckedChanged;
                }
            }
        }

        public override void UpdateModel()
        {
            foreach (RadioButton button in this.buttons)
            {
                if (button.Checked)
                {
                    SetModelProp(this.PropName, button.Tag);
                    break;
                }
            }
        }

        public override void UpdateView()
        {
            object model = GetModelProp(this.PropName);
            foreach (RadioButton button in this.buttons)
            {
                button.Checked = button.Tag.Equals(model);
            }
        }
    }
}
