using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;

namespace TCM.WinForm.MVVM
{
    public class MVVMBinder
    {
        private object viewModel;
        private List<ControlBinder> binders;
        private int recursiveUpdateView;
        private Dictionary<string, string> errors;
        private ErrorProvider errorProvider;
        private bool autoModelUpdate;

        public MVVMBinder()
        {
            this.viewModel = null;
            this.binders = new List<ControlBinder>();
            this.recursiveUpdateView = 0;
            this.errors = new Dictionary<string, string>();
            this.autoModelUpdate = false;
        }

        public void Add(ControlBinder binder)
        {
            if (this.binders.Contains(binder))
            {
                throw new ArgumentException("Binder was already added");
            }

            this.binders.Add(binder);

            binder.OnAdded(this);
        }

        public void Apply(object viewModel)
        {
            this.viewModel = viewModel;

            foreach(ControlBinder binder in this.binders)
            {
                binder.Attach(viewModel);
            }

            UpdateView(null);
        }

        public object ViewModel
        {
            get
            {
                return this.viewModel;
            }
        }

        public bool UpdateModel()
        {
            foreach(ControlBinder binder in this.binders)
            {
                binder.UpdateModel();
            }

            foreach (ControlBinder binder in this.binders)
            {
                binder.ValidateModel();
            }

            bool hasErrors = this.HasErrors;
            if (hasErrors)
            {
                BindErrors();
            }

            return !hasErrors;
        }

        public void UpdateView()
        {
            UpdateView(null);
        }

        private void UpdateView(ControlBinder source)
        {
            if (++this.recursiveUpdateView == 10)
            {
                return;
            }

            try
            {
                foreach (ControlBinder binder in this.binders)
                {
                    if (binder == source)
                    {
                        continue;
                    }

                    try
                    {
                        binder.SuspendUpdate();

                        binder.UpdateView();
                    }
                    finally
                    {
                        binder.ResumeUpdate();
                    }
                }
            }
            finally
            {
                --this.recursiveUpdateView;
            }
        }

        internal void OnModelChanged(ControlBinder source)
        {
        }

        public void Detach()
        {
            foreach (ControlBinder binder in this.binders)
            {
                binder.Detach();
            }

            this.binders.Clear();
        }

        internal void OnBeginSetModelProp(string propName)
        {
            this.errors.Remove(propName);
        }

        internal void OnEndSetModelProp(string propName)
        {
        }

        internal void OnModelError(ControlBinder binder, string propName, string message)
        {
            //if (this.errorProvider != null)
            //{
            //    Control cont = binder.ErrorControl;
            //    if (cont != null)
            //    {
            //        this.errorProvider.SetError(cont, message);
            //    }
            //}

            this.errors[propName] = message;
        }

        public bool HasErrors
        {
            get
            {
                return this.errors.Count > 0;
            }
        }

        public Dictionary<string, string> GetErrors()
        {
            return this.errors;
        }

        public void BindErrors()
        {
            if (this.errorProvider == null)
            {
                return;
            }

            foreach (var entry in this.errors)
            {
                string propName = entry.Key;
                string message = entry.Value;

                ControlBinder binder = FindBinderByPropName(propName);
                if (binder != null)
                {
                    Control cont = binder.ErrorControl;
                    if (cont != null)
                    {
                        this.errorProvider.SetError(cont, message);
                    }
                }
            }
        }

        public void ClearErrors()
        {
            if (this.errorProvider == null)
            {
                return;
            }

            foreach(ControlBinder binder in this.binders)
            {
                Control cont = binder.ErrorControl;
                if (cont != null)
                {
                    this.errorProvider.SetError(cont, null);
                }
            }
        }

        private ControlBinder FindBinderByPropName(string propName)
        {
            foreach (ControlBinder binder in this.binders)
            {
                if (binder.PropName == propName)
                {
                    return binder;
                }
            }

            return null;
        }

        public ErrorProvider ErrorProvider
        {
            get
            {
                return this.errorProvider;
            }
            set
            {
                this.errorProvider = value;
            }
        }

        public bool AutoModelUpdate
        {
            get
            {
                return this.autoModelUpdate;
            }
            set
            {
                this.autoModelUpdate = value;
            }
        }
    }
}
