using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;

namespace SpriteAnimator.ViewModels
{
    public class CreateAtlasPromptViewModel : Screen
    {
        public CreateAtlasPromptViewModel(String fileName)
        {
            this.FileName = fileName;
        }

        #region Commands
        public void Okay()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
        #endregion

        #region Properties
        private String fileName = String.Empty;
        public String FileName
        {
            get { return fileName; }
            set
            {
                if (value == fileName)
                    return;

                fileName = value;
                NotifyOfPropertyChange(() => FileName);
            }
        }
        #endregion
    }
}
