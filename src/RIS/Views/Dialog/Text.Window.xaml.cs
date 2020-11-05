#region

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using RIS.ViewModels;
using SRS.Utilities;

#endregion

namespace RIS.Views
{
    public partial class TextWindow : Window
    {
        public TextWindow(string result)
        {
            try
            {
                InitializeComponent();

                var _viewModel = new TextViewModel();
                _viewModel.CloseRequested += (sender, e) => { Close(); };
                DataContext = _viewModel;

                var mcFlowDoc = new FlowDocument();
                var para = new Paragraph();
                para.Inlines.Add(result);
                mcFlowDoc.Blocks.Add(para);
                uxField_Result.Document = mcFlowDoc;
            }
            catch (Exception ex)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), ex);
            }
        }
    }
}