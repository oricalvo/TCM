using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace TCM.WinForm.MVVM
{
    public class CheckBoxBinder : ControlBinder
    {
        private CheckBox checkBox;
        private string propName;

        public CheckBoxBinder(CheckBox checkBox, string propName)
            : base(checkBox, propName)
        {
            this.checkBox = checkBox;
            this.propName = propName;
        }

        public override void Attach(object viewModel)
        {
            base.Attach(viewModel);

            if (this.RootBinder.AutoModelUpdate)
            {
                this.checkBox.CheckedChanged += new EventHandler(checkBox_CheckedChanged);
            }
        }

        public override void Detach()
        {
            if (this.RootBinder.AutoModelUpdate)
            {
                this.checkBox.CheckedChanged -= new EventHandler(checkBox_CheckedChanged);
            }
        }

        public override void UpdateModel()
        {
            SetModelProp(this.propName, this.checkBox.Checked);
        }

        public override void UpdateView()
        {
            this.checkBox.Checked = (bool)GetModelProp(this.propName);
        }

        void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.IsUpdateViewSuspended)
            {
                return;
            }

            UpdateModel();
        }
    }
}
