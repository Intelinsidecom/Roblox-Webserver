using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobloxFiles;

namespace RobloxAssetValidation
{
    public class Analysis
    {
        public static bool AnalyzeFile(string File)
        {
            var file = RobloxFile.Open(File);

            var FileFormat = "";

            if (file is BinaryRobloxFile)
            {
                // LZ4 compressed
                FileFormat = "Binary";
                Console.WriteLine("This file used Roblox's binary format!");
            }
            else
            {
                // primal
                FileFormat = "XML";
                Console.WriteLine("This file used Roblox's xml format!");
            }

            // various checks to determine what file is this exactly and call interface method to pull more info
            var Isworkspace = file.FindFirstChildOfClass<Workspace>();
            var IsModel = file.FindFirstChildOfClass<Model>();


            // Place
            if (Isworkspace == null)
            {
            }
            else
            {
                IPlace Work = new IPlace();
                IValidatePlace PlaceVal = new IValidatePlace();
                if (PlaceVal.ValidateAsset(File) == true)
                {
                    Console.WriteLine("Validation Successful");
                    Work.PullAdditionalInfo(File);
                    return true;
                }
            }

            // Model
            if (IsModel == null)
            {
                // not a model
            }
            else
            {
                IModel Model = new IModel();
                IValidateModel ModelVal = new IValidateModel();
                if (ModelVal.ValidateAsset(File) == true)
                {
                    Model.PullAdditionalInfo(File);
                    string ModelName = (Model.FileName);
                    return true;
                }
            }
            return false;
        }
    }
}
