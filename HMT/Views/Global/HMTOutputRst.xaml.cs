using HMT.Copilot;
using HMT.Models;
using Newtonsoft.Json;
using RestSharp;
using suiren.Models;
using suiren.Services;
using suiren.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HMT.Views.Global
{
    public class HMTOutputRstViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<HMTOutputRstMessage> _messages = new ObservableCollection<HMTOutputRstMessage>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<HMTOutputRstMessage> Messages
        {
            get => _messages;
            set => SetField(ref _messages, value);
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Interaction logic for HMTOutputRst.xaml
    /// </summary>
    public partial class HMTOutputRst : UserControl
    {
        public HMTOutputRst()
        {
            InitializeComponent();
        }
    }
}
