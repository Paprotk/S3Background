using Sims3.UI;

namespace S3MenuBackground
{
	public class ModInfoDialogCustom : ModalDialog
	{
		public static ModInfoDialogCustom.Result Show(string[] modFileNames)
		{
			ModInfoDialogCustom.Result result = ModInfoDialogCustom.Result.OK;
			if (modFileNames != null && modFileNames.Length > 0)
			{
				using (ModInfoDialogCustom modInfoDialog = new ModInfoDialogCustom(modFileNames))
				{
					modInfoDialog.StartModal();
					result = modInfoDialog.mResult;
					modInfoDialog.Dispose();
				}
			}
			return result;
		}
        
		public ModInfoDialogCustom(string[] packageFileNames) : base("ModInfoDialogCustom", 161369792, true, ModalDialog.PauseMode.PauseTask, null)
		{
			this.mOkButton = (this.mModalDialogWindow.GetChildByID(161369840U, true) as Button);
			this.mTextEdit = (this.mModalDialogWindow.GetChildByID(161369824U, true) as TextEdit);
			this.mOkButton.Click += this.OnOkClick;
			string text = string.Empty;
			foreach (string obj in packageFileNames)
			{
				text = text + obj + '\n';
			}
			this.mTextEdit.Caption = (string.IsNullOrEmpty(text) ? string.Empty : text);
		}
        
		public void OnOkClick(WindowBase sender, UIButtonClickEventArgs eventArgs)
		{
			this.mResult = ModInfoDialogCustom.Result.OK;
			this.StopModal();
		}
        
		public const string kLayoutName = "ModInfoDialogCustom";
        
		public const int kWinExportID = 161369792;
        
		public ModInfoDialogCustom.Result mResult;
        
		public Button mOkButton;

		public TextEdit mTextEdit;
        
		public enum ControlIDs : uint
		{
			OkButton = 161369840U,
			TextEdit = 161369824U
		}
        
		public enum Result
		{
			OK
		}
	}
}
