using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace TCM.WinForm.MVVM
{
    public class NumericUpDownBinder : ControlBinder
    {
        private NumericUpDown control;
        private string propName;

        public NumericUpDownBinder(NumericUpDown control, string propName)
            : base(control, propName)
        {
            this.control = control;
            this.propName = propName;
        }

        public override void Attach(object viewModel)
        {
            base.Attach(viewModel);

            if (this.RootBinder.AutoModelUpdate)
            {
                this.control.ValueChanged += new EventHandler(control_ValueChanged);
            }
        }

        public override void Detach()
        {
            if (this.RootBinder.AutoModelUpdate)
            {
                this.control.ValueChanged -= new EventHandler(control_ValueChanged);
            }
        }

        public override void UpdateModel()
        {
            SetModelProp(this.propName, this.control.Value);
        }

        public override void UpdateView()
        {
            this.control.Value = Convert.ToDecimal(GetModelProp(this.propName));
        }

        void control_ValueChanged(object sender, EventArgs e)
        {
            if (this.IsUpdateViewSuspended)
            {
                return;
            }

            UpdateModel();
        }
    }
}
