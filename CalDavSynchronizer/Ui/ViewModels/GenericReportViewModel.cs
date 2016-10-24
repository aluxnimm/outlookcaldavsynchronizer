using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CalDavSynchronizer.Ui.ViewModels
{
  public class GenericReportViewModel : ViewModelBase
  {
    private string _reportText;
    private string _title;

    public event EventHandler CloseRequested;

    public GenericReportViewModel()
    {
      OkCommand = new DelegateCommand(_ => OnCloseRequested());
    }
    
    public ICommand OkCommand { get; }

    public string ReportText
    {
      get { return _reportText; }
      set { CheckedPropertyChange(ref _reportText, value); }
    }

    private void OnCloseRequested()
    {
      CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public static GenericReportViewModel DesingInstance => new GenericReportViewModel() { Title = "The title",  ReportText = "This is an important report" };

    public string Title
    {
      get { return _title; }
      set { CheckedPropertyChange (ref _title, value); }
    }

    public void AppendLine(string text)
    {
      ReportText = ReportText + Environment.NewLine + text;
    }

  }
}