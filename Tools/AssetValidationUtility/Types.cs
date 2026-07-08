using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobloxFiles;

namespace RobloxAssetValidation
{
    public interface Types
    {
        string FileName { get; }
        void PullAdditionalInfo(string File);
    }

// i decided to add I at the start to not collide with Roblox File Format Library
    class IPlace : Types
    {
        public string FileName { get; private set; } = "";
        bool IsPlace = true;
        public void PullAdditionalInfo(string File)
        {
            var file = RobloxFile.Open(File);
            if (file.FindFirstChildOfClass<Workspace>() == null)
            {
                // magic had to happen here for this scenario
                IsPlace = false;
            }
            else
            {
                IsPlace = true;
            }
        }
    }

    public class IModel : Types
    {
        public string FileName { get; private set; }
        public void PullAdditionalInfo(string File)
        {
            var file = RobloxFile.Open(File);
            if (file.FindFirstChildOfClass<Model>() == null)
            {
            }
            else
            {
                FileName = file.FindFirstChildOfClass<Model>().ToString();
                // add more info later maybe?
            }
        }
    }
}
