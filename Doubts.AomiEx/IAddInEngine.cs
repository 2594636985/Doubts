using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Doubts.AomiEx
{
    public interface IAddInEngine
    {
        void AddAddInsFromDirectory(string addInDir);

        void AddAddInFile(string addInFile);

        void Initialize();

        List<T> BuildItems<T>(string path, object parameter, bool throwOnNotFound = true);
    }
}
