using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;

namespace TCM.WinForm.MVVM
{
    public abstract class ControlBinder
    {
        private MVVMBinder root;
        private object view;
        private string propName;
        private object viewModel;
        private int suspendCount;
        private BindingValidationHandler validateHandler;
        private Control errorControl;

        public ControlBinder(object cont, string propName)
        {
            this.root = null;
            this.view = cont;
            this.propName = propName;
            this.errorControl = null;
        }

        internal void OnAdded(MVVMBinder root)
        {
            this.root = root;
        }

        public virtual void Attach(object viewModel)
        {
            this.viewModel = viewModel;
            this.suspendCount = 0;
        }

        protected void OnModelChanged()
        {
            this.root.OnModelChanged(this);
        }

        public void SuspendUpdate()
        {
            ++this.suspendCount;
        }

        public void ResumeUpdate()
        {
            --this.suspendCount;
        }

        public bool IsUpdateViewSuspended
        {
            get
            {
                return this.suspendCount > 0;
            }
        }

        public abstract void UpdateModel();

        public abstract void UpdateView();

        public abstract void Detach();

        internal virtual void ValidateModel()
        {
            if (this.validateHandler != null)
            {
                List<string> errors = new List<string>();
                this.validateHandler(this.RootBinder, errors);

                foreach (string error in errors)
                {
                    this.root.OnModelError(this, this.PropName, error);
                }
            }
        }

        public ControlBinder Clone()
        {
            return (ControlBinder)this.MemberwiseClone();
        }

        internal void SetValidation(BindingValidationHandler validateHandler)
        {
            this.validateHandler = validateHandler;
        }

        protected Type GetPropertyType()
        {
            return GetPropertyInfo().PropertyType;
        }

        protected PropertyInfo GetPropertyInfo()
        {
            PropertyInfo propInfo = this.viewModel.GetType().GetProperty(this.propName);
            if (propInfo == null)
            {
                throw new Exception("Property " + this.propName + " was not found for viewModel " + this.viewModel.GetType().FullName);
            }

            return propInfo;
        }

        public object GetModelProp(string propName)
        {
            return this.viewModel.GetType().GetProperty(propName).GetValue(this.viewModel, new object[0]);
        }

        public void SetModelProp(string propName, object value)
        {
            this.root.OnBeginSetModelProp(propName);

            try
            {
                PropertyInfo propInfo = this.viewModel.GetType().GetProperty(this.propName);

                object oldValue = propInfo.GetValue(this.viewModel, new object[0]);
                if (ValuesEqual(value, oldValue))
                {
                    return;
                }

                //
                //  The .NET class Convert does not handle correctly conversion to nullable types
                //  TypeConverter does !
                //  However, TypeConverter does not support conversion between decimal and int
                //

                object convertedValue;
                try
                {
                    convertedValue = Convert.ChangeType(value, propInfo.PropertyType);
                }
                catch
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(propInfo.PropertyType);
                    convertedValue = converter.ConvertFrom(value);
                }

                propInfo.SetValue(this.viewModel, convertedValue, new object[0]);

                OnModelChanged();
            }
            catch (Exception err)
            {
                this.root.OnModelError(this, propName, err.Message);
            }
            finally
            {
                this.root.OnEndSetModelProp(propName);
            }
        }

        private bool ValuesEqual(object value, object oldValue)
        {
            if (oldValue == null && value == null)
            {
                return true;
            }

            if (oldValue != null)
            {
                return oldValue.Equals(value);
            }
            else
            {
                return false;
            }
        }

        public object ViewModel
        {
            get { return this.viewModel; }
        }

        public object View
        {
            get
            {
                return this.view;
            }
        }

        public Control ViewAsControl
        {
            get
            {
                return this.view as Control;
            }
        }

        public string PropName
        {
            get
            {
                return this.propName;
            }
        }

        public MVVMBinder RootBinder
        {
            get
            {
                return this.root;
            }
        }

        public void SetErrorControl(Control cont)
        {
            this.errorControl = cont;
        }

        public Control ErrorControl
        {
            get
            {
                if (this.errorControl != null)
                {
                    return this.errorControl;
                }

                return this.ViewAsControl;
            }
        }
    }
}
