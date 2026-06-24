using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.ViewModel
{
    public class DialogViewModel : BindableBase
    {
        private string _message;
        private readonly TaskCompletionSource<bool> _taskCompletionSource;
        public Task<bool> DialogTask => _taskCompletionSource.Task;

        #region Properties
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        public MyICommand<string> ResponseCommand { get; set; }
        #endregion

        public DialogViewModel(string message)
        {
            Message = message;
            _taskCompletionSource = new TaskCompletionSource<bool>();

            ResponseCommand = new MyICommand<string>(OnResponse);
        }

        private void OnResponse(string confirm) 
        {
           bool result = bool.Parse(confirm);

            _taskCompletionSource.SetResult(result);
        }
    }
}
