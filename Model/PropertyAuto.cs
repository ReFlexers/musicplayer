using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace MusciPlayerWpf.Model
{
    class PropertyAuto : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
