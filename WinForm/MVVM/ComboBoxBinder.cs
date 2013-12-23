using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;

namespace TCM.WinForm.MVVM
{
    public class ComboBoxBinder : ControlBinder
    {
        private ComboBox comboBox;
        private string textPropName;
        private string valuePropName;
        private string selectedPropName;
        private string listPropName;
        private IEnumerable values;
        private bool insertEmptyItem;

        public ComboBoxBinder(ComboBox comboBox, string propName) : base(comboBox, propName)
        {
            this.comboBox = comboBox;
            this.selectedPropName = propName;
        }

        public override void Attach(object viewModel)
        {
            base.Attach(viewModel);

            if (this.RootBinder.AutoModelUpdate)
            {
                this.comboBox.SelectedIndexChanged += new EventHandler(comboBox_SelectedIndexChanged);
            }
        }

        public override void Detach()
        {
            if (this.RootBinder.AutoModelUpdate)
            {
                this.comboBox.SelectedIndexChanged -= new EventHandler(comboBox_SelectedIndexChanged);
            }
        }

        public override void UpdateModel()
        {
            ComboBoxEntry entry = (ComboBoxEntry)this.comboBox.SelectedItem;
            if (entry != null)
            {
                SetModelProp(this.selectedPropName, entry.Value);
            }
        }

        public override void UpdateView()
        {
            ComboBoxEntry selectedEntry = null;

            object selectedValue = GetModelProp(this.selectedPropName);

            this.comboBox.Items.Clear();
            IEnumerable values = this.values;
            if (values == null)
            {
                values = (IEnumerable)GetModelProp(this.listPropName);
            }

            if (this.insertEmptyItem)
            {
                ComboBoxEntry entry = new ComboBoxEntry()
                {
                    Text = "",
                    Value = null,
                };
                this.comboBox.Items.Add(entry);
            }

            foreach (object obj in values)
            {
                string text;
                if (this.textPropName != null)
                {
                    text = (string)obj.GetType().GetProperty(this.textPropName).GetValue(obj, new object[0]);
                }
                else
                {
                    text = obj.ToString();
                }

                object value = obj;
                if (this.valuePropName != null)
                {
                    value = obj.GetType().GetProperty(this.valuePropName).GetValue(obj, new object[0]);
                }

                ComboBoxEntry entry = new ComboBoxEntry()
                {
                    Text = text,
                    Value = value,
                };
                this.comboBox.Items.Add(entry);

                if (value.Equals(selectedValue))
                {
                    selectedEntry = entry;
                }
            }

            if (selectedEntry != null)
            {
                this.comboBox.SelectedItem = selectedEntry;
            }
        }

        void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.IsUpdateViewSuspended)
            {
                return;
            }

            UpdateModel();
        }

        public void SetValues(IEnumerable values)
        {
            this.values = values;
        }

        public string ListPropName
        {
            get
            {
                return this.listPropName;
            }
            set
            {
                this.listPropName = value;
            }
        }

        public string TextPropName
        {
            get
            {
                return this.textPropName;
            }
            set
            {
                this.textPropName = value;
            }
        }

        public string ValuePropName
        {
            get
            {
                return this.valuePropName;
            }
            set
            {
                this.valuePropName = value;
            }
        }

        public bool InsertEmptyItem
        {
            get
            {
                return this.insertEmptyItem;
            }
            set
            {
                this.insertEmptyItem = value;
            }
        }

        class ComboBoxEntry
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            {
                return this.Text;
            }
        }
    }
}
