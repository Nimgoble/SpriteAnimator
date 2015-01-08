using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Win32;

using Caliburn.Micro;

using SpriteAnimator.Models;

namespace SpriteAnimator.ViewModels
{
    public class OpenSetViewModel : Screen
    {
        #region Private Members
        private readonly IEventAggregator eventAggregator;
        private readonly IWindowManager windowManager;
        #endregion

        public OpenSetViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
        {
            this.eventAggregator = eventAggregator;
            this.windowManager = windowManager;
        }

        #region Commands
        public void ChooseFile()
        {
            OpenFileDialog chooseFileDialog = new OpenFileDialog();
            chooseFileDialog.DefaultExt = ".xml";
            chooseFileDialog.Filter = "(*.png, *bmp, *.jpg, *.jpeg, *.xml)|*.png;*.bmp;*.jpg;*.jpeg;*.xml";
            chooseFileDialog.Multiselect = false;

            bool? rtn = chooseFileDialog.ShowDialog();
            if (rtn == true)
                FilePath = chooseFileDialog.FileName;
        }

        public void LoadFile()
        {
            FileInfo info = new FileInfo(filePath);
            TextureAtlas textureAtlas = null;
            String atlasPath = String.Empty;
            if (info.Extension.ToLower().Contains("xml"))
                atlasPath = filePath;
            else
            {
                atlasPath = filePath.Replace(info.Extension, ".xml");
                //We're loading an image.  Look for the corresponding atlas in the same directory.
                if (!File.Exists(atlasPath))
                {
                    //Prompt for the atlas creation
                    CreateAtlasPromptViewModel vm = new CreateAtlasPromptViewModel(info.Name);
                    bool? rtn = windowManager.ShowDialog(vm);
                    if (rtn == true)
                    {
                        //Create an empty atlas file with the same name as the image in the same directory
                        textureAtlas = new TextureAtlas();
                        textureAtlas.ImagePath = info.Name; //Relative

                        try
                        {
                            using (FileStream stream = new FileStream(atlasPath, FileMode.OpenOrCreate))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(TextureAtlas));
                                serializer.Serialize(stream, textureAtlas);
                            }
                        }
                        catch (Exception ex)
                        {
                            String debugMe = String.Empty;
                            return;
                        }
                    }
                    else
                        return;
                }
            }

            //Try to deserialize the atlas path
            //Check for the image path
            //Load image
            using (FileStream stream = new FileStream(atlasPath, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TextureAtlas));
                textureAtlas = serializer.Deserialize(stream) as TextureAtlas;
            }

            String imagePath = textureAtlas.ImagePath;
            if(!Path.IsPathRooted(imagePath))
                imagePath = String.Format(@"{0}/{1}", info.Directory, imagePath);

            TextureAtlasViewModel textureAtlasViewModel = new TextureAtlasViewModel(textureAtlas, imagePath);
	        FilePath = String.Empty;
            this.eventAggregator.Publish(new Events.AtlasLoadedEvent() { Atlas = textureAtlasViewModel });
        }
        public bool CanLoadFile
        {
            get { return (String.IsNullOrEmpty(FilePath) == false); }
        }
        #endregion

        #region Properties
        private String filePath = String.Empty;
        public String FilePath
        {
            get { return filePath; }
            set
            {
                if (value == filePath)
                    return;

                filePath = value;
                NotifyOfPropertyChange(() => FilePath);
                NotifyOfPropertyChange(() => CanLoadFile);
            }
        }
        #endregion
    }
}
