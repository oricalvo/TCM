using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace TCM.WinForm.MVVM
{
    public class LabelBinder : ControlBinder
    {
        private Label label;
        private string propName;

        public LabelBinder(Label label, string propName)
            : base(label, propName)
        {
            this.label = label;
            this.propName = propName;
        }

        public override void Attach(object viewModel)
        {
            base.Attach(viewModel);
        }

        public override void Detach()
        {
        }

        public override void UpdateModel()
        {
        }

        public override void UpdateView()
        {
            this.label.Text = GetModelProp(this.propName).ToString();
        }
    }
}
