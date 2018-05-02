using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary1
{
    public interface IProjectService
    {
        void SaveProject();

        void CloseProject();

        void LoadProject(string filename);

        void Compare();
    }
}