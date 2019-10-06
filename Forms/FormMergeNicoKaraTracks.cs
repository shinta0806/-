// ============================================================================
// 
// メインウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using MergeNicoKaraTracks.SharedMisc;

using Shinta;

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MergeNicoKaraTracks.Forms
{
	public partial class FormMergeNicoKaraTracks : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormMergeNicoKaraTracks()
		{
			InitializeComponent();
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 環境設定
		private MnktSettings mMnktSettings;

		// 終了時タスク安全中断用
		private CancellationTokenSource mClosingCancellationTokenSource = new CancellationTokenSource();

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 入力元動画を参照
		// --------------------------------------------------------------------
		private void BrowseInputMovie(Label oTargetLabel, String oMovieKind)
		{
			try
			{
				OpenFileDialogMisc.Title = "入力元動画（" + oMovieKind + "）の場所を指定";
				OpenFileDialogMisc.Filter = "mp4 動画ファイル|*" + Common.FILE_EXT_MP4 + "|すべてのファイル|*.*";
				if (OpenFileDialogMisc.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				oTargetLabel.Text = OpenFileDialogMisc.FileName;
				UpdateAllInputMovieUi();
				UpdateGuide();
				UpdateOutputPath();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "入力元動画参照時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 入力値のチェック
		// --------------------------------------------------------------------
		private void Check()
		{
			if (String.IsNullOrEmpty(LabelInputMovieOn.Text))
			{
				throw new Exception("オンボーカル動画を指定して下さい。");
			}
			if (String.IsNullOrEmpty(LabelInputMovieOff.Text))
			{
				throw new Exception("オフボーカル動画を指定して下さい。");
			}
			if (String.IsNullOrEmpty(TextBoxOutputMovie.Text))
			{
				throw new Exception("出力ファイル名を指定して下さい。");
			}
			if (TextBoxOutputMovie.Text == LabelInputMovieOn.Text)
			{
				throw new Exception("出力ファイル名がオンボーカル動画と同じです。");
			}
			if (TextBoxOutputMovie.Text == LabelInputMovieOn2.Text)
			{
				throw new Exception("出力ファイル名がオンボーカル 2 動画と同じです。");
			}
			if (TextBoxOutputMovie.Text == LabelInputMovieOff.Text)
			{
				throw new Exception("出力ファイル名がオフボーカル動画と同じです。");
			}
			if (TextBoxOutputMovie.Text == LabelInputMovieOff2.Text)
			{
				throw new Exception("出力ファイル名がオフボーカル 2 動画と同じです。");
			}
			String aMp4BoxPath = Path.GetFullPath(mMnktSettings.Mp4boxPathSeed);
			if (!File.Exists(aMp4BoxPath))
			{
				throw new Exception("指定された MP4Box.exe は存在しません：\n" + aMp4BoxPath);
			}
		}

		// --------------------------------------------------------------------
		// 入力元動画の情報をすべてクリア
		// --------------------------------------------------------------------
		private void ClearAllInputMovie()
		{
			ClearInputMovieOn();
			ClearInputMovieOn2();
			ClearInputMovieOff();
			ClearInputMovieOff2();
		}

		// --------------------------------------------------------------------
		// 入力元動画（Off）の情報をクリア
		// --------------------------------------------------------------------
		private void ClearInputMovieOff()
		{
			LabelInputMovieOff.Text = null;
			TextBoxTrackNameAddOff.Text = null;
		}

		// --------------------------------------------------------------------
		// 入力元動画（Off2）の情報をクリア
		// --------------------------------------------------------------------
		private void ClearInputMovieOff2()
		{
			LabelInputMovieOff2.Text = null;
			TextBoxTrackNameAddOff2.Text = null;
		}

		// --------------------------------------------------------------------
		// 入力元動画（On）の情報をクリア
		// --------------------------------------------------------------------
		private void ClearInputMovieOn()
		{
			LabelInputMovieOn.Text = null;
			TextBoxTrackNameAddOn.Text = null;
		}

		// --------------------------------------------------------------------
		// 入力元動画（On2）の情報をクリア
		// --------------------------------------------------------------------
		private void ClearInputMovieOn2()
		{
			LabelInputMovieOn2.Text = null;
			TextBoxTrackNameAddOn2.Text = null;
		}

		// --------------------------------------------------------------------
		// EXE ファイルがドロップされた
		// --------------------------------------------------------------------
		private void FileDropExe(String oPath)
		{
			mMnktSettings.Mp4boxPathSeed = oPath;
			UpdateMp4box();
		}

		// --------------------------------------------------------------------
		// MP4 ファイルがドロップされた
		// --------------------------------------------------------------------
		private void FileDropMp4(String oPath)
		{
			if (String.IsNullOrEmpty(LabelInputMovieOn.Text))
			{
				LabelInputMovieOn.Text = oPath;
			}
			else if (String.IsNullOrEmpty(LabelInputMovieOff.Text))
			{
				LabelInputMovieOff.Text = oPath;
			}
			else if (String.IsNullOrEmpty(LabelInputMovieOn2.Text))
			{
				LabelInputMovieOn2.Text = oPath;
			}
			else if (String.IsNullOrEmpty(LabelInputMovieOff2.Text))
			{
				LabelInputMovieOff2.Text = oPath;
			}
			else
			{
				throw new Exception("既にすべての動画が指定されています。");
			}
			UpdateAllInputMovieUi();
			UpdateGuide();
			UpdateOutputPath();
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// 最初にログの設定をする
			SetLogWriter();

			// その他の設定
			SetMnktSettings();

			// タイトルバー
			Text = MnktConstants.APP_NAME_J;
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif

			// ステータスバー
			ToolStripStatusLabelStatus.Text = MnktConstants.APP_VER + "   /   " + MnktConstants.COPYRIGHT_J + "   ";

			// 映像使用
			RadioButtonOff.Checked = true;
		}

		// --------------------------------------------------------------------
		// Off2 が有効か
		// --------------------------------------------------------------------
		private Boolean IsOff2Enabled()
		{
			return RadioButtonOff2.Enabled;
		}

		// --------------------------------------------------------------------
		// On2 が有効か
		// --------------------------------------------------------------------
		private Boolean IsOn2Enabled()
		{
			return RadioButtonOn2.Enabled;
		}

		// --------------------------------------------------------------------
		// 結合
		// --------------------------------------------------------------------
		private Task Merge()
		{
			return Task.Run(() =>
			{
				try
				{
					// 終了時に強制終了されないように設定
					Thread.CurrentThread.IsBackground = false;

					// UI を処理中に
					Invoke(new Action(() =>
					{
						ButtonGo.Enabled = false;
						ToolStripProgressBarStatus.Visible = true;
					}));

					StringBuilder aParam = new StringBuilder();

					// 引数（映像）
					aParam.Append("-add \"");
					aParam.Append(VideoPath());
					aParam.Append("#video\" ");

					// 引数（On）
					aParam.Append("-add \"");
					aParam.Append(LabelInputMovieOn.Text);
					aParam.Append("#audio:name=");
					aParam.Append(TrackName("On", TextBoxTrackNameAddOn.Text));
					aParam.Append(":lang=ja\" ");

					if (IsOn2Enabled())
					{
						// 引数（On2）
						aParam.Append("-add \"");
						aParam.Append(LabelInputMovieOn2.Text);
						aParam.Append("#audio:name=");
						aParam.Append(TrackName("On", TextBoxTrackNameAddOn2.Text));
						aParam.Append(":lang=ja\" ");
					}

					// 引数（Off）
					aParam.Append("-add \"");
					aParam.Append(LabelInputMovieOff.Text);
					aParam.Append("#audio:name=");
					aParam.Append(TrackName("Off", TextBoxTrackNameAddOff.Text));
					aParam.Append("\" ");

					if (IsOff2Enabled())
					{
						// 引数（Off2）
						aParam.Append("-add \"");
						aParam.Append(LabelInputMovieOff2.Text);
						aParam.Append("#audio:name=");
						aParam.Append(TrackName("Off", TextBoxTrackNameAddOff2.Text));
						aParam.Append("\" ");
					}

					// 引数（出力）
					aParam.Append("-new \"");
					aParam.Append(TextBoxOutputMovie.Text);
					aParam.Append("\" ");

					// MP4Box 実行
					ProcessStartInfo aInfo = new ProcessStartInfo(Path.GetFullPath(mMnktSettings.Mp4boxPathSeed), aParam.ToString());
					aInfo.WindowStyle = ProcessWindowStyle.Hidden;
					Process aProcess = Process.Start(aInfo);
					aProcess.WaitForExit();

					Invoke(new Action(() =>
					{
						ToolStripProgressBarStatus.Visible = false;
					}));

					if (aProcess.ExitCode != 0)
					{
						throw new Exception("結合中にエラーが発生しました。");
					}
					mLogWriter.ShowLogMessage(TraceEventType.Information, "結合完了しました。");
				}
				catch (Exception oExcep)
				{
					mLogWriter.ShowLogMessage(TraceEventType.Error, "結合処理実行時エラー：\n" + oExcep.Message);
					mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
				}
				finally
				{
					Invoke(new Action(() =>
					{
						ButtonGo.Enabled = true;
						ToolStripProgressBarStatus.Visible = false;
					}));
				}
			});
		}

		// --------------------------------------------------------------------
		// LogWriter の設定
		// --------------------------------------------------------------------
		private void SetLogWriter()
		{
			mLogWriter = new LogWriter(MnktConstants.APP_ID);
			mLogWriter.ApplicationQuitToken = mClosingCancellationTokenSource.Token;
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "起動しました：" + MnktConstants.APP_NAME_J + " "
					+ MnktConstants.APP_VER + " ====================");
#if DEBUG
			mLogWriter.ShowLogMessage(TraceEventType.Verbose, "デバッグモード：" + Common.DEBUG_ENABLED_MARK);
#endif
		}

		// --------------------------------------------------------------------
		// MnktSettings の設定
		// --------------------------------------------------------------------
		private void SetMnktSettings()
		{
			mMnktSettings = new MnktSettings();
			mMnktSettings.Reload();

			// 終了時の状態を復元
			if (!mMnktSettings.DesktopBounds.IsEmpty)
			{
				Location = mMnktSettings.DesktopBounds.Location;
			}

			UpdateMp4box();
		}

		// --------------------------------------------------------------------
		// 入力元動画情報の入れ替え
		// --------------------------------------------------------------------
		private void SwapInputMovie(Label oLabelLhs, TextBox oTextBoxLhs, Label oLabelRhs, TextBox oTextBoxRhs)
		{
			String aTemp;

			// ラベル
			aTemp = oLabelLhs.Text;
			oLabelLhs.Text = oLabelRhs.Text;
			oLabelRhs.Text = aTemp;

			// テキストボックス
			aTemp = oTextBoxLhs.Text;
			oTextBoxLhs.Text = oTextBoxRhs.Text;
			oTextBoxRhs.Text = aTemp;
		}

		// --------------------------------------------------------------------
		// 追加のトラック名を合わせたトラック名を取得
		// 引数として指定できない文字は除去する
		// --------------------------------------------------------------------
		private String TrackName(String oBase, String oAdd)
		{
			if (String.IsNullOrEmpty(oAdd))
			{
				return oBase + " Vocal";
			}
			else
			{
				return oBase + " Vocal (" + oAdd.Replace('\"', '\'') + ")";
			}
		}

		// --------------------------------------------------------------------
		// 入力元動画の UI を更新
		// --------------------------------------------------------------------
		private void UpdateAllInputMovieUi()
		{
			UpdateInputMovieUiOn();
			UpdateInputMovieUiOn2();

			// Off2 の状態で Off の状態が変わるため、Off2 を先にやる
			UpdateInputMovieUiOff2();
			UpdateInputMovieUiOff();
		}

		// --------------------------------------------------------------------
		// 案内を更新
		// フォーカスの設定をするため、UpdateAllInputMovieUi() 実行後に呼ぶこと
		// --------------------------------------------------------------------
		private void UpdateGuide()
		{
			// 背景を元に戻す
			LabelInputMovieOn.BackColor = SystemColors.Control;
			ButtonBrowseInputMovieOn.UseVisualStyleBackColor = true;
			LabelInputMovieOff.BackColor = SystemColors.Control;
			ButtonBrowseInputMovieOff.UseVisualStyleBackColor = true;
			ButtonGo.UseVisualStyleBackColor = true;

			if (String.IsNullOrEmpty(LabelInputMovieOn.Text))
			{
				// オンボ登録を促す
				LabelGuide.Text = "オンボーカル動画を指定して下さい（ドラッグ＆ドロップも可能）。";
				LabelInputMovieOn.BackColor = Color.LightCyan;
				ButtonBrowseInputMovieOn.BackColor = Color.LightCyan;
				ButtonBrowseInputMovieOn.Focus();
			}
			else if (String.IsNullOrEmpty(LabelInputMovieOff.Text))
			{
				// オフボ登録を促す
				LabelGuide.Text = "オフボーカル動画を指定して下さい（ドラッグ＆ドロップも可能）。";
				LabelInputMovieOff.BackColor = Color.LightCyan;
				ButtonBrowseInputMovieOff.BackColor = Color.LightCyan;

				// フォーカスはオンボ追加トラック
				TextBoxTrackNameAddOn.Focus();
			}
			else
			{
				// 開始またはさらなる動画の追加
				LabelGuide.Text = "指定に問題がなければ「出力開始」ボタンをクリックして下さい。";
				ButtonGo.BackColor = Color.LightCyan;

				if (String.IsNullOrEmpty(LabelInputMovieOn2.Text))
				{
					LabelGuide.Text += "または、2 つ目のオンボーカル動画を追加することもできます。";

					// フォーカスはオフボ追加トラック
					TextBoxTrackNameAddOff.Focus();
				}
				else if (String.IsNullOrEmpty(LabelInputMovieOff2.Text))
				{
					LabelGuide.Text += "または、2 つ目のオフボーカル動画を追加することもできます。";

					// フォーカスはオンボ 2 追加トラック
					TextBoxTrackNameAddOn2.Focus();
				}
				else
				{
					// フォーカスはオフボ 2 追加トラック
					TextBoxTrackNameAddOff2.Focus();
				}
			}
		}

		// --------------------------------------------------------------------
		// 入力元動画（On）の UI を更新
		// --------------------------------------------------------------------
		private void UpdateInputMovieUiOn()
		{
			// 作業なし
		}

		// --------------------------------------------------------------------
		// 入力元動画（On2）の UI を更新
		// --------------------------------------------------------------------
		private void UpdateInputMovieUiOn2()
		{
			Boolean aEnabled = !String.IsNullOrEmpty(LabelInputMovieOn2.Text);

			if (aEnabled)
			{
				LabelTrackOn2.ForeColor = SystemColors.ControlText;
			}
			else
			{
				LabelTrackOn2.ForeColor = Color.DarkGray;
			}
			RadioButtonOn2.Enabled = aEnabled;
			if (!aEnabled && RadioButtonOn2.Checked)
			{
				// 無効なのにチェックされている場合は On にチェックを移す
				RadioButtonOn.Checked = true;
			}
			TextBoxTrackNameAddOn2.Enabled = aEnabled;
			ButtonUpOn2.Enabled = aEnabled;
			ButtonDownOn2.Enabled = aEnabled;
			ButtonClearOn2.Enabled = aEnabled;
		}

		// --------------------------------------------------------------------
		// 入力元動画（Off）の UI を更新
		// --------------------------------------------------------------------
		private void UpdateInputMovieUiOff()
		{
			ButtonDownOff.Enabled = IsOff2Enabled();
		}

		// --------------------------------------------------------------------
		// 入力元動画（Off2）の UI を更新
		// ButtonDownOff2 はいじらない
		// --------------------------------------------------------------------
		private void UpdateInputMovieUiOff2()
		{
			Boolean aEnabled = !String.IsNullOrEmpty(LabelInputMovieOff2.Text);

			if (aEnabled)
			{
				LabelTrackOff2.ForeColor = SystemColors.ControlText;
			}
			else
			{
				LabelTrackOff2.ForeColor = Color.DarkGray;
			}
			RadioButtonOff2.Enabled = aEnabled;
			if (!aEnabled && RadioButtonOff2.Checked)
			{
				// 無効なのにチェックされている場合は On にチェックを移す
				RadioButtonOff.Checked = true;
			}
			TextBoxTrackNameAddOff2.Enabled = aEnabled;
			ButtonUpOff2.Enabled = aEnabled;
			ButtonClearOff2.Enabled = aEnabled;
		}

		// --------------------------------------------------------------------
		// MP4Box.exe の場所を表示
		// --------------------------------------------------------------------
		private void UpdateMp4box()
		{
			LabelMp4boxPathSeed.Text = mMnktSettings.Mp4boxPathSeed;
		}

		// --------------------------------------------------------------------
		// 出力ファイル名を更新
		// --------------------------------------------------------------------
		private void UpdateOutputPath()
		{
			String aVideoPath = VideoPath();
			if (String.IsNullOrEmpty(aVideoPath))
			{
				TextBoxOutputMovie.Text = null;
			}
			else
			{
				TextBoxOutputMovie.Text = Path.GetDirectoryName(aVideoPath) + "\\" + Path.GetFileNameWithoutExtension(aVideoPath)
						+ "(On-Off)" + Path.GetExtension(aVideoPath);
			}
		}

		// --------------------------------------------------------------------
		// 映像使用の動画のパス
		// --------------------------------------------------------------------
		private String VideoPath()
		{
			if (RadioButtonOn.Checked)
			{
				return LabelInputMovieOn.Text;
			}
			else if (RadioButtonOn2.Checked)
			{
				return LabelInputMovieOn2.Text;
			}
			else if (RadioButtonOff.Checked)
			{
				return LabelInputMovieOff.Text;
			}
			else
			{
				return LabelInputMovieOff2.Text;
			}
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormMergeNicoKaraTracks_Load(object sender, EventArgs e)
		{
			try
			{
				Init();
				ClearAllInputMovie();
				UpdateAllInputMovieUi();
				UpdateGuide();
				UpdateOutputPath();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "起動時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormMergeNicoKaraTracks_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				// 終了時タスクキャンセル
				mClosingCancellationTokenSource.Cancel();

				// 終了時の状態（mClosingCancellationTokenSource を渡すとデータベース取得が止まるので渡さない）
				mMnktSettings.PrevLaunchPath = Application.ExecutablePath;
				mMnktSettings.PrevLaunchVer = MnktConstants.APP_VER;
				mMnktSettings.DesktopBounds = DesktopBounds;
				mMnktSettings.Save();

				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "終了しました：" + MnktConstants.APP_NAME_J + " "
						+ MnktConstants.APP_VER + " --------------------");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "終了時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormMergeNicoKaraTracks_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				e.Effect = DragDropEffects.None;

				if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					// ファイルのときは受け付ける
					e.Effect = DragDropEffects.Copy;
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ドラッグエンター時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormMergeNicoKaraTracks_DragDrop(object sender, DragEventArgs e)
		{
			try
			{
				String[] aDropFiles = (String[])e.Data.GetData(DataFormats.FileDrop, false);
				if (aDropFiles.Length == 0)
				{
					throw new Exception("ファイルが見つかりません。");
				}

				String aExtension = Path.GetExtension(aDropFiles[0]).ToLower();
				switch (aExtension)
				{
					case Common.FILE_EXT_MP4:
						FileDropMp4(aDropFiles[0]);
						break;
					case Common.FILE_EXT_EXE:
						FileDropExe(aDropFiles[0]);
						break;
					default:
						throw new Exception("未対応のファイル形式です：" + aExtension);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ドラッグ＆ドロップ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseMp4box_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileDialogMisc.Title = "MP4Box.exe の場所を指定";
				OpenFileDialogMisc.Filter = "実行ファイル|*" + Common.FILE_EXT_EXE + "|すべてのファイル|*.*";
				if (OpenFileDialogMisc.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				mMnktSettings.Mp4boxPathSeed = OpenFileDialogMisc.FileName;
				UpdateMp4box();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "MP4Box 参照時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseInputMovieOn_Click(object sender, EventArgs e)
		{
			// 例外捕捉は BrowseInputMovie() 内で実施
			BrowseInputMovie(LabelInputMovieOn, "オンボーカル");
		}

		private void ButtonBrowseInputMovieOn2_Click(object sender, EventArgs e)
		{
			// 例外捕捉は BrowseInputMovie() 内で実施
			BrowseInputMovie(LabelInputMovieOn2, "オンボーカル 2");
		}

		private void ButtonBrowseInputMovieOff_Click(object sender, EventArgs e)
		{
			// 例外捕捉は BrowseInputMovie() 内で実施
			BrowseInputMovie(LabelInputMovieOff, "オフボーカル");
		}

		private void ButtonBrowseInputMovieOff2_Click(object sender, EventArgs e)
		{
			// 例外捕捉は BrowseInputMovie() 内で実施
			BrowseInputMovie(LabelInputMovieOff2, "オフボーカル 2");
		}

		private void RadioButton_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateOutputPath();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "チェックボックスチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonUpOn2_Click(object sender, EventArgs e)
		{
			try
			{
				SwapInputMovie(LabelInputMovieOn2, TextBoxTrackNameAddOn2, LabelInputMovieOn, TextBoxTrackNameAddOn);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "オンボーカル 2 上へボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonUpOff_Click(object sender, EventArgs e)
		{
			try
			{
				if (IsOn2Enabled())
				{
					SwapInputMovie(LabelInputMovieOff, TextBoxTrackNameAddOff, LabelInputMovieOn2, TextBoxTrackNameAddOn2);
				}
				else
				{
					SwapInputMovie(LabelInputMovieOff, TextBoxTrackNameAddOff, LabelInputMovieOn, TextBoxTrackNameAddOn);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "オフボーカル上へボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonUpOff2_Click(object sender, EventArgs e)
		{
			try
			{
				SwapInputMovie(LabelInputMovieOff2, TextBoxTrackNameAddOff2, LabelInputMovieOff, TextBoxTrackNameAddOff);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "オフボーカル 2 上へボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDownOn_Click(object sender, EventArgs e)
		{
			try
			{
				if (IsOn2Enabled())
				{
					SwapInputMovie(LabelInputMovieOn, TextBoxTrackNameAddOn, LabelInputMovieOn2, TextBoxTrackNameAddOn2);
				}
				else
				{
					SwapInputMovie(LabelInputMovieOn, TextBoxTrackNameAddOn, LabelInputMovieOff, TextBoxTrackNameAddOff);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "オンボーカル下へボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDownOn2_Click(object sender, EventArgs e)
		{
			try
			{
				SwapInputMovie(LabelInputMovieOn2, TextBoxTrackNameAddOn2, LabelInputMovieOff, TextBoxTrackNameAddOff);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "オンボーカル 2 下へボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDownOff_Click(object sender, EventArgs e)
		{
			try
			{
				if (!IsOff2Enabled())
				{
					return;
				}

				SwapInputMovie(LabelInputMovieOff, TextBoxTrackNameAddOff, LabelInputMovieOff2, TextBoxTrackNameAddOff2);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "オフボーカル下へボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonClearOn_Click(object sender, EventArgs e)
		{
			try
			{
				ClearInputMovieOn();
				UpdateAllInputMovieUi();
				UpdateGuide();
				UpdateOutputPath();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "オンボーカルクリアボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonClearOn2_Click(object sender, EventArgs e)
		{
			try
			{
				ClearInputMovieOn2();
				UpdateAllInputMovieUi();
				UpdateGuide();
				UpdateOutputPath();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "オンボーカル 2 クリアボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonClearOff_Click(object sender, EventArgs e)
		{
			try
			{
				ClearInputMovieOff();
				UpdateAllInputMovieUi();
				UpdateGuide();
				UpdateOutputPath();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "オフボーカルクリアボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonClearOff2_Click(object sender, EventArgs e)
		{
			try
			{
				ClearInputMovieOff2();
				UpdateAllInputMovieUi();
				UpdateGuide();
				UpdateOutputPath();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "オフボーカル 2 クリアボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private async void ButtonGo_Click(object sender, EventArgs e)
		{
			try
			{
				Check();
				await Merge();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "出力開始ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonClearAll_Click(object sender, EventArgs e)
		{
			try
			{
				ClearAllInputMovie();
				UpdateAllInputMovieUi();
				UpdateGuide();
				UpdateOutputPath();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "全クリアボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormMergeNicoKaraTracks ___END___
}
// namespace MergeNicoKaraTracks.Forms ___END___
