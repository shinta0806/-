// ============================================================================
// 
// Mnkt の設定を管理
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergeNicoKaraTracks.SharedMisc
{
	// 設定の保存場所を Application.UserAppDataPath 配下にする
	[SettingsProvider(typeof(ApplicationNameSettingsProvider))]
	public class MnktSettings : ApplicationSettingsBase
	{
		// ====================================================================
		// public プロパティ
		// ====================================================================

		// --------------------------------------------------------------------
		// 設定
		// --------------------------------------------------------------------

		// MP4Box のパス（相対または絶対）
		private const String KEY_NAME_MP4BOX_PATH_SEED = "Mp4boxPathSeed";
		[UserScopedSetting]
		[DefaultSettingValue(@".\" + MnktConstants.FILE_NAME_MP4BOX)]
		public String Mp4boxPathSeed
		{
			get => (String)this[KEY_NAME_MP4BOX_PATH_SEED];
			set => this[KEY_NAME_MP4BOX_PATH_SEED] = value;
		}

		// --------------------------------------------------------------------
		// 終了時の状態
		// --------------------------------------------------------------------

		// 前回起動時のバージョン
		private const String KEY_NAME_PREV_LAUNCH_VER = "PrevLaunchVer";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public String PrevLaunchVer
		{
			get => (String)this[KEY_NAME_PREV_LAUNCH_VER];
			set => this[KEY_NAME_PREV_LAUNCH_VER] = value;
		}

		// 前回起動時のパス
		private const String KEY_NAME_PREV_LAUNCH_PATH = "PrevLaunchPath";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public String PrevLaunchPath
		{
			get => (String)this[KEY_NAME_PREV_LAUNCH_PATH];
			set => this[KEY_NAME_PREV_LAUNCH_PATH] = value;
		}

		// ウィンドウ位置
		private const String KEY_NAME_DESKTOP_BOUNDS = "DesktopBounds";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public Rectangle DesktopBounds
		{
			get => (Rectangle)this[KEY_NAME_DESKTOP_BOUNDS];
			set => this[KEY_NAME_DESKTOP_BOUNDS] = value;
		}

	}
	// public class MnktSettings ___END___
}
// namespace MergeNicoKaraTracks.SharedMisc ___END___
