using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;

namespace TCM.WinForm.MVVM
{
    public class MVVMBindings
    {
        private MVVMBinder binder;

        public MVVMBindings()
        {
            this.binder = new MVVMBinder();
        }

        public MVVMBinder Binder
        {
            get
            {
                return this.binder;
            }
        }

        public MVVMBinder Done(object viewModel)
        {
            this.binder.Apply(viewModel);
            return this.binder;
        }

        public TextBoxBindings TextBox(TextBox textBox, string propName)
        {
            TextBoxBindings binder = new TextBoxBindings(this, textBox, propName);
            return binder;
        }

        public RadioGroupBindings RadioGroup(RadioButton[] buttons, string propName)
        {
            RadioGroupBindings binder = new RadioGroupBindings(this, buttons, propName);
            return binder;
        }

        public LabelBindings Label(Label label, string propName)
        {
            LabelBindings binder = new LabelBindings(this, label, propName);
            return binder;
        }

        public ComboBoxBindings ComboBox(ComboBox comboBox, string propName)
        {
            ComboBoxBindings binder = new ComboBoxBindings(this, comboBox, propName);
            return binder;
        }

        public CheckBoxBindings CheckBox(CheckBox checkBox, string propName)
        {
            CheckBoxBindings binder = new CheckBoxBindings(this, checkBox, propName);
            return binder;
        }

        public NumericUpDownBindings NumericUpDown(NumericUpDown control, string propName)
        {
            NumericUpDownBindings binder = new NumericUpDownBindings(this, control, propName);
            return binder;
        }

        public MVVMBindings ErrorProvider(ErrorProvider errorProvider)
        {
            this.binder.ErrorProvider = errorProvider;
            return this;
        }

        public MVVMBindings AutoModelUpdate(bool autoModelUpdate)
        {
            this.binder.AutoModelUpdate = autoModelUpdate;

            return this;
        }
    }

    public delegate void BindingValidationHandler(MVVMBinder binder, List<string> errors);

    public class ControlBindings<BindingT, BinderT> where BinderT : ControlBinder
    {
        private MVVMBindings root;
        private BinderT binder;

        public ControlBindings(MVVMBindings root, BinderT binder)
        {
            this.root = root;
            this.binder = binder;

            this.root.Binder.Add(this.binder);
        }

        protected BinderT Binder
        {
            get
            {
                return this.binder;
            }
        }

        public MVVMBindings Parent()
        {
            return this.root;
        }

        public BindingT Validation(BindingValidationHandler validation)
        {
            this.binder.SetValidation(validation);

            return (BindingT)(object)this;
        }

        public BindingT ErrorControl(Control cont)
        {
            this.binder.SetErrorControl(cont);

            return (BindingT)(object)this;
        }
    }

    public class TextBoxBindings : ControlBindings<TextBoxBindings, TextBoxBinder>
    {
        private TextBoxBinder binder;

        public TextBoxBindings(MVVMBindings root, TextBox textBox, string propName)
            : base(root, new TextBoxBinder(textBox, propName))
        {
        }
    }

    public class LabelBindings : ControlBindings<LabelBindings, LabelBinder>
    {
        public LabelBindings(MVVMBindings root, Label label, string propName)
            : base(root, new LabelBinder(label, propName))
        {
        }
    }

    public class CheckBoxBindings : ControlBindings<CheckBoxBindings, CheckBoxBinder>
    {
        public CheckBoxBindings(MVVMBindings root, CheckBox checkBox, string propName)
            : base(root, new CheckBoxBinder(checkBox, propName))
        {
        }
    }

    public class ComboBoxBindings : ControlBindings<ComboBoxBindings, ComboBoxBinder>
    {
        public ComboBoxBindings(MVVMBindings root, ComboBox comboBox, string propName)
            : base(root, new ComboBoxBinder(comboBox, propName))
        {
        }

        public ComboBoxBindings Values(IEnumerable values)
        {
            this.Binder.SetValues(values);

            return this;
        }

        public ComboBoxBindings TextPropName(string propName)
        {
            this.Binder.TextPropName = propName;

            return this;
        }

        public ComboBoxBindings ValuePropName(string propName)
        {
            this.Binder.ValuePropName = propName;

            return this;
        }

        public ComboBoxBindings ListPropName(string propName)
        {
            this.Binder.ListPropName = propName;

            return this;
        }

        public ComboBoxBindings InsertEmptyItem(bool insert)
        {
            this.Binder.InsertEmptyItem = insert;

            return this;
        }
    }

    public class NumericUpDownBindings : ControlBindings<NumericUpDownBindings, NumericUpDownBinder>
    {
        public NumericUpDownBindings(MVVMBindings root, NumericUpDown control, string propName)
            : base(root, new NumericUpDownBinder(control, propName))
        {
        }
    }

    public class RadioGroupBindings : ControlBindings<RadioGroupBindings, RadioGroupBinder>
    {
        public RadioGroupBindings(MVVMBindings root, RadioButton[] buttons, string propName)
            : base(root, new RadioGroupBinder(buttons, propName))
        {
        }
    }
}
